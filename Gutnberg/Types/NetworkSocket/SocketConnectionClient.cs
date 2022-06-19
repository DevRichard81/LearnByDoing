using System.Collections.Concurrent;
using System.Threading;
using Project_Gutenberg.GutenbergShared;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistic;
using System.Net.Sockets;

namespace Project_Gutenberg.Types.NetworkSocket
{
    public class SocketConnectionClient : IConnectionType
    {
        private ConfigurationSocket? Configuration { get; set; }
        private byte[] receiveBuffer;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }
        public CancellationTokenSource? tks;

        public void Init(IConfiguration newConfiguration, IGutenberg gutenberg)
        {
            Console.WriteLine("SocketConnectionClient:Init");
            Configuration = newConfiguration as ConfigurationSocket;            
            SocketConnectionShare.CreateSocket(Configuration);
            SocketConnectionShare.Connect(Configuration);
            //
            receiveBuffer = new byte[Configuration.reciveBufferSize];
            tks = gutenberg.gutenbergThreads.cancellationTokenSource;
            gutenberg.gutenbergThreads.AddThread(Read, gutenberg.statistic, gutenberg.errorObject, gutenberg.gutenbergBuffers);
            gutenberg.gutenbergThreads.AddThread(Write, gutenberg.statistic, gutenberg.errorObject, gutenberg.gutenbergBuffers);
        } 

        public void Close()
        {
            Console.WriteLine("SocketConnectionClient:Close");
            SocketConnectionShare.Disconnect(Configuration);
            tks?.Cancel();
        }

        public bool HasConnection()
        {
            if(Configuration.socket != null && Configuration.socket.Connected)
                    return true;

            Close();
            Configuration.socket = null;
            return false;
        }

        /// <summary>
        /// Not used for FileConnction
        /// </summary>
        public void Start() { }

        public void Read(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
        {
            var config = (ConfigurationSocket)Configuration;
            int byteRecv = SocketConnectionShare.Read(ref statisticOfFunction, config.socket, ref receiveBuffer);
            if(byteRecv > 0)
            {
                buffer.AddReceivedMessage(receiveBuffer[0..byteRecv]);
            }
            Thread.Sleep(1);
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
        {
            if (HasConnection())
            {
                byte[]? sendBuffer;
                if (buffer.BufferSend.TryDequeue(out sendBuffer))
                {
                    SocketConnectionShare.Write(ref statisticOfFunction, Configuration.socket, sendBuffer);
                }
            }
            else
            {
                ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Error, 0, "Try to send message but connection was closed", "Write");
                Thread.Sleep(100);
            }
        }
    }
}
