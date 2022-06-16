using Project_Gutenberg.Configuration;
using Project_Gutenberg.Statistics;
using Project_Gutenberg.Error;
using Project_Gutenberg.Types;
using Project_Gutenberg.Statistic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using Project_Gutenberg.GutenbergShared;

namespace Project_Gutenberg
{
    public class Gutenberg : IGutenberg
    {
        protected IGutenbergBuffers GutenbergBuffers { get; set; }
        protected GutenbergThreads GutenbergThreads;
        protected IConfiguration? configuration;
        private IConnectionType? type;
        public StatisticInterface? statistic { get; set; }
        public ErrorObject? errorObject { get; set; }

        public Gutenberg()
        {
            statistic = new StatisticInterface();
            GutenbergBuffers = new GutenbergBuffers();
            GutenbergThreads = new GutenbergThreads();
        }

        ~Gutenberg() {
            Terminated();
            Thread.Sleep(100);
        }
        public void Configuration(IConfiguration newConfiguration)
        {
            configuration = newConfiguration;
        }

        public Gutenberg Configuration(IConfiguration newConfiguration, IConnectionType newType)
        {
            Configuration(newConfiguration);
            type = newType as IConnectionType;
            return this;
        }

        public virtual void Init()
        {
            if (configuration == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "Configuration can not be null", "Init");
                return;
            }
            if (type == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, nameof(type) + " can not be null", "Init");
                return;
            }
            type.Init(configuration as IConfiguration);
            AddThread(type.Read);
            AddThread(type.Write);
        }
        public virtual void Start()
        { 
            type.Start();
            GutenbergThreads.Start();
        }
        public virtual void Terminated() => type?.Close();
        public bool HasMessage() => GutenbergBuffers.HasMessage();

        public virtual void HasError()
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
            Console.WriteLine(GutenbergThreads.ToString());
            Console.WriteLine("Buffers:");
            Console.WriteLine(GutenbergBuffers.ToString());
        }

        internal void AddThread(GutenbergThreads.delegateVoidWithStatistic threadFunction)
        {
            if (threadFunction == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "threadFunction can not be null", "AddThread");
                return;
            }
            if (GutenbergThreads == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "GutenbergThreads can not be null", "AddThread");
                return;
            }
            if (statistic == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "statistics can not be null", "AddThread");
                return;
            }
            if (GutenbergThreads.cancellationTokenSource == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "GutenbergThreads.cancellationTokenSource can not be null", "AddThread");
                return;
            }

            GutenbergThreads.Addthread(new ThreadStart(
                () =>
                {
                    StatisticOfFunction statisticOfFunction = new();

                    while (!GutenbergThreads.cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            threadFunction(ref statisticOfFunction, GutenbergBuffers);
                            statistic.IncomingMessage = statisticOfFunction.handelMessage;
                            statistic.readData = statisticOfFunction.handelDataLength;
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
                })
            );
            Interlocked.Increment(ref statistic.runningThreads);
        }    

        public virtual void Put(byte[] setMessage)
        {
            GutenbergBuffers.AddSendMessage(setMessage);
        }

        public virtual byte[] Get()
        {
            return GutenbergBuffers.GetReceivedMessage();
        }
    }
}