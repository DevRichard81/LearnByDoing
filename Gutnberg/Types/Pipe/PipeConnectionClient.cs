using System.Collections.Concurrent;
using System.IO.Pipes;
using Gutenberg.Configuration;
using Gutenberg.Error;
using Gutenberg.Statistic;

namespace Gutenberg.Types.Pipe
{
    public class PipeConnectionClient : IConnectionType
    {
        private ConfigurationPipes? Configuration { get; set; }
        private NamedPipeClientStream connection;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationPipes;
        }

        public void Start()
        {
            connection = new NamedPipeClientStream(
                        Configuration.ServerName,
                        Configuration.pipeName,
                        Configuration.pipeDirection,
                        PipeOptions.Asynchronous);            
            connection.Connect();
            connection.ReadMode = PipeTransmissionMode.Byte;
        }

        public void Close()
        {
            connection.Close();
            connection.Dispose();
            Thread.Sleep(500);
            connection = null;
        }

        public void Read(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            byte[] reciveBuffer = new byte[Configuration.reciveBufferSize];

            if(connection.IsConnected && connection.CanRead)
            {
                try
                {
                    Array.Clear(reciveBuffer);
                    int byteRecv = connection.Read(reciveBuffer, 0, Configuration.reciveBufferSize);
                    if (byteRecv > 0)
                    {
                        buffer.Enqueue(reciveBuffer);
                        statisticOfFunction.handelDataLength += (uint)byteRecv;
                        statisticOfFunction.handelMessage += 1;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(1);
            }
            else
            {
                if (!connection.CanRead)
                {
                    ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Error, 0, "Connection not supossed to read from it", "Read");
                }

                Thread.Sleep(100);
            }
            statisticOfFunction.type = StatisticOfFunction.Type.Incoming;            
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            if (connection.IsConnected)
            {
                byte[]? sendBuffer;
                if (buffer.TryDequeue(out sendBuffer))
                {
                    try
                    {
                        connection.Write(sendBuffer, 0, sendBuffer.Length);
                        connection.WaitForPipeDrain();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    statisticOfFunction.handelDataLength += (uint)sendBuffer.Length;
                    statisticOfFunction.handelMessage += 1;
                }                
                Thread.Sleep(1);
            }
            else
            {
                ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Error, 0, "Try to send message but connection was closed", "Write");
                Thread.Sleep(100);
            }
            statisticOfFunction.type = StatisticOfFunction.Type.Outcoming;
        }
    }
}
