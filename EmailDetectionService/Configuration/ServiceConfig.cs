using System;
using System.Collections.Generic;
using System.Text;

namespace EmailDetectionService.Configuration
{
    public class ServiceConfig
    {
        public string ServiceName { get; set; }
        public string ObservableFolderPath { get; set; }
        public int FolderCheckTimeout { get; set; }
    }
}
