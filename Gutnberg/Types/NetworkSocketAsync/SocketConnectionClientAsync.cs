using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        private SocketConnectionAsyncObject AsyncObject;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Close()
        {
            AsyncObject.Configuration?.socket?.Shutdown(SocketShutdown.Both);
            AsyncObject.Configuration?.socket?.Close();
            AsyncObject.connectDone.Reset();
        }
        public void Init(IConfiguration newConfiguration, IGutenberg gutenberg)
        {
            AsyncObject = new SocketConnectionAsyncObject();
            AsyncObject.Configuration = newConfiguration as ConfigurationSocket;
            SocketConnectionShareAsync.CreateSocket(ref AsyncObject);
            gutenberg.gutenbergThreads.AddThread(ReadWrite, gutenberg.statistic, gutenberg.errorObject, gutenberg.gutenbergBuffers);
        }
        public void Start()
        {
            AsyncObject.Configuration.socket.BeginConnect(
                AsyncObject.Configuration.ipEndPoint,
                new AsyncCallback(SocketConnectionShareAsync.ConnectCallback),
                AsyncObject);
        }
        public bool HasConnection()
        {
            if(AsyncObject.Configuration.socket.Connected)
                return true;

            Close();
            AsyncObject.Configuration.socket = null;
            return false;
        }
        public void ReadWrite(
            ref StatisticOfFunction statisticOfFunctionSend,
            ref StatisticOfFunction statisticOfFunctionReceive,
            ConcurrentQueue<byte[]> bufferSend,
            ConcurrentQueue<byte[]> bufferReceive)
        {
            AsyncObject.connectDone.WaitOne(); // WaitFor a connection

            AsyncObject.StatisticReceive = statisticOfFunctionReceive;
            AsyncObject.StatisticSend = statisticOfFunctionSend;
            AsyncObject.BufferReceive = bufferReceive;
            AsyncObject.BufferSend = bufferSend;
            AsyncObject.BufferReceiveBuffer = new byte[AsyncObject.Configuration.reciveBufferSize];

            if (!AsyncObject.receiveActive.IsSet)
            {
                AsyncObject.receiveActive.Set();
                AsyncObject.socket.BeginReceive(
                    AsyncObject.BufferReceiveBuffer, 0, AsyncObject.Configuration.reciveBufferSize,
                    SocketFlags.None,
                    new AsyncCallback(SocketConnectionShareAsync.ReceiveCallback),
                    AsyncObject);
            }

            if (!AsyncObject.sendActive.IsSet)
            {
                AsyncObject.sendActive.Set();

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
        }
    }
}
