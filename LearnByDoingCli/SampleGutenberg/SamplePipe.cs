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
        string PongBy { get; set; }
        byte[]? handledMessage;

        public SamplePipe()
        {
            config = new ConfigurationPipes()
                .SetPipeName("MyPipe")
                .SetNumThreads(2).SetPipeDirections(PipeDirection.InOut);

            Gutenbergs = new Dictionary<string, IGutenberg>
            {
                { "Server", new Gutenberg().Configuration(config, new PipeConnectionClient()) },
                { "Client", new Gutenberg().Configuration(config, new PipeConnectionClient()) }
            };

            PongBy = "Server";
        }

        public void Run()
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("Doing a Pipe Sample");
            Console.WriteLine("---------------------");

            Init();
            if (Gutenbergs == null)
                return;

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
                        if (itm.Key == PongBy)
                            itm.Value.Put(Encoding.ASCII.GetBytes("Recived your message. Message:" + Encoding.ASCII.GetString(handledMessage)));
                    }
                }
                // <-
                InputReact();
            }
        }
    }
}
