using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Project_Gutenberg.Error;
using Project_Gutenberg.Statistic;
using Project_Gutenberg.Statistics;

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
            cancellationTokenSource?.Cancel();
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

        public void AddThread(delegateVoidWithStatistic threadFunction, StatisticInterface statistic, ErrorObject? errorObject, IGutenbergBuffers gutenbergBuffers)
        {
            if (errorObject == null)
            {
                return;
            }
            if (threadFunction == null)
            {
                errorObject.Set(ErrorObject.ErrorType.Fatal, 0, "threadFunction can not be null", "AddThread");
                return;
            }
            if (statistic == null)
            {
                errorObject.Set(ErrorObject.ErrorType.Fatal, 0, "statistics can not be null", "AddThread");
                return;
            }
            if (cancellationTokenSource == null)
            {
                errorObject.Set(ErrorObject.ErrorType.Fatal, 0, "GutenbergThreads.cancellationTokenSource can not be null", "AddThread");
                return;
            }

            AddThread(new ThreadStart(
                () =>
                {
                    StatisticOfFunction statisticOfFunction = new();

                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            threadFunction(ref statisticOfFunction, gutenbergBuffers);
                            statistic.IncomingMessage = statisticOfFunction.handelMessage;
                            statistic.readData = statisticOfFunction.handelDataLength;
                            statisticOfFunction.Reset();
                            Thread.Sleep(100);
                        }
                        catch (SocketException ex)
                        {
                            errorObject.Set(ErrorObject.ErrorType.Error, 0, "SocketException [" + ex.Message + "]", "ThreadInside");
                        }
                        catch (Exception ex)
                        {
                            errorObject.Set(ErrorObject.ErrorType.Fatal, 0, "Unknow Exception [" + ex.Message + "]", "ThreadInside");
                        }
                    }
                    Interlocked.Decrement(ref statistic.runningThreads);
                })
            );
            Interlocked.Increment(ref statistic.runningThreads);
        }
        public void AddThread(delegateVoidWithStatisticSendRead threadFunction, StatisticInterface statistic, ErrorObject? errorObject, IGutenbergBuffers gutenbergBuffers)
        {
            if (errorObject == null)
            {
                return;
            }
            if (threadFunction == null)
            {
                errorObject.Set(ErrorObject.ErrorType.Fatal, 0, "threadFunction can not be null", "AddThread");
                return;
            }
            if (statistic == null)
            {
                errorObject.Set(ErrorObject.ErrorType.Fatal, 0, "statistics can not be null", "AddThread");
                return;
            }
            if (cancellationTokenSource == null)
            {
                errorObject.Set(ErrorObject.ErrorType.Fatal, 0, "GutenbergThreads.cancellationTokenSource can not be null", "AddThread");
                return;
            }

            AddThread(new ThreadStart(
                () =>
                {
                    StatisticOfFunction statisticOfFunctionSend = new();
                    StatisticOfFunction statisticOfFunctionRead = new();

                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            threadFunction(
                                ref statisticOfFunctionSend,
                                ref statisticOfFunctionRead,
                                gutenbergBuffers.BufferSend,
                                gutenbergBuffers.BufferReceive);

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
                            errorObject.Set(ErrorObject.ErrorType.Error, 0, "SocketException [" + ex.Message + "]", "ThreadInside");
                        }
                        catch (Exception ex)
                        {
                            errorObject.Set(ErrorObject.ErrorType.Fatal, 0, "Unknow Exception [" + ex.Message + "]", "ThreadInside");
                        }
                    }
                    Interlocked.Decrement(ref statistic.runningThreads);
                })
            );
            Interlocked.Increment(ref statistic.runningThreads);
        }
        private void AddThread(ThreadStart newThread)
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
