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

        public void Init(IConfiguration newConfiguration);
        public void Start();
        public void Close();

        public void ReadWrite(
            ref StatisticOfFunction statisticOfFunctionSend,
            ref StatisticOfFunction statisticOfFunctionReceive,
            ref ConcurrentQueue<byte[]> bufferSend,
            ref ConcurrentQueue<byte[]> bufferReceive);
    }
}
