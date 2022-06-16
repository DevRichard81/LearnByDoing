using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Gutenberg.GutenbergShared
{
    public interface IGutenbergBuffers
    {
        public ConcurrentQueue<byte[]> BufferReceive { get; }
        public ConcurrentQueue<byte[]> BufferSend { get; }
        public ConcurrentQueue<byte[]> GetBuffer(int no);
        public bool HasMessage();
        public void AddReceivedMessage(byte[] messageReceived);
        public byte[] GetReceivedMessage();
        public void AddSendMessage(byte[] messageToSend);
        public byte[] GetSendMessage();
    }
}
