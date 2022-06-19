using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_Gutenberg.Error;
using Project_Gutenberg;

namespace Project_Gutenberg.GutenbergShared
{
    public class GutenbergBuffers : IGutenbergBuffers
    {
        private ConcurrentQueue<byte[]> buffersReceive;
        public ConcurrentQueue<byte[]> BufferReceive { get { return buffersReceive; } }
        public bool HasMessage()
        {
            return buffersReceive.Count > 0;
        }

        private ConcurrentQueue<byte[]> buffersSend;
        public ConcurrentQueue<byte[]> BufferSend { get { return buffersSend; } }

        public ConcurrentQueue<byte[]> GetBuffer(int no)
        {
            if (no == 0)
                return buffersReceive;
            else
                return buffersSend;
        }

        public GutenbergBuffers()
        {
            buffersReceive = new ConcurrentQueue<byte[]>();
            buffersSend = new ConcurrentQueue<byte[]>();
        }

        public override string ToString()
        {
            return "GutenbergBuffer Count of Queues (IN"+ buffersReceive?.Count + "/OUT"+ buffersSend?.Count + ")";
        }

        public void AddReceivedMessage(byte[] messageReceived)
        {
            buffersReceive.Enqueue(messageReceived);
        }
        public byte[] GetReceivedMessage()
        {
            byte[]? recMessage;
            while (!buffersReceive.TryDequeue(out recMessage))
            {
                Thread.Sleep(5);
            }
            return recMessage;
        }

        public void AddSendMessage(byte [] messageToSend)
        {
            buffersSend.Enqueue(messageToSend);
        }
        public byte [] GetSendMessage()
        {
            byte[]? recMessage;
            while (!buffersSend.TryDequeue(out recMessage))
            {
                Thread.Sleep(5);
            }
            return recMessage;
        }
    }
}
