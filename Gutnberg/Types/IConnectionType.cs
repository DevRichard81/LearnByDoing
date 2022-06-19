using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_Gutenberg.GutenbergShared;
using Project_Gutenberg.Configuration;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.Types
{
    public interface IConnectionType
    {
        public ErrorObject? ErrorObject { get; set; }

        public void Init(IConfiguration newConfiguration, IGutenberg gutenberg);
        public void Start();
        public void Close();
        public bool HasConnection();

        public void Read(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers gutenbergBuffer);
        public void Write(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers gutenbergBuffer);
    }
}
