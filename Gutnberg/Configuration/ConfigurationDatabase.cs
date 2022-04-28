using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gutenberg.Configuration
{
    public class ConfigurationDatabase : IConfiguration
    {
        public string userName;
        public string userPwd;
        public string connectionString;
    }
}
