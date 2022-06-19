using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.Types
{
    public interface IConnectionTypeAsync
    {
        public ErrorObject? ErrorObject { get; set; }

        public void Init(IConfiguration newConfiguration, IGutenberg gutenberg);
        public void Start();
        public void Close();
        public bool HasConnection();

        public void ReadWrite(
            ref StatisticOfFunction statisticOfFunctionSend,
            ref StatisticOfFunction statisticOfFunctionReceive,
            ConcurrentQueue<byte[]> bufferSend,
            ConcurrentQueue<byte[]> bufferReceive);
    }
}
