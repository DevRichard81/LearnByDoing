using Gutenberg.Configuration;
using Gutenberg.Statistics;
using Gutenberg.Error;
using Gutenberg.Types;
using Gutenberg.Statistic;
using System.Collections.Concurrent;

namespace Gutenberg
{
    public class Gutenberg<TConfig, TType>
    {
        private delegate void delegateVoidWithStatistic(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> );
        private CancellationTokenSource? cancellationTokenSource;

        private TConfig? configuration;
        private IConnectionType? type;
        private Thread[]? threads;
        public StatisticInterface? statistic;
        public ErrorObject? errorObject;

        private ConcurrentQueue<byte[]> ReadBuffers;
        private ConcurrentQueue<byte[]> WritteBuffers;

        ~Gutenberg() {
            Terminated();
            Thread.Sleep(100);            
        }

        public Gutenberg<TConfig, TType> Configuration(TConfig newConfiguration, TType newType)
        {
            configuration = newConfiguration;
            type = newType as IConnectionType;
            cancellationTokenSource = new CancellationTokenSource();            
            statistic = new StatisticInterface();
            InitThreads();
            ReadBuffers = new ConcurrentQueue<byte[]>();
            WritteBuffers = new ConcurrentQueue<byte[]>();
            return this;
        }

        public void Init()
        {
            if (configuration == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "Configuration can not be null", "Init");
                return;
            }
            if (type == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "Typen can not be null", "Init");
                return;
            }
            type.Init(configuration as IConfiguration);
            AddThread(type.Read);
            AddThread(type.Write);
        }
        public void Start()
        {
            byte anyErros = 0;
            for (int i = 0; i < threads.Length; i++)
            {
                if (threads[i] == null)
                    anyErros++;
                else
                {
                    threads[i].Start();
                }
            }
            if (anyErros != 0)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Error, 0, "By starting of Threads there was unexpected [" + anyErros + "] failurs", "Start");
            }
        }
        public void Terminated()
        {
            cancellationTokenSource.Cancel();
            while (statistic.runningThreads != 0)
            {
                Thread.Sleep(100);
            }
            InitThreads();
        }

        private void InitThreads()
        {
            threads = new Thread[2];
        }
        private void AddThread(delegateVoidWithStatistic threadFunction, ref ReadBuffers)
        {
            if (threadFunction == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "threadFunction can not be null", "AddThread");
                return;
            }
            if (threads == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "threads can not be null", "AddThread");
                return;
            }

            for (int i = 0; i < threads.Length; i++)
            {
                if(threads[i] == null)
                {
                    threads[i] = new Thread(() => 
                    {
                        StatisticOfFunction statisticOfFunction = new StatisticOfFunction() { type = StatisticOfFunction.Type.Incoming};

                        while (!cancellationTokenSource.IsCancellationRequested)
                        {
                            threadFunction(ref statisticOfFunction);
                            switch (statisticOfFunction.type)
                            {
                                case StatisticOfFunction.Type.Incoming:
                                    statistic.IncomingMessage = statisticOfFunction.handelMessage;
                                    statistic.readData = statisticOfFunction.handelDataLength;
                                    break;
                                case StatisticOfFunction.Type.Outcoming:
                                    statistic.OutcomingMessage = statisticOfFunction.handelMessage;
                                    statistic.writeData = statisticOfFunction.handelDataLength;
                                    break;
                            }
                            statisticOfFunction.Reset();
                            Thread.Sleep(100);
                        }
                        Interlocked.Decrement(ref statistic.runningThreads);
                    });
                    Interlocked.Increment(ref statistic.runningThreads);
                    break;
                }
            }

        }

    }
}