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
        public string ServerName = ".";
        public string pipeName;
        public int numThreads;
        public PipeDirection pipeDirection = PipeDirection.InOut;
        public PipeTransmissionMode pipeTransmissionMode = PipeTransmissionMode.Byte;
        public int reciveBufferSize = 1024;
    }
}
