using Gutenberg.Configuration;
using Gutenberg.Statistics;
using Gutenberg.Error;
using Gutenberg.Types;
using Gutenberg.Statistic;
using System.Collections.Concurrent;

namespace Gutenberg
{
    public enum BufferIndex
    {
        Send = 0, 
        Receive = 1
    }

    public class Gutenberg<TConfig, TType>
    {
        private delegate void delegateVoidWithStatistic(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer);
        private CancellationTokenSource? cancellationTokenSource;

        private TConfig? configuration;
        private IConnectionType? type;
        private Thread[]? threads;
        public StatisticInterface? statistic;
        public ErrorObject? errorObject;

        private ConcurrentQueue<byte[]>[] buffers;

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
            InitBuffers();
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
            AddThread(type.Read, (int)BufferIndex.Receive);
            AddThread(type.Write, (int)BufferIndex.Send);
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
        public int HasMessage(int bufferIndex)
        {
            return buffers[bufferIndex].Count;
        }
        public byte[] Get(int bufferIndex)
        {
            if (buffers[bufferIndex].Count == 0)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Warning, 0, "Try to Recive Empty data from buffer", "Get");
                return null;
            }

            byte[]? recMessage;
            while (!buffers[bufferIndex].TryDequeue(out recMessage))
            {
                Thread.Sleep(5);
            }
            return recMessage;
        }
        public void Put(int bufferIndex, byte [] setMessage)
        {
            buffers[bufferIndex].Enqueue(setMessage);
        }


        private void InitThreads()
        {
            threads = new Thread[2];
        }
        private void InitBuffers()
        {
            buffers = new ConcurrentQueue<byte[]>[2];
            for(int i = 0; i < buffers.Length; i++)
            {
                buffers[i] = new ConcurrentQueue<byte[]>();
            }
        }
        private void AddThread(delegateVoidWithStatistic threadFunction, int bufferIndex)
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
            if (statistic == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "statistics can not be null", "AddThread");
                return;
            }
            if (cancellationTokenSource == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "cancellationTokenSource can not be null", "AddThread");
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
                            threadFunction(ref statisticOfFunction, ref buffers[bufferIndex]);
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