using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gutenberg.Types.Pipe
{
    internal class PipeStateObject
    {
        public NamedPipeServerStream namePipeServerStream;

        internal void BeginConnect()
        {
            if(namePipeServerStream == null)
            {
                return;
            }
            namePipeServerStream.BeginWaitForConnection(EndConnect, this);
        }

        internal void EndConnect(IAsyncResult iar)
        {
            var pipeStateObject = (PipeStateObject)iar.AsyncState;
            pipeStateObject.namePipeServerStream.EndWaitForConnection(iar);
            pipeStateObject.namePipeServerStream.ReadMode = PipeTransmissionMode.Byte;
        }
    }
}
