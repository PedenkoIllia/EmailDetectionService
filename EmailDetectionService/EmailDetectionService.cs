using EmailDetectionService.DAL;
using EmailDetectionService.Models;
using EmailDetectionService.Processors;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace EmailDetectionService
{
    public class EmailDetectionService
    {
        private static readonly ILog _log = LogManager.GetLogger("EmailDetectionService");

        private IDataSourceDetectedMessages dataSource;
        private IEmailProcessing processor;
        
        public void Start()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            dataSource = new DetectedMessageRepository();
            processor = new EmailProcessor(dataSource);

            _log.Info("Service started");
            Thread thread = new Thread(new ThreadStart(processor.StartProcessMessages));
            thread.Start();
        }

        public void Stop()
        {
            _log.Info("Service stopped");
            processor.StopProcessMessages();
        }

        public void RunAsConsole(string serviceName)
        {
            Console.WriteLine($"----Service {serviceName} started in console mode----");

            dataSource = new DetectedMessageRepository();

            Console.WriteLine($"In this mode you can see last 100 records about detected emails");
            Console.WriteLine("Press Enter to get the last 100 records");
            Console.ReadLine();
            
            try
            {
                var messages = dataSource.GetMessages(5);
                Console.WriteLine("Detected emails:");
                foreach (MessageModel msg in messages)
                {
                    Console.WriteLine(msg);
                }

                Console.Write("Press Enter to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem with database: " + ex.Message);
                Console.ReadLine();
            }
        }
    }
}
