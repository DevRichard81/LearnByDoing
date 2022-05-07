using System.Text;
using System.Threading;
using Project_Gutenberg;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Types.File;

namespace LearnByDoingCli
{
    internal class SampleFile : SampleShared, ISamples
    {
        byte[] handledMessage;
        string pongBy;

        public SampleFile()
        {
            config = new ConfigurationFile()
                .SetDirectorys(@"D:\Gutenberg\", "incoming", "outcoming", "error")
                .SetSendFile("suffix", "yyyyMMddHHmmssfffffff", "prefix", "txt");

            Gutenbergs = new Dictionary<string, Gutenberg>();
            Gutenbergs.Add("Server", new Gutenberg().Configuration(config, new FileConnection()));

            pongBy = "Server";
        }

        public void Run()
        {
            Console.WriteLine("-------------------");
            Console.WriteLine("Doing a File Sample");
            Console.WriteLine("-------------------");

            Init();

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
                        if (itm.Key == pongBy)
                            itm.Value.Put(Encoding.ASCII.GetBytes("Recived your message. Message:" + Encoding.ASCII.GetString(handledMessage)));
                    }
                }
                InputReact();
            }
        }
    }
}
