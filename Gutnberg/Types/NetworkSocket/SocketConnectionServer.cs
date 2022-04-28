using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gutenberg.Configuration;
using Gutenberg.Error;
using Gutenberg.Statistic;

namespace Gutenberg.Types.NetworkSocket
{
    public class SocketConnectionServer : IConnectionType
    {
        private IConfiguration? Configuration { get; set; }
        private byte[] receiveBuffer;
        private Thread listener;
        private List<Socket> connected;
        public ErrorObject? ErrorObject { get; private set; }

        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationSocket;
            SocketConnectionShare.CreateSocket((ConfigurationSocket)Configuration);
            SocketConnectionShare.Listenner((ConfigurationSocket)Configuration);
            //
            var config = (ConfigurationSocket)Configuration;
            receiveBuffer = new byte[config.reciveBufferSize];
            connected = new List<Socket>();
        }

        public void Start()
        {
            var config = (ConfigurationSocket)Configuration;
            listener = new Thread(() =>
            {
                Socket clientSocket = config.socket.Accept();
                connected.Add(clientSocket);
            });
            listener.Start();
        }

        public void Close()
        {
            SocketConnectionShare.Disconnect((ConfigurationSocket)Configuration);
        }

        public void Read(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            var config = (ConfigurationSocket)Configuration;
            for (int i = 0; i < connected.Count; i++)
            {
                if (connected[i].Connected)
                {
                    Array.Clear(receiveBuffer);
                    int byteRecv = connected[i].Receive(receiveBuffer);
                    if (byteRecv > 0)
                    {
                        buffer.Enqueue(receiveBuffer[0..byteRecv]);
                        statisticOfFunction.handelDataLength += (uint)byteRecv;
                        statisticOfFunction.handelMessage++;                        
                    }
                }
            }
            statisticOfFunction.type = StatisticOfFunction.Type.Incoming;
            Console.WriteLine("D" + buffer.Count)
            Thread.Sleep(1);
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            var config = (ConfigurationSocket)Configuration;
            byte[]? sendBuffer;
            int byteSend = 0;
            if (buffer.TryDequeue(out sendBuffer))
            {
                for(int i = 0; i < connected.Count; i++)
                {
                    if (connected[i].Connected)
                    {
                         int x = config.socket.Send(sendBuffer);
                        if(byteSend == 0)
                            byteSend = x;
                    }
                }

                if (byteSend > 0)
                {
                    statisticOfFunction.handelMessage++;
                    statisticOfFunction.handelDataLength = (uint)byteSend;
                    statisticOfFunction.type = StatisticOfFunction.Type.Outcoming;
                }
            }
        }
    }
}
