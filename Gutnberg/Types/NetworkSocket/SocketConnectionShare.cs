using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Gutenberg.Configuration;

namespace Gutenberg.Types.NetworkSocket
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
    }
}
