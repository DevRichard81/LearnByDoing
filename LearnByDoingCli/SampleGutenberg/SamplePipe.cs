using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using Project_Gutenberg;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Types.Pipe;

namespace LearnByDoingCli
{
    internal class SamplePipe : SampleShared, ISamples
    {
        bool sendClientToServer;
        string pongBy;
        byte[] handledMessage;

        public SamplePipe()
        {
            config = new ConfigurationPipes()
                .SetPipeName("MyPipe")
                .SetNumThreads(2).SetPipeDirections(PipeDirection.InOut);

            Gutenbergs = new Dictionary<string, Gutenberg>();
            Gutenbergs.Add("Server", new Gutenberg().Configuration(config, new PipeConnectionClient()));
            Gutenbergs.Add("Client", new Gutenberg().Configuration(config, new PipeConnectionClient()));

            pongBy = "Server";
        }

        public void Run()
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("Doing a Pipe Sample");
            Console.WriteLine("---------------------");

            Init();

            while (!finish)
            {
                foreach (var itm in Gutenbergs)
                {
                    ReportStatistics();
                    itm.Value.Status();
                }

                // ->
                if (!sendClientToServer)
                {
                    sendClientToServer = true;
                    Gutenbergs["Client"].Put(Encoding.ASCII.GetBytes("Thanks for your Message from Client."));
                    Gutenbergs["Client"].HasError();
                    Thread.Sleep(100);
                }

                foreach (var itm in Gutenbergs)
                {
                    handledMessage = ReceiveMessageIf(itm.Value);
                    itm.Value.HasError();
                    if (handledMessage != null)
                    {
                        Console.WriteLine("Server Received Message:" + Encoding.ASCII.GetString(handledMessage));
                        if (itm.Key == pongBy)
                            itm.Value.Put(Encoding.ASCII.GetBytes("Recived your message. Message:" + Encoding.ASCII.GetString(handledMessage)));
                    }
                }
                // <-
                InputReact();
            }
        }
    }
}
