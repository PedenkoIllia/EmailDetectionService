using EmailDetectionService.DAL;
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
        private object processinsgFilesLock = new object();

        public EmailProcessor(IDataSourceDetectedMessages dataSource, int maxThreadCount = 10)
        {
            this.dataSource = dataSource;
            Controller = new JobController(maxThreadCount);
        }

        public void StartProcessMessages()
        {
            Controller.Start();
            do
            {
                string folderPath = Config.ObservableFolderPath;
                if (Directory.Exists(folderPath))
                {
                    List<string> newEmailFiles;

                    lock (processinsgFilesLock)
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
                            lock (processinsgFilesLock)
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
                            lock (processinsgFilesLock)
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
                MessageModel msg = GetMessageModelFromFile(emailFile);
                bool result = dataSource.InsertMessage(msg);
                
                if (result)
                    RemoveFile(emailFile);

                return result;
            }
            catch (Exception ex)
            {
                _log.Error("Problem with file: " + emailFile, ex);
                SendToFailed(emailFile);
            }

            return false;
        }

        public MessageModel GetMessageModelFromFile(string emailFile)
        {
            var messageHeaders = HeaderList.Load(emailFile);
            MessageModel message = new MessageModel()
            {
                Message_ID = messageHeaders[HeaderId.MessageId],
                Subject = messageHeaders[HeaderId.Subject],
                From = messageHeaders[HeaderId.From],
                To = messageHeaders[HeaderId.To],
                Date = messageHeaders[HeaderId.Date]
        };

            if (message.From == null || message.Date == null)
                throw new FormatException();

            if (String.IsNullOrEmpty(message.Message_ID))
                message.Message_ID = "<" + Guid.NewGuid().ToString() + message.From.Substring(message.From.IndexOf('@')) + ">";

            return message;
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

        public void SendToFailed(string message)
        {
            string fileName = message.Substring(message.LastIndexOf('\\'));
            string failedDirectory = message.Remove(message.Length - fileName.Length) + @"\FailedFiles";

            if (!Directory.Exists(failedDirectory))
                Directory.CreateDirectory(failedDirectory);
            try
            {
                File.Move(message, failedDirectory + fileName, true);
            }
            catch (Exception ex)
            {
                _log.Error("Can't move file to failed! " + ex.Message);
            }
        }

        public void StopProcessMessages()
        {
            Controller.Stop();
        }
    }
}
