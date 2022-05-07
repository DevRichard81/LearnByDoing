using System.Collections.Concurrent;
using System.Net.Sockets;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
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
            SocketConnectionShare.CreateSocket(Configuration);
            SocketConnectionShare.Listenner(Configuration);
            //
            connected = new List<Socket>();
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

        public void Read(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            int byteRec = SocketConnectionShare.Read(
                Configuration, 
                ref statisticOfFunction,
                connected.ToArray(),
                out List<byte[]> BufferList);

            if (byteRec > 0)
            {
                foreach(var item in BufferList)
                    buffer.Enqueue(item);
            }
            Thread.Sleep(1);
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            byte[]? sendBuffer;
            if (buffer.TryDequeue(out sendBuffer))
            {
                SocketConnectionShare.Write(ref statisticOfFunction, connected.ToArray(), sendBuffer);
            }
        }
    }
}
