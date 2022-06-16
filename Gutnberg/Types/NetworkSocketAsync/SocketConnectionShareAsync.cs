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
        public static void CreateSocket(ref SocketConnectionAsyncObject asyncObject)
        {
            if (asyncObject == null)
            {
                Console.WriteLine("Fatal, " + nameof(asyncObject) + " can not be null");
                return;
            }

            Console.WriteLine("CreateSocket");
            asyncObject.socket = new Socket(
                asyncObject.Configuration.ipEndPoint.AddressFamily,
                SocketType.Stream,
                asyncObject.Configuration.protocolType);
        }
        public static void ConnectCallback(IAsyncResult ar)
        {
            var AsyncObject = ar.AsyncState as SocketConnectionAsyncObject;
            if (AsyncObject == null)
            {
                Console.WriteLine("Fatal, can not cast " + nameof(ar) + " to " + nameof(SocketConnectionAsyncObject));
                return;
            }
            if (AsyncObject.socket == null)
            {
                Console.WriteLine("Fatal, can not have her a null socket");
                return;
            }

            try
            {
                AsyncObject.Configuration.socket.EndConnect(ar);
                AsyncObject.connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static void ReceiveCallback(IAsyncResult ar)
        {
            var AsyncObject = ar.AsyncState as SocketConnectionAsyncObject;
            if (AsyncObject == null)
            {
                Console.WriteLine("Fatal, can not cast " + nameof(ar) + " to " + nameof(SocketConnectionAsyncObject));
                return;
            }
            if (AsyncObject.Configuration.socket == null)
            {
                Console.WriteLine("Fatal, can not have her a null socket");
                return;
            }
            try
            {
                int bytesRead = AsyncObject.Configuration.socket.EndReceive(ar);
                AsyncObject.StatisticReceive.handelDataLength += (uint)bytesRead;
                AsyncObject.StatisticReceive.handelMessage++;
                AsyncObject.BufferReceive.Enqueue(AsyncObject.BufferReceiveBuffer);
                // Restart the listening
                AsyncObject.receiveActive.Set();
                AsyncObject.socket.BeginReceive(
                    AsyncObject.BufferReceiveBuffer, 0, AsyncObject.Configuration.reciveBufferSize,
                    SocketFlags.None,
                    new AsyncCallback(SocketConnectionShareAsync.ReceiveCallback),
                    AsyncObject);                
            }
            catch (Exception e)
            {
                AsyncObject.ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, e.Message, nameof(ReceiveCallback));
            }
        }
        public static void SendCallback(IAsyncResult ar)
        {
            var AsyncObject = ar.AsyncState as SocketConnectionAsyncObject;
            if (AsyncObject == null)
            {
                Console.WriteLine("Fatal, can not cast " + nameof(ar) + " to " + nameof(SocketConnectionAsyncObject));
                return;
            }
            if (AsyncObject.Configuration.socket == null)
            {
                Console.WriteLine("Fatal, can not have her a null socket");
                return;
            }

            try
            {
                // Complete sending the data to the remote device.  
                int bytesSent = AsyncObject.Configuration.socket.EndSend(ar);
                AsyncObject.StatisticSend.handelDataLength += (uint)bytesSent;
                AsyncObject.StatisticSend.handelMessage++;
                AsyncObject.sendActive.Reset();
            }
            catch (Exception e)
            {
                AsyncObject.ErrorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, e.Message, nameof(SendCallback));
            }
        }
    }
}
