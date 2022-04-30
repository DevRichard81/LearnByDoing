using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Gutenberg.Configuration;
using Gutenberg.Error;
using Gutenberg.Statistic;

namespace Gutenberg.Types.NetworkSocket
{
    public class SocketConnectionClient : IConnectionType
    {
        private IConfiguration? Configuration { get; set; }
        private byte[] receiveBuffer;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }


        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationSocket;            
            SocketConnectionShare.CreateSocket((ConfigurationSocket)Configuration);
            SocketConnectionShare.Connect((ConfigurationSocket)Configuration);
            //
            var config = (ConfigurationSocket)Configuration;
            receiveBuffer = new byte[config.reciveBufferSize];
        }
        public void Close()
        {
            SocketConnectionShare.Disconnect((ConfigurationSocket)Configuration);
        }

        /// <summary>
        /// Not used for FileConnction
        /// </summary>
        public void Start() { }

        public void Read(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            var config = (ConfigurationSocket)Configuration;
            int byteRecv = SocketConnectionShare.Read(ref statisticOfFunction, config.socket, ref receiveBuffer);
            if(byteRecv > 0)
            {
                buffer.Enqueue(receiveBuffer[0..byteRecv]);
            }
            Thread.Sleep(1);
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            var config = (ConfigurationSocket)Configuration;
            if (config.socket.Connected)
            {
                byte[]? sendBuffer;
                if (buffer.TryDequeue(out sendBuffer))
                {
                    SocketConnectionShare.Write(ref statisticOfFunction, config.socket, sendBuffer);
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
