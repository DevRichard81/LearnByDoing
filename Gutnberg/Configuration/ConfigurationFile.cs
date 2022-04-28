using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gutenberg.Configuration
{
    public class ConfigurationFile : IConfiguration
    {
        public string baseDirectory;
        public string incomeDirectory;
        public string outcomeDirectory;
        public string errorDirectory;

        public string sendFilePrefix;
        public string sendFileDateFormat;
        public string sendFileSuffix;
        public string sendFileExtension;
    }
}
