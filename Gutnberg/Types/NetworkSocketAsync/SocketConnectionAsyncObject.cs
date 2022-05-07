using System.Collections.Concurrent;
using System.Net.Sockets;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.Types.NetworkSocketAsync
{
    public class SocketConnectionAsyncObject
    {
        public StatisticOfFunction StatisticSend { get; set; }
        public StatisticOfFunction StatisticReceive { get; set; }
        public ConcurrentQueue<byte[]> BufferSend { get; set; }
        public ConcurrentQueue<byte[]> BufferReceive { get; set; }
        public byte[] BufferReceiveBuffer { get; set; }

        public Socket socket { get; set; }
        public ErrorObject? ErrorObject { get; set; }
    }
}
