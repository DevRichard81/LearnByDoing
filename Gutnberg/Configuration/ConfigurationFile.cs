using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gutenberg.Configuration
{
    public class ConfigurationFile : IConfiguration
    {
        public string baseDirectory    = String.Empty;
        public string incomeDirectory  = String.Empty;
        public string outcomeDirectory = String.Empty;
        public string errorDirectory   = String.Empty;
                                       
        public string sendFilePrefix     = String.Empty;
        public string sendFileDateFormat = String.Empty;
        public string sendFileSuffix     = String.Empty;
        public string sendFileExtension  = String.Empty;
    }
}
