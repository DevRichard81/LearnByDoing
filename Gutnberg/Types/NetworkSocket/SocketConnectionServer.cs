using System.Collections.Concurrent;
using System.Net.Sockets;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
using Project_Gutenberg.GutenbergShared;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.Types.NetworkSocket
{
    public class SocketConnectionServer : IConnectionType
    {
        private ConfigurationSocket? Configuration { get; set; }
        private Thread listener;
        private List<Socket> connected;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationSocket;
            connected = new List<Socket>();
            SocketConnectionShare.CreateSocket(Configuration);
            SocketConnectionShare.Listenner(Configuration);            
        }

        public void Start()
        {
            listener = new Thread(() =>
            {
                Socket clientSocket = Configuration.socket.Accept();
                connected.Add(clientSocket);
            });
            listener.Start();
        }

        public void Close()
        {
            SocketConnectionShare.Disconnect(Configuration);
        }

        public void Read(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
        {
            int byteRec = SocketConnectionShare.Read(
                Configuration, 
                ref statisticOfFunction,
                connected.ToArray(),
                out List<byte[]> BufferList);

            if (byteRec > 0)
            {
                foreach(var item in BufferList)
                    buffer.AddReceivedMessage(item);
            }
            Thread.Sleep(1);
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
        {
            byte[]? sendBuffer;
            if (buffer.BufferSend.TryDequeue(out sendBuffer))
            {
                SocketConnectionShare.Write(ref statisticOfFunction, connected.ToArray(), sendBuffer);
            }
        }
    }
}
