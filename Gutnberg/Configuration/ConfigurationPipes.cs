using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Gutenberg.Configuration
{
    public class ConfigurationPipes : IConfiguration
    {
        public string serverName                         = ".";
        public string pipeName                           = String.Empty;
        public int numThreads                            = 1;
        public PipeDirection pipeDirection               = PipeDirection.InOut;
        public PipeTransmissionMode pipeTransmissionMode = PipeTransmissionMode.Byte;
        public int reciveBufferSize                      = 1024;

        public ConfigurationPipes SetServerName(string serverName)
        {
            this.serverName = serverName;
            return this;
        }
        public ConfigurationPipes SetPipeName(string PipeName)
        {
            this.pipeName = PipeName;
            return this;
        }
        public ConfigurationPipes SetNumThreads(int numThreads)
        {
            this.numThreads = numThreads;
            return this;
        }
        public ConfigurationPipes SetPipeDirections(PipeDirection pipeDirection)
        {
            this.pipeDirection = pipeDirection;
            return this;
        }
    }
}
