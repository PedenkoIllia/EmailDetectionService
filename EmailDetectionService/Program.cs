using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using Topshelf;

namespace EmailDetectionService
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        public static IConfiguration AppConfiguration { get; set; }

        static Program()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        static void Main(string[] args)
        {
            string serviceName = Config.ServiceConfig.ServiceName ?? "EmailDetectionService";
            bool isService = true;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower()) 
                {
                    case "-n":
                    case "--name":
                        if(args.Length > i+1)
                            serviceName = args[++i];
                        break;

                    case "-c":
                    case "--console":
                        isService = false;
                        break;
                }
            }

            try
            {
                if (isService)
                    HostFactory.Run(hostConfig =>
                    {
                        hostConfig.Service<EmailDetectionService>(serviceConfig =>
                        {
                            serviceConfig.ConstructUsing(() => new EmailDetectionService());
                            serviceConfig.WhenStarted(s => s.Start());
                            serviceConfig.WhenStopped(s => s.Stop());
                        });
                        hostConfig.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(10)));
                        hostConfig.SetServiceName(serviceName);
                        hostConfig.StartAutomatically();
                    });
                else
                {
                    var service = new EmailDetectionService();
                    service.RunAsConsole(serviceName);
                }
            }
            catch (Exception e)
            {
                log.Error("!!!Can't start the service!!!" + Environment.NewLine, e);
            }
        }
    }
}
