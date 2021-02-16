using EmailDetectionService.Configuration;
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

        public static ServiceConfig ServiceConfig 
        {
            get 
            {
                return AppConfiguration.GetSection("ServiceConfig").Get<ServiceConfig>();
            }
        }

        public static string GetConnectionString(string connectionName)
        {
            return AppConfiguration.GetSection("ConnectionStrings")[connectionName];
        }
    }
}
