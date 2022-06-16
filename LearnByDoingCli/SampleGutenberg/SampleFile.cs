using System.Text;
using System.Threading;
using Project_Gutenberg;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Types.File;

namespace LearnByDoingCli
{
    internal class SampleFile : SampleShared, ISamples
    {
        byte[]? handledMessage;
        string PongBy { get; set; }

        public SampleFile()
        {
            config = new ConfigurationFile()
                .SetDirectorys(@"D:\Gutenberg\", "incoming", "outcoming", "error")
                .SetSendFile("suffix", "yyyyMMddHHmmssfffffff", "prefix", "txt");

            Gutenbergs = new Dictionary<string, IGutenberg>
            {
                { "Server", new Gutenberg().Configuration(config, new FileConnection()) }
            };

            PongBy = "Server";
        }

        public void Run()
        {
            Console.WriteLine("-------------------");
            Console.WriteLine("Doing a File Sample");
            Console.WriteLine("-------------------");

            Init();
            if (Gutenbergs == null)
                return;

            while (!finish)
            {
                ReportStatistics();

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
                InputReact();
            }
        }
    }
}
