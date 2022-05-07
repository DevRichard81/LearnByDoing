using System.IO.Pipes;

namespace Project_Gutenberg.Types.Pipe
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
