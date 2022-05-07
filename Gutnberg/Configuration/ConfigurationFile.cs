using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Gutenberg.Configuration
{
    public class ConfigurationFile : IConfiguration
    {
        public string baseDirectory    = String.Empty;
        public string incomeDirectory  = String.Empty;
        public string outcomeDirectory = String.Empty;
        public string readedDirectory  = String.Empty;
                                       
        public string sendFilePrefix     = String.Empty;
        public string sendFileDateFormat = String.Empty;
        public string sendFileSuffix     = String.Empty;
        public string sendFileExtension  = String.Empty;

        public ConfigurationFile SetDirectorys(string baseDirectoryPath, string incomeDirectoryName, string outcomeDirectoryName, string readDirectoryName)
        {
            this.baseDirectory = baseDirectoryPath;
            this.incomeDirectory = incomeDirectoryName;
            this.outcomeDirectory = outcomeDirectoryName;
            this.readedDirectory = readDirectoryName;
            return this;
        }

        public ConfigurationFile SetSendFile(string prefix, string dataFormat, string suffix, string extension)
        {
            this.sendFilePrefix = prefix;
            this.sendFileDateFormat = dataFormat;
            this.sendFileSuffix = suffix;
            this.sendFileExtension = extension;
            return this;
        }
    }
}
