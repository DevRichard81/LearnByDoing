﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gutenberg.Configuration;
using Gutenberg.Error;
using Gutenberg.Statistic;
using Gutenberg.Types.NetworkSocket;

namespace Gutenberg.Types.Pipe
{
    public class PipeConnectionServer : IConnectionType
    {
        private IConfiguration? Configuration { get; set; }        
        private List<PipeStateObject> connections;
        private ErrorObject? _errorObject;
        public ErrorObject? ErrorObject { get { return _errorObject; } set { _errorObject = value; } }

        public void Init(IConfiguration newConfiguration)
        {
            Configuration = newConfiguration as ConfigurationPipes;
            connections = new List<PipeStateObject>();
        }

        public void Start()
        {
            var config = Configuration as ConfigurationPipes;
            for (int idx=0; idx < config.numThreads; idx++)
            {
                PipeStateObject pipeStateObject = new PipeStateObject();
                pipeStateObject.namePipeServerStream = new NamedPipeServerStream(
                    config.pipeName, 
                    config.pipeDirection,
                    config.numThreads,
                    config.pipeTransmissionMode,
                    PipeOptions.Asynchronous);                
                pipeStateObject.BeginConnect();                
                connections.Add(pipeStateObject);
            }
        }

        public void Close()
        {
            foreach(var itm in connections)
            {
                if (itm.namePipeServerStream.IsConnected)
                {
                    itm.namePipeServerStream.Flush();
                    itm.namePipeServerStream.Disconnect();
                    Thread.Sleep(10);
                }
                itm.namePipeServerStream.Close();            
            }
            Thread.Sleep(500);
            connections.Clear();
        }

        public void Read(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            var config = Configuration as ConfigurationPipes;
            byte[] reciveBuffer = new byte[config.reciveBufferSize];
            
            foreach (var itm in connections)
            {
                if (itm.namePipeServerStream.IsConnected)
                {
                    Array.Clear(reciveBuffer);
                    int byteRecv = itm.namePipeServerStream.Read(reciveBuffer, 0, config.reciveBufferSize);
                    if (byteRecv > 0)
                    {
                        buffer.Enqueue(reciveBuffer);
                        statisticOfFunction.handelDataLength += (uint)byteRecv;
                        statisticOfFunction.handelMessage += 1;
                    }
                }
            }
            statisticOfFunction.type = StatisticOfFunction.Type.Incoming;
            Thread.Sleep(1);
        }

        public void Write(ref StatisticOfFunction statisticOfFunction, ref ConcurrentQueue<byte[]> buffer)
        {
            byte[]? sendBuffer;
            bool wasSendOnce = false;
            if (buffer.TryDequeue(out sendBuffer))
            {
                foreach (var itm in connections)
                {
                    if (itm.namePipeServerStream.IsConnected)
                    {
                        itm.namePipeServerStream.Write(sendBuffer, 0, sendBuffer.Length);
                        itm.namePipeServerStream.WaitForPipeDrain();
                        wasSendOnce = true;
                    }
                }
                if (wasSendOnce) 
                {
                    statisticOfFunction.handelDataLength += (uint)sendBuffer.Length;
                    statisticOfFunction.handelMessage += 1;
                }
            }
            statisticOfFunction.type = StatisticOfFunction.Type.Outcoming;
            Thread.Sleep(1);
        }
    }
}