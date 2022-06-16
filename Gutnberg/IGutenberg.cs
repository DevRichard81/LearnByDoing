using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_Gutenberg.GutenbergShared;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistics;

namespace Project_Gutenberg
{
    public enum BufferIndex
    {
        Send = 0,
        Receive = 1
    }

    public interface IGutenberg
    {
        public ErrorObject? errorObject { get; set; }
        public StatisticInterface? statistic { get; set; }

        public void Init();
        public void Start();
        public void Terminated();
        public void Status();
        public void HasError();
        public bool HasMessage();
        public void Put(byte[] setMessage);
        public byte [] Get();
    }
}
