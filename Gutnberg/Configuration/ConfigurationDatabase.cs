using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gutenberg.Configuration
{
    public class ConfigurationDatabase : IConfiguration
    {
        public string userName         = String.Empty;
        public string userPwd          = String.Empty;
        public string connectionString = String.Empty;
    }
}
