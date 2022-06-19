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
        private List<Socket> connected;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Init(IConfiguration newConfiguration, IGutenberg gutenberg)
        {
            Configuration = newConfiguration as ConfigurationSocket;
            connected = new List<Socket>();
            SocketConnectionShare.CreateSocket(Configuration);
            SocketConnectionShare.Listenner(Configuration);
            gutenberg.gutenbergThreads.AddThread(Listener, gutenberg.statistic, gutenberg.errorObject, gutenberg.gutenbergBuffers);
            gutenberg.gutenbergThreads.AddThread(Read, gutenberg.statistic, gutenberg.errorObject, gutenberg.gutenbergBuffers);
            gutenberg.gutenbergThreads.AddThread(Write, gutenberg.statistic, gutenberg.errorObject, gutenberg.gutenbergBuffers);
            gutenberg.isServer = true;
        }

        public void Start() { }

        public void Close()
        {
            SocketConnectionShare.Disconnect(Configuration);
        }
        public bool HasConnection()
        {
            bool hasActiveConnection = false;
            
            for(int i = 0; i < connected.Count; i++)
            {
                if(connected[i].Connected)
                    hasActiveConnection = true;
            }
                        
            return hasActiveConnection;
        }

        public void Listener(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers buffer)
        {
            if (connected.Count == 0)
            {
                Socket clientSocket = Configuration.socket.Accept();
                connected.Add(clientSocket);
                Console.WriteLine("Connecting with " + clientSocket?.RemoteEndPoint?.ToString());
            }
            else
            {
                bool isRemoved = false;
                for(int i = 0; i < connected.Count; i++)
                {
                    if (!connected[i].Connected)
                    {
                        Console.WriteLine("Client Disconnected removing the connection " + connected[i].RemoteEndPoint.ToString());
                        connected[i].Shutdown(SocketShutdown.Both);
                        connected[i].Disconnect(true);
                        connected[i].Close();
                        connected.RemoveAt(i);
                        isRemoved = true;
                    }
                }    
                if(isRemoved)
                    Configuration.socket.Listen(Configuration.backlog);
            }
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
