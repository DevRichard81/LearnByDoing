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


    public interface IGutenberg
    {
        public ErrorObject? errorObject { get; set; }
        public StatisticInterface? statistic { get; set; }
        public IGutenbergBuffers gutenbergBuffers { get; set; }
        public GutenbergThreads gutenbergThreads { get; set; }
        public bool isServer { get; set; }

        public void Init();
        public void Start();
        public void Terminated();
        public void Status();
        public void HasError();
        public bool HasMessage();
        public bool HasConnection();
        public void Put(byte[] setMessage);
        public byte [] Get();
    }
}
