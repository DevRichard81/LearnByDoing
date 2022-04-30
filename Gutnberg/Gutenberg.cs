using Gutenberg.Configuration;
using Gutenberg.Statistics;
using Gutenberg.Error;
using Gutenberg.Types;
using Gutenberg.Statistic;
using System.Collections.Concurrent;
using System.Net.Sockets;

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
            type.Start();
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
            type.Close();
            InitThreads();
        }
        public int HasMessage()
        {
            int bufIdx = (int)BufferIndex.Receive;
            return buffers[bufIdx].Count;
        }
        public byte[] Get()
        {
            int bufIdx = (int)BufferIndex.Receive;
            if (buffers[bufIdx].Count == 0)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Warning, 0, "Try to Recive Empty data from buffer", "Get");
                return null;
            }

            byte[]? recMessage;
            while (!buffers[bufIdx].TryDequeue(out recMessage))
            {
                Thread.Sleep(5);
            }
            return recMessage;
        }
        public void Put(byte [] setMessage)
        {
            int bufIdx = (int)BufferIndex.Send;
            buffers[bufIdx].Enqueue(setMessage);
        }

        public void HasError()
        {
            if (errorObject != null)
            {
                Console.WriteLine("Error in " + nameof(Gutenberg));
                Console.WriteLine(errorObject.ToString());
            }

            if (type.ErrorObject != null)
            {
                Console.WriteLine("Error in ConnectionType" + type.GetType().Name);
                Console.WriteLine(type.ErrorObject.ToString());
            }
        }
        public void Status()
        {
            Console.WriteLine("Threads:");
            if (threads == null)
            {
                Console.WriteLine("Threads not init");
            }
            else 
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    Console.WriteLine("ID " + i + " Status " + threads[i].IsAlive + threads[i].ThreadState.ToString());
                } 
            }
            Console.WriteLine("Buffers:");
            if (buffers == null || buffers.Length == 0)
            {
                Console.WriteLine("Buffers not init");
            }
            else
            {
                for (int i = 0; i < buffers.Length; i++)
                {
                    Console.WriteLine("ID " + i + " Count " + buffers[i].Count);
                }
            }
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

            if (threads[bufferIndex] == null)
            {
                threads[bufferIndex] = new Thread(() =>
                {
                    StatisticOfFunction statisticOfFunction = new StatisticOfFunction();

                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            threadFunction(ref statisticOfFunction, ref buffers[bufferIndex]);
                            switch (statisticOfFunction.type)
                            {
                                case StatisticOfFunction.Type.Incoming:
                                    Console.WriteLine("AddThread[" + bufferIndex + "] IN");
                                    statistic.IncomingMessage = statisticOfFunction.handelMessage;
                                    statistic.readData = statisticOfFunction.handelDataLength;
                                    break;
                                case StatisticOfFunction.Type.Outcoming:
                                    Console.WriteLine("AddThread[" + bufferIndex + "] OUT");
                                    statistic.OutcomingMessage = statisticOfFunction.handelMessage;
                                    statistic.writeData = statisticOfFunction.handelDataLength;
                                    break;
                                default:
                                    Console.WriteLine("StatisticOfFunction was unknow");
                                    break;
                            }
                            statisticOfFunction.Reset();
                            Thread.Sleep(100);
                        }
                        catch (SocketException ex)
                        {
                            errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Error, 0, "SocketException [" + ex.Message + "]", "ThreadInside");
                        }
                        catch (Exception ex)
                        {
                            errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "Unknow Exception [" + ex.Message + "]", "ThreadInside");
                        }
                    }
                    Interlocked.Decrement(ref statistic.runningThreads);
                });
                Interlocked.Increment(ref statistic.runningThreads);
            }
        }
    }
}