using EmailDetectionService.DAL;
using log4net;
using MimeKit;
using System;
using System.Collections.Generic;
using EmailDetectionService.Models;
using System.IO;
using System.Threading;

namespace EmailDetectionService.Processors
{
    public class EmailProcessor : IEmailProcessing
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(EmailProcessor));
        private readonly IDataSourceDetectedMessages dataSource;

        private bool isRunning;

        public EmailProcessor(IDataSourceDetectedMessages dataSource)
        {
            this.dataSource = dataSource;
        }

        public void StartProcessMessages()
        {
            isRunning = true;

            while (isRunning)
            {
                var sConfig = Config.ServiceConfig;

                string folderPath = sConfig.ObservableFolderPath;
                if (!String.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    var messages = new List<string>(Directory.GetFiles(folderPath, "*.eml"));
                    if (messages.Count > 0)
                        ProcessMessagesQueue(messages);
                }
                else
                {
                    _log.Error("The watched folder is not specified correctly. Check service settings (appsettings.json)!");
                    Thread.Sleep(sConfig.FolderCheckTimeout*2);
                }

                Thread.Sleep(sConfig.FolderCheckTimeout);
            }
        }

        public void ProcessMessagesQueue(IEnumerable<string> emailFiles)
        {
            foreach (string emailFile in emailFiles)
            {
                bool isSuccesfull = ProcessMessage(emailFile);
                if (!isSuccesfull && File.Exists(emailFile))
                {
                    try
                    {
                        SendToFailed(emailFile);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Cant move failed file: " + emailFile, ex);
                    }

                }
            }
        }

        public bool ProcessMessage(string emailFile)
        {
            try
            {
                MessageModel msg = GetMessageModelFromFile(emailFile);

                dataSource.InsertMessage(msg);
                File.Delete(emailFile);

                return true;
            }
            catch (FormatException ex)
            {
                _log.Error("Incorrect .eml file format or corrupted file: " + emailFile, ex);
            }
            catch (Exception ex)
            {
                _log.Error("Problem with file: " + emailFile, ex);
            }

            return false;
        }

        public MessageModel GetMessageModelFromFile(string emailFile)
        {
            var messageHeaders = HeaderList.Load(emailFile);
            MessageModel message = new MessageModel();
            message.Message_ID = messageHeaders[HeaderId.MessageId];
            message.Subject = messageHeaders[HeaderId.Subject];
            message.From = messageHeaders[HeaderId.From];
            message.To = messageHeaders[HeaderId.To];
            string date = messageHeaders[HeaderId.Date];

            if (message.Subject == null && message.From == null && message.To == null && date == null)
                throw new FormatException();

            if (date != null)
                message.Date = DateTime.Parse(date);

            if (String.IsNullOrEmpty(message.Message_ID))
                message.Message_ID = "<" + Guid.NewGuid().ToString() + message.From.Substring(message.From.IndexOf('@')) + ">";

            return message;
        }

        public void SendToFailed(string message)
        {
            string fileName = message.Substring(message.LastIndexOf('\\'));
            string failedDirectory = message.Remove(message.Length - fileName.Length) + @"\FailedFiles";

            if (!Directory.Exists(failedDirectory))
                Directory.CreateDirectory(failedDirectory);

            File.Move(message, failedDirectory + fileName);
        }

        public void StopProcessMessages()
        {
            isRunning = false;
        }
    }
}
