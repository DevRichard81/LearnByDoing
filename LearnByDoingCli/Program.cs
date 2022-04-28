using Gutenberg.Configuration;
using Gutenberg.Types.File;
using Gutenberg;
using System.Text;
using LearnByDoingCli;
using System.Net.Sockets;
using Gutenberg.Types.NetworkSocket;

Console.WriteLine("---------------------------");
Console.WriteLine("Welcome to Larning by Doing");
Console.WriteLine("---------------------------");

//
ISamples mySample;
SampleFile mySampleFile;
//


ConfigurationSocket config = new ConfigurationSocket();
config.SetEndPoint("127.0.0.1", 1001);
config.protocolType = ProtocolType.Tcp;

// Socket Server
Gutenberg<ConfigurationSocket, SocketConnectionServer> gutenbergS = new Gutenberg<ConfigurationSocket, SocketConnectionServer>().Configuration(config, new SocketConnectionServer());
gutenbergS.Init();
if (gutenbergS.errorObject != null)
    Console.WriteLine(gutenbergS.errorObject.ToString());
gutenbergS.Start();
if (gutenbergS.errorObject != null)
    Console.WriteLine(gutenbergS.errorObject.ToString());

// Socket Client
Gutenberg<ConfigurationSocket, SocketConnectionClient> gutenbergC = new Gutenberg<ConfigurationSocket, SocketConnectionClient>().Configuration(config, new SocketConnectionClient());
gutenbergC.Init();
if (gutenbergC.errorObject != null)
    Console.WriteLine(gutenbergC.errorObject.ToString());
gutenbergC.Start();
if (gutenbergC.errorObject != null)
    Console.WriteLine(gutenbergC.errorObject.ToString());

bool finish = false;
bool sendClientToServer = false;

Console.WriteLine("Finish init prozss");
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

    if (!sendClientToServer)
    {
        sendClientToServer = true;
        gutenbergC.Put((int)BufferIndex.Send, Encoding.ASCII.GetBytes("Thanks for your Message from Client."));
        Thread.Sleep(500);
    }

    if (gutenbergS.HasMessage((int)BufferIndex.Receive) > 0)
    {
        var message = gutenbergS.Get((int)BufferIndex.Receive);
        Console.WriteLine("Message was recived by S Message is:" + Encoding.ASCII.GetString(message));
        gutenbergS.Put((int)BufferIndex.Send, Encoding.ASCII.GetBytes("Recived your message Client. Message:" + Encoding.ASCII.GetString(message)));
        Thread.Sleep(100);
    }

    if (gutenbergC.HasMessage((int)BufferIndex.Receive) > 0)
    {
        var message = gutenbergC.Get((int)BufferIndex.Receive);
        Console.WriteLine("Message was recived by C Message is:" + Encoding.ASCII.GetString(message));
        Thread.Sleep(100);
    }

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

/*


    if (gutenberg.HasMessage((int)BufferIndex.Receive) > 0)
    {
        Console.WriteLine("Message was recived");
        var message = gutenberg.Get((int)BufferIndex.Receive);
        Console.WriteLine("Message is:" + Encoding.ASCII.GetString(message));

        var sendMessage = Encoding.ASCII.GetBytes("Thanks for your Message:" + Encoding.ASCII.GetString(message));
        gutenberg.Put((int)BufferIndex.Send, sendMessage);
    }


  */