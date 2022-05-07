using System.Net.Sockets;
using System.Text;
using Project_Gutenberg;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Types.NetworkSocket;

namespace LearnByDoingCli
{
    internal class SampleSocket : SampleShared, ISamples
    {        
        //
        private bool sendClientToServer;
        private string pongBy;
        private byte[] handledMessage;

        public SampleSocket()
        {
            config = new ConfigurationSocket()
                .SetEndPoint("127.0.0.1", 1001)
                .SetProtocolType(ProtocolType.Tcp);

            Gutenbergs = new Dictionary<string, Gutenberg>();
            Gutenbergs.Add("Server", new Gutenberg().Configuration(config, new SocketConnectionServer()));
            Gutenbergs.Add("Client", new Gutenberg().Configuration(config, new SocketConnectionClient()));

            pongBy = "Server";
        }

        public void Run()
        {
            if(Gutenbergs.Count != 2)
            {
                Console.WriteLine("Server or Client was not exsiting, count only" + Gutenbergs.Count);
                return;
            }
            Console.WriteLine("---------------------");
            Console.WriteLine("Doing a Socket Sample");
            Console.WriteLine("---------------------");

            Init();

            while (!finish)
            {
                foreach(var itm in Gutenbergs)
                {
                    ReportStatistics();
                }
                
                // ->
                if (!sendClientToServer)
                {
                    sendClientToServer = true;
                    Gutenbergs["Client"].Put(Encoding.ASCII.GetBytes("Thanks for your Message from Client."));
                    Gutenbergs["Client"].HasError();
                    Thread.Sleep(100);                    
                }

                foreach(var itm in Gutenbergs)
                {
                    handledMessage = ReceiveMessageIf(itm.Value);
                    itm.Value.HasError();
                    if (handledMessage != null)
                    {
                        Console.WriteLine("Server Received Message:" + Encoding.ASCII.GetString(handledMessage));
                        if(itm.Key == pongBy)
                            itm.Value.Put(Encoding.ASCII.GetBytes("Recived your message. Message:" + Encoding.ASCII.GetString(handledMessage)));
                    }
                }
                // <-
                InputReact();
            }
        }
    }
}
