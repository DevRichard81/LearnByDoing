using Project_Gutenberg.Configuration;
using Project_Gutenberg.Statistics;
using Project_Gutenberg.Error;
using Project_Gutenberg.Types;
using Project_Gutenberg.Statistic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Project_Gutenberg.GutenbergShared;

namespace Project_Gutenberg
{
    public class GutenbergAsync : Gutenberg, IGutenberg
    {
        private IConnectionTypeAsync? typeAsync;      

        ~GutenbergAsync() {
            Terminated();
            Thread.Sleep(100);            
        }
        public GutenbergAsync Configuration(IConfiguration newConfiguration, IConnectionTypeAsync newType)
        {
            Configuration(newConfiguration);
            typeAsync = newType;
            return this;
        }
        public override void Init()
        {
            if (configuration == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, "Configuration can not be null", "Init");
                return;
            }
            if (typeAsync == null)
            {
                errorObject = new ErrorObject().Set(ErrorObject.ErrorType.Fatal, 0, nameof(typeAsync) + " can not be null", "Init");
                return;
            }
            typeAsync.Init(configuration as IConfiguration);

            AddThread(typeAsync.ReadWrite);
        }
        public override void Start()
        {
            typeAsync.Start();
            GutenbergThreads.Start();
        }
        internal void AddThread(GutenbergThreads.delegateVoidWithStatisticSendRead threadFunction)
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
                    StatisticOfFunction statisticOfFunctionSend = new();
                    StatisticOfFunction statisticOfFunctionRead = new();

                    while (!GutenbergThreads.cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            threadFunction(
                                ref statisticOfFunctionSend,
                                ref statisticOfFunctionRead,
                                GutenbergBuffers.BufferSend,
                                GutenbergBuffers.BufferReceive);

                            statistic.IncomingMessage = statisticOfFunctionRead.handelMessage;
                            statistic.readData = statisticOfFunctionRead.handelDataLength;
                            statistic.OutcomingMessage = statisticOfFunctionSend.handelMessage;
                            statistic.writeData = statisticOfFunctionSend.handelDataLength;
                            statisticOfFunctionSend.Reset();
                            statisticOfFunctionRead.Reset();
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
        public override void Terminated()
        {
            base.Terminated();
            typeAsync?.Close();
        }
        public override void HasError()
        {
            if (errorObject != null)
            {
                Console.WriteLine("Error in " + nameof(Gutenberg));
                Console.WriteLine(errorObject.ToString());
            }

            if (typeAsync.ErrorObject != null)
            {
                Console.WriteLine("Error in ConnectionType" + typeAsync.GetType().Name);
                Console.WriteLine(typeAsync.ErrorObject.ToString());
            }
        }      
    }
}