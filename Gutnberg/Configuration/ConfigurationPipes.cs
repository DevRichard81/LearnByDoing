using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gutenberg.Configuration
{
    public class ConfigurationPipes : IConfiguration
    {
        public string pipeName;
        public int numThreads;
        public PipeDirection pipeDirection;
    }
}
