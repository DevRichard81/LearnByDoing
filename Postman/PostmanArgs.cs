using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerArgs;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Types.Pipe;
using Project_Gutenberg;
using Project_Gutenberg.Types.NetworkSocket;
using System.Net.Sockets;

namespace Postman
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    internal class PostmanArgs
    {
        [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
        public bool Help { get; set; }

        [ArgActionMethod, ArgDescription("Socket Server Connection")]
        public void SocketServer(ConfigSocket args)
        {
            Program.gutenberg = new Gutenberg()
                .Configuration(
                new ConfigurationSocket()
                    .SetEndPoint(args.ip, args.port)
                    .SetProtocolType(ProtocolType.Tcp),
                new SocketConnectionServer());
        }

        [ArgActionMethod, ArgDescription("Socket Client Connection")]
        public void SocketClient(ConfigSocket args)
        {
            Program.gutenberg = new Gutenberg()
                .Configuration(
                    new ConfigurationSocket()
                        .SetEndPoint(args.ip, args.port)
                        .SetProtocolType(ProtocolType.Tcp),
                    new SocketConnectionClient());
        }

        [ArgActionMethod, ArgDescription("Pipe Server Connection")]
        public void PipeServer(ConfigPipe args)
        {
            Program.gutenberg = new Gutenberg()
                .Configuration(
                new ConfigurationPipes()
                    .SetPipeName(args.pipeName)
                    .SetNumThreads(2)
                    .SetPipeDirections(PipeDirection.InOut),
                new PipeConnectionServer());
        }

        [ArgActionMethod, ArgDescription("Pipe Client Connection")]
        public void PipeClient(ConfigPipe args)
        {
            Program.gutenberg = new Gutenberg()
                .Configuration(
                new ConfigurationPipes()
                    .SetPipeName(args.pipeName)
                    .SetNumThreads(2)
                    .SetPipeDirections(PipeDirection.InOut),
                new PipeConnectionClient());
        }
    }

    public class ConfigSocket
    {
        [ArgRequired, ArgDescription("IP Numbers"), ArgPosition(1), DefaultValue("127.0.0.1")]
        public string ip { get; set; }
        [ArgRequired, ArgDescription("Port"), ArgPosition(2), ArgRange(1,65000)]
        public int port { get; set; }
    }

    public class ConfigPipe
    {
        [ArgRequired, ArgDescription("PipeName"), ArgPosition(1)]
        public string pipeName { get; set; }

    }
}
