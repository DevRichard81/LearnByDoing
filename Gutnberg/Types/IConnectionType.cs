using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gutenberg.Configuration;
using Gutenberg.Statistic;

namespace Gutenberg.Types
{
    public interface IConnectionType
    {        
        public void Init(IConfiguration newConfiguration);
        public void Start();
        public void Close();

        public void Read(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer);
        public void Write(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer);
    }
}
