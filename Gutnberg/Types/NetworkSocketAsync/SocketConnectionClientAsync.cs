using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistic;
using Project_Gutenberg.Types;
using Project_Gutenberg.Types.NetworkSocket;

namespace Project_Gutenberg.Types.NetworkSocketAsync
{
    public class SocketConnectionClientAsync : IConnectionTypeAsync
    {
        private ConfigurationSocket? Configuration { get; set; }
        private SocketConnectionAsyncObject AsyncObject;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationSocket;
            SocketConnectionShareAsync.CreateSocket(Configuration, ref AsyncObject);
            SocketConnectionShare.Connect(Configuration);       
        }

        public void ReadWrite(
            ref StatisticOfFunction statisticOfFunctionSend,
            ref StatisticOfFunction statisticOfFunctionReceive,
            ref ConcurrentQueue<byte[]> bufferSend,
            ref ConcurrentQueue<byte[]> bufferReceive)
        {
            AsyncObject.StatisticReceive = statisticOfFunctionReceive;
            AsyncObject.StatisticSend = statisticOfFunctionSend;
            AsyncObject.BufferReceive = bufferReceive;
            AsyncObject.BufferSend = bufferSend;
            AsyncObject.BufferReceiveBuffer = new byte[Configuration.reciveBufferSize];

            AsyncObject.socket.BeginReceive(
                AsyncObject.BufferReceiveBuffer, 0, Configuration.reciveBufferSize,
                SocketFlags.None,
                new AsyncCallback(SocketConnectionShareAsync.ReceiveCallback),
                AsyncObject);

            byte[]? sendBuffer;
            if (bufferSend.TryDequeue(out sendBuffer))
            {
                AsyncObject.socket.BeginSend(
                    sendBuffer, 0, sendBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(SocketConnectionShareAsync.SendCallback),
                    AsyncObject);
            }            
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
