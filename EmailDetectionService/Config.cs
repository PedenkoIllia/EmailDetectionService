using log4net;
using Microsoft.Extensions.Configuration;
using System;

namespace EmailDetectionService
{
    internal class Config
    {
        private static readonly ILog log = LogManager.GetLogger("Configuration");

        private static IConfigurationRoot AppConfiguration { get; }

        static Config()
        {
            try
            {
                AppConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
            }
            catch (Exception e)
            {
                log.Error("Problem with appsettings.json file. ", e);
            }
        }

        public static string ServiceName 
        { 
            get => AppConfiguration.GetValue<string>("ServiceConfig:ServiceName", "EmailDetectionService"); 
        }
        public static string ObservableFolderPath 
        {
            get => AppConfiguration.GetValue<string>("ServiceConfig:ObservableFolderPath", "."); 
        }
        public static int FolderCheckTimeout 
        { 
            get => AppConfiguration.GetValue<int>("ServiceConfig:FolderCheckTimeout", 1000); 
        }
        public static int MaxProcessingFilesCount 
        {
            get => AppConfiguration.GetValue<int>("ServiceConfig:MaxProcessingFilesCount", 10);
        }
        public static int DatabaseSyncDelay
        {
            get => AppConfiguration.GetValue<int>("ServiceConfig:DatabaseSyncDelay", 5000);
        }

        public static string GetConnectionString(string connectionName)
        {
            return AppConfiguration.GetSection("ConnectionStrings")[connectionName];
        }
    }
}
