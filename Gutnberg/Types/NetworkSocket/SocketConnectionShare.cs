using System.Net.Sockets;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.Types.NetworkSocket
{
    internal static class SocketConnectionShare
    {
        public static void CreateSocket(ConfigurationSocket configuration)
        {
            Console.WriteLine("CreateSocket");
            configuration.socket = new Socket(configuration.ipEndPoint.AddressFamily, SocketType.Stream, configuration.protocolType);
        }

        public static void Connect(ConfigurationSocket configuration)
        {
            Console.WriteLine("Connect");
            for (int i = 0; i < configuration.retrayConnection; i++)
            {
                try
                {
                    configuration.socket.Connect(configuration.ipEndPoint);
                }
                catch(SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        Console.WriteLine("Wait for Retray");
                        Thread.Sleep(configuration.retrayWait);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void Listenner(ConfigurationSocket configuration)
        {
            Console.WriteLine("Listenner");
            configuration.socket.Bind(configuration.ipEndPoint);
            configuration.socket.Listen(configuration.backlog);            
        }

        public static void Disconnect(ConfigurationSocket configuration)
        {
            Console.WriteLine("Disconnect");
            try
            {
                configuration.socket.Shutdown(SocketShutdown.Both);
            }
            catch(ObjectDisposedException ex) {  }

            configuration.socket.Close();
        }

        public static int Read(ConfigurationSocket configuration, ref StatisticOfFunction statisticOfFunction, Socket [] readFromSockets, out List<byte[]> bufferList)
        {
            int countAllBytes = 0;
            byte[] buffer = new byte[configuration.reciveBufferSize];

            bufferList = new List<byte[]>();

            for(int i = 0; i < readFromSockets.Length; i++)
            {
                var readBytes = Read(readFromSockets[i], ref buffer);
                bufferList.Add(buffer[0..readBytes]);
                countAllBytes += readBytes;
            }

            statisticOfFunction.handelDataLength += (uint)countAllBytes;
            statisticOfFunction.handelMessage += (uint)bufferList.Count;
            statisticOfFunction.type = StatisticOfFunction.Type.Incoming;

            return countAllBytes;
        }
        public static int Read(Socket readFromSocket, ref byte[] buffer)
        {
            StatisticOfFunction statisticOfFunction = new StatisticOfFunction();
            return Read(ref statisticOfFunction, readFromSocket, ref buffer);
        }
        public static int Read(ref StatisticOfFunction statisticOfFunction, Socket readFromSocket, ref byte[] buffer)
        {
            Array.Clear(buffer);
            int countBytes = readFromSocket.Receive(buffer);
            if (statisticOfFunction != null)
            {
                statisticOfFunction.handelDataLength += (uint)countBytes;
                statisticOfFunction.handelMessage += 1;
                statisticOfFunction.type = StatisticOfFunction.Type.Incoming;
            }
            return countBytes;
        }

        public static int Write(ref StatisticOfFunction statisticOfFunction, Socket[] writeToSocket, byte[] sendBuffer)
        {
            int countAllBytes = 0;
            for (int i = 0; i < writeToSocket.Length; i++)
            {
                var x = Write(writeToSocket[i], sendBuffer);
                if (countAllBytes == 0)
                    countAllBytes = x;
            }

            statisticOfFunction.handelDataLength += (uint)countAllBytes;
            statisticOfFunction.handelMessage += 1;
            statisticOfFunction.type = StatisticOfFunction.Type.Outcoming;
            return countAllBytes;
        }
        public static int Write(Socket writeToSocket, byte[] sendBuffer)
        {
            StatisticOfFunction statisticOfFunction = new StatisticOfFunction();
            return Write(ref statisticOfFunction, writeToSocket, sendBuffer);
        }
        public static int Write(ref StatisticOfFunction statisticOfFunction, Socket writeToSocket, byte[] sendBuffer)
        {
            int byteSent = writeToSocket.Send(sendBuffer);
            if (byteSent > 0)
            {
                statisticOfFunction.handelMessage++;
                statisticOfFunction.handelDataLength = (uint)sendBuffer.Length;
                statisticOfFunction.type = StatisticOfFunction.Type.Outcoming;
            }
            return byteSent;
        }
    }
}
