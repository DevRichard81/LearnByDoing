using System.Collections.Concurrent;
using System.IO.Pipes;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
using Project_Gutenberg.GutenbergShared;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.Types.Pipe
{
    public class PipeConnectionClient : IConnectionType
    {
        private ConfigurationPipes? Configuration { get; set; }
        private NamedPipeClientStream connection;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Init(IConfiguration newConfiguration, IGutenberg gutenberg)
        {
            Configuration = newConfiguration as ConfigurationPipes;
            gutenberg.gutenbergThreads.AddThread(Read, gutenberg.statistic, gutenberg.errorObject, gutenberg.gutenbergBuffers);
            gutenberg.gutenbergThreads.AddThread(Write, gutenberg.statistic, gutenberg.errorObject, gutenberg.gutenbergBuffers);
        }

        public void Start()
        {
            connection = new NamedPipeClientStream(
                        Configuration.serverName,
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

        public bool HasConnection()
        {
            if(connection.IsConnected) 
            {
                return true; 
            }

            Close();
            return false;
        }

        public void Read(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
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
                        buffer.AddReceivedMessage(reciveBuffer);
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

        public void Write(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
        {
            if (connection.IsConnected)
            {
                if (buffer.BufferSend.TryDequeue(out byte[]? sendBuffer))
                {
                    try
                    {
                        connection.Write(sendBuffer, 0, sendBuffer.Length);
                        connection.WaitForPipeDrain();
                    }
                    catch (Exception ex)
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
