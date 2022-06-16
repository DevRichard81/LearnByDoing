using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistic;

namespace Project_Gutenberg.GutenbergShared
{
    public class GutenbergThreads
    {
        public delegate void delegateVoidWithStatistic(ref StatisticOfFunction statisticOfFunction, IGutenbergBuffers gutenbergBuffer);
        public delegate void delegateVoidWithStatisticSendRead(
            ref StatisticOfFunction statisticOfFunctionSend,
            ref StatisticOfFunction statisticOfFunctionReceive,
            ConcurrentQueue<byte[]> bufferSend,
            ConcurrentQueue<byte[]> bufferReceive);

        private List<Thread> threads;
        public CancellationTokenSource? cancellationTokenSource { get; private set; }

        public GutenbergThreads()
        {
            threads = new List<Thread>();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public int CountRunningThreads 
        {
            get
            {
                int count = 0;
                foreach (var thread in threads)
                {
                    if (thread != null && thread?.IsAlive == true)
                        count++;
                }
                return count; 
            } 
        }

        ~GutenbergThreads()
        {
            cancellationTokenSource.Cancel();
            while(CountRunningThreads != 0)
                Thread.Sleep(100);
            threads.Clear();
        }

        public void Start()
        {
            for (int i = 0; i < threads.Count; i++)
            {
                if (threads[i].ThreadState != ThreadState.Running)
                {
                    threads[i].Start();
                    Console.WriteLine($"Status after Thread Starting [{i}][{threads[i].ThreadState}]");
                }
            }
        }

        public void Addthread(ThreadStart newThread)
        {            
            threads.Add(new Thread(newThread));
            Console.WriteLine($"AddThread [{threads.Count}]");
        }
        public override string ToString()
        {
            StringBuilder sb = new();
            foreach(var itm in threads)
            {
                sb.AppendLine(itm.Name);
                sb.AppendLine(" ");
                sb.AppendLine(itm.ThreadState.ToString());
            }

            return sb.ToString();
        }
    }
}
