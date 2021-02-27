using EmailDetectionService.DAL;
using EmailDetectionService.Helper;
using EmailDetectionService.Models;
using log4net;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace EmailDetectionService.Processors
{
    public class EmailProcessor : IEmailProcessor
    {
        private static readonly ILog _log = LogManager.GetLogger("EDS.EmailProcessor");
        private readonly IDataSourceDetectedMessages dataSource;
        public JobController Controller;

        private readonly List<string> EmailFilesInProcess = new List<string>();
        private object processingFilesLock = new object();

        private readonly List<MessageObject> ProcessedMessages = new List<MessageObject>();
        private object processedMessagesLock = new object();

        public EmailProcessor(IDataSourceDetectedMessages dataSource)
        {
            this.dataSource = dataSource;
            Controller = new JobController(Config.MaxProcessingFilesCount);
        }

        public void StartProcessMessages()
        {
            Controller.Start();
            ProcessNotWritedMessages();
            Controller.StartAdditionalProcess(() => DelayedDatabaseWrite());
            do
            {
                string folderPath = Config.ObservableFolderPath;
                if (Directory.Exists(folderPath))
                {
                    List<string> newEmailFiles;

                    lock (processingFilesLock)
                    {
                        newEmailFiles = Directory.GetFiles(folderPath, "*.eml").ToList()
                                            .Except(EmailFilesInProcess)
                                            .ToList();
                    }
                    
                    if (newEmailFiles.Count > 0)
                        ProcessMessagesQueue(newEmailFiles);
                }
                else
                {
                    _log.Error("The watched folder is not specified correctly. Check service settings (appsettings.json)!");
                    Thread.Sleep(Config.FolderCheckTimeout * 2);
                }

                Thread.Sleep(Config.FolderCheckTimeout);
            }
            while(Controller.IsRunning);
        }

        public void ProcessMessagesQueue(IEnumerable<string> emailFiles)
        {
            foreach (string emailFile in emailFiles)
            {

                if (File.Exists(emailFile))
                {
                    Controller.StartNewProcess(() =>
                    {
                        try
                        {
                            lock (processingFilesLock)
                            {
                                EmailFilesInProcess.Add(emailFile);
                            }

                            if (ProcessMessage(emailFile))
                                _log.Info("Message processed: " + emailFile);
                            else
                                _log.Warn("Message not processed: " + emailFile);
                        }
                        finally
                        {
                            lock (processingFilesLock)
                            {
                                EmailFilesInProcess.Remove(emailFile);
                            }
                        }
                    });
                }
                else
                    _log.Info("File does not exists anymore: " + emailFile);
            }
        }

        public bool ProcessMessage(string emailFile)
        {
            try
            {
                MessageModel msgModel = MessageParser.GetMessageModelFromFile(emailFile);
                MessageObject message = new MessageObject { Model = msgModel, Path = emailFile };
                lock(processedMessagesLock)
                    ProcessedMessages.Add(message);

                message.Path = SendToSubfolder(emailFile, "ProcessedFiles");

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Problem with file: " + emailFile, ex);
                SendToSubfolder(emailFile, "FailedFiles");
            }

            return false;
        }

        private void ProcessNotWritedMessages()
        {
            string processedDirectory = Config.ObservableFolderPath + @"\ProcessedFiles";
            if (Directory.Exists(processedDirectory))
            {
                foreach (string file in Directory.GetFiles(processedDirectory, "*.eml").ToList())
                {
                    try
                    {
                        MessageModel msgModel = MessageParser.GetMessageModelFromFile(file);
                        ProcessedMessages.Add(new MessageObject { Model = msgModel, Path = file });
                    }
                    catch { }
                }
            }
        }

        private void DelayedDatabaseWrite()
        {
            while (Controller.IsRunning)
            {
                Thread.Sleep(Config.DatabaseSyncDelay);
                List<string> filePathList = new List<string>();
                bool result;

                lock (processedMessagesLock)
                {
                    List<MessageModel> processedModels = ProcessedMessages.Select(x => x.Model).ToList();
                    filePathList = ProcessedMessages.Select(x => x.Path).ToList();
                    if (processedModels.Count > 0)
                    {
                        result = dataSource.InsertMessagesScope(processedModels);
                        if(result)
                        {
                            ProcessedMessages.Clear();
                        }
                    }
                }

                foreach (string file in filePathList)
                {
                    _log.Info("Message writed into database: " + file);
                    RemoveFile(file);
                }
            }
        }

        public void RemoveFile(string emailFile)
        {
            try
            {
                File.Delete(emailFile);
            }
            catch (Exception ex)
            {
                _log.Error("Can't remove file! " + ex.Message);
            }
        }

        public string SendToSubfolder(string filePath, string subFolder)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf('\\'));
            string subDirectory = filePath.Remove(filePath.Length - fileName.Length) + '\\' + subFolder;
            string resultFilePath = subDirectory + fileName;

            if (!Directory.Exists(subDirectory))
                Directory.CreateDirectory(subDirectory);
            try
            {
                File.Move(filePath, resultFilePath, true);
            }
            catch (Exception ex)
            {
                _log.Error("Can't move file to subdirectory" + subFolder + "! " + ex.Message);
                return filePath;
            }

            return resultFilePath;
        }

        public void StopProcessMessages()
        {
            Controller.Stop();
        }
    }
}
