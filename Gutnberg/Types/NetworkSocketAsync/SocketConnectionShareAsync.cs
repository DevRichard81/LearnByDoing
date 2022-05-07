using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Project_Gutenberg.Error;
using Project_Gutenberg.Configuration;

namespace Project_Gutenberg.Types.NetworkSocketAsync
{
    public static class SocketConnectionShareAsync
    {
        public static void CreateSocket(ConfigurationSocket configuration, ref SocketConnectionAsyncObject asyncObject)
        {
            Console.WriteLine("CreateSocket");
            asyncObject.socket = new Socket(configuration.ipEndPoint.AddressFamily, SocketType.Stream, configuration.protocolType);
        }
        public static void ReceiveCallback(IAsyncResult ar)
        {
            SocketConnectionAsyncObject client = ar.AsyncState as SocketConnectionAsyncObject;
            if (client == null)
                return;
            try
            {
                int bytesRead = client.socket.EndReceive(ar);
                client.StatisticReceive.handelDataLength += (uint)bytesRead;
                client.StatisticReceive.handelMessage++;
                client.BufferReceive.Enqueue(client.BufferReceiveBuffer);
            }
            catch (Exception e)
            {
                client.ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, e.Message, nameof(ReceiveCallback));
            }
        }
        public static void SendCallback(IAsyncResult ar)
        {
            SocketConnectionAsyncObject client = ar.AsyncState as SocketConnectionAsyncObject;
            if (client == null)
                return;

            try
            {
                // Complete sending the data to the remote device.  
                int bytesSent = client.socket.EndSend(ar);
                client.StatisticSend.handelDataLength += (uint)bytesSent;
                client.StatisticSend.handelMessage++;
            }
            catch (Exception e)
            {
                client.ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, e.Message, nameof(SendCallback));
            }
        }
    }
}
