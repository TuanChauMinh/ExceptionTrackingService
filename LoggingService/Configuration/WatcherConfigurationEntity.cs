using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingService.Configuration
{
    public class WatcherConfigurationEntity
    {
        public string FolderDirectory { get; set; }
        public string LogFileName { get; set; }
        public string SlackHookUrl { get; set; }
    }
}
