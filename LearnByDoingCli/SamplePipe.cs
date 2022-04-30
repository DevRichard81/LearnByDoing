using System.Net.Sockets;
using System.Text;
using Gutenberg;
using Gutenberg.Configuration;
using Gutenberg.Types.Pipe;

namespace LearnByDoingCli
{
    internal class SamplePipe : ISamples
    {
        public void Run()
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("Doing a Pipe Sample");
            Console.WriteLine("---------------------");

            ConfigurationPipes config = new ConfigurationPipes();
            config.pipeName = "MyPipe";
            config.numThreads = 2;
            config.pipeDirection = System.IO.Pipes.PipeDirection.InOut;

            // Pipe Server
            Gutenberg<ConfigurationPipes, PipeConnectionServer> gutenbergS = new Gutenberg<ConfigurationPipes, PipeConnectionServer>().Configuration(config, new PipeConnectionServer());
            gutenbergS.Init();
            if (gutenbergS.errorObject != null)
                Console.WriteLine(gutenbergS.errorObject.ToString());
            gutenbergS.Start();
            if (gutenbergS.errorObject != null)
                Console.WriteLine(gutenbergS.errorObject.ToString());

            // Pipe Client
            Gutenberg<ConfigurationPipes, PipeConnectionClient> gutenbergC = new Gutenberg<ConfigurationPipes, PipeConnectionClient>().Configuration(config, new PipeConnectionClient());
            gutenbergC.Init();
            if (gutenbergC.errorObject != null)
                Console.WriteLine(gutenbergC.errorObject.ToString());
            gutenbergC.Start();
            if (gutenbergC.errorObject != null)
                Console.WriteLine(gutenbergC.errorObject.ToString());

            bool finish = false;
            bool sendClientToServer = false;

            Console.WriteLine("Finish init prozess");
            Thread.Sleep(1000);

            while (!finish)
            {
                Console.WriteLine("S" + gutenbergS.statistic.ToString());
                if (gutenbergS.errorObject != null)
                {
                    Console.WriteLine("S" + gutenbergS.errorObject.ToString());
                    gutenbergS.errorObject = null;
                }

                Console.WriteLine("C" + gutenbergC.statistic.ToString());
                if (gutenbergC.errorObject != null)
                {
                    Console.WriteLine("C" + gutenbergC.errorObject.ToString());
                    gutenbergC.errorObject = null;
                }

                gutenbergS.Status();
                gutenbergC.Status();

                // ->
                if (!sendClientToServer)
                {
                    Console.WriteLine("Try to send");
                    sendClientToServer = true;
                    gutenbergC.Put(Encoding.ASCII.GetBytes("Thanks for your Message from Client."));
                    Thread.Sleep(500);
                    gutenbergC.HasError();
                }

                if (gutenbergS.HasMessage() > 0)
                {
                    var message = gutenbergS.Get();
                    Console.WriteLine("Message was recived by S Message is:" + Encoding.ASCII.GetString(message));
                    gutenbergS.Put(Encoding.ASCII.GetBytes("Recived your message Client. Message:" + Encoding.ASCII.GetString(message)));
                    Thread.Sleep(100);
                    gutenbergS.HasError();
                }

                if (gutenbergC.HasMessage() > 0)
                {
                    var message = gutenbergC.Get();
                    Console.WriteLine("Message was recived by C Message is:" + Encoding.ASCII.GetString(message));
                    Thread.Sleep(100);
                    gutenbergC.HasError();
                }
                // <-

                Thread.Sleep(500);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo result = Console.ReadKey();
                    if (result.Key == ConsoleKey.Escape)
                        finish = true;
                }
            }

            gutenbergS.Terminated();
            gutenbergS = null;
            gutenbergC.Terminated();
            gutenbergC = null;
        }
    }
}
