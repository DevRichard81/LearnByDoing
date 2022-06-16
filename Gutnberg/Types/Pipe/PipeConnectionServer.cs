using System.Collections.Concurrent;
using System.IO.Pipes;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
using Project_Gutenberg.GutenbergShared;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.Types.Pipe
{
    public class PipeConnectionServer : IConnectionType
    {
        private ConfigurationPipes? Configuration { get; set; }        
        private List<PipeStateObject> connections;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationPipes;
            connections = new List<PipeStateObject>();
        }

        public void Start()
        {
            for (int idx=0; idx < Configuration.numThreads; idx++)
            {
                PipeStateObject pipeStateObject = new()
                {
                    namePipeServerStream = new NamedPipeServerStream(
                    Configuration.pipeName,
                    Configuration.pipeDirection,
                    Configuration.numThreads,
                    Configuration.pipeTransmissionMode,
                    PipeOptions.Asynchronous)
                };
                pipeStateObject.BeginConnect();                
                connections.Add(pipeStateObject);
            }
        }

        public void Close()
        {
            foreach(var itm in connections)
            {
                if (itm.namePipeServerStream.IsConnected)
                {
                    itm.namePipeServerStream.Flush();
                    itm.namePipeServerStream.Disconnect();
                    Thread.Sleep(10);
                }
                itm.namePipeServerStream.Close();            
            }
            Thread.Sleep(500);
            connections.Clear();
        }

        public void Read(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
        {
            byte[] reciveBuffer = new byte[Configuration.reciveBufferSize];
            
            foreach (var itm in connections)
            {
                if (itm.namePipeServerStream.IsConnected)
                {
                    Array.Clear(reciveBuffer);
                    int byteRecv = itm.namePipeServerStream.Read(reciveBuffer, 0, Configuration.reciveBufferSize);
                    if (byteRecv > 0)
                    {
                        buffer.AddReceivedMessage(reciveBuffer);
                        statisticOfFunction.handelDataLength += (uint)byteRecv;
                        statisticOfFunction.handelMessage += 1;
                    }
                }
            }
            statisticOfFunction.type = StatisticOfFunction.Type.Incoming;
            Thread.Sleep(1);
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
        {
            byte[]? sendBuffer;
            bool wasSendOnce = false;
            if (buffer.BufferSend.TryDequeue(out sendBuffer))
            {
                foreach (var itm in connections)
                {
                    if (itm.namePipeServerStream.IsConnected)
                    {
                        itm.namePipeServerStream.Write(sendBuffer, 0, sendBuffer.Length);
                        itm.namePipeServerStream.WaitForPipeDrain();
                        wasSendOnce = true;
                    }
                }
                if (wasSendOnce) 
                {
                    statisticOfFunction.handelDataLength += (uint)sendBuffer.Length;
                    statisticOfFunction.handelMessage += 1;
                }
            }
            statisticOfFunction.type = StatisticOfFunction.Type.Outcoming;
            Thread.Sleep(1);
        }
    }
}
