﻿using System.Text;
using Gutenberg;
using Gutenberg.Configuration;
using Gutenberg.Types.File;

namespace LearnByDoingCli
{
    internal class SampleFile : ISamples
    {
        public void Run()
        {
            Console.WriteLine("-------------------");
            Console.WriteLine("Doing a File Sample");
            Console.WriteLine("-------------------");

            ConfigurationFile config = new ConfigurationFile();
            config.baseDirectory = @"D:\Gutenberg\";
            config.incomeDirectory = "incoming";
            config.outcomeDirectory = "outcoming";
            config.errorDirectory = "error";

            config.sendFileSuffix = "suffix";
            config.sendFileDateFormat = "yyyyMMddHHmmssfffffff";
            config.sendFilePrefix = "prefix";
            config.sendFileExtension = "txt";

            Gutenberg<ConfigurationFile, FileConnection> gutenberg = new Gutenberg<ConfigurationFile, FileConnection>().Configuration(config, new FileConnection());
            gutenberg.Init();
            if (gutenberg.errorObject != null)
                Console.WriteLine(gutenberg.errorObject.ToString());
            gutenberg.Start();
            if (gutenberg.errorObject != null)
                Console.WriteLine(gutenberg.errorObject.ToString());

            bool finish = false;


            while (!finish)
            {
                Console.WriteLine(gutenberg.statistic.ToString());
                if (gutenberg.HasMessage() > 0)
                {
                    Console.WriteLine("Message was recived");
                    var message = gutenberg.Get();
                    Console.WriteLine("Message is:" + Encoding.ASCII.GetString(message));

                    var sendMessage = Encoding.ASCII.GetBytes("Thanks for your Message:" + Encoding.ASCII.GetString(message));
                    gutenberg.Put(sendMessage);
                }

                Thread.Sleep(250);
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo result = Console.ReadKey();
                    if (result.Key == ConsoleKey.Escape)
                        finish = true;
                }
            }

            gutenberg.Terminated();
            gutenberg = null;
        }
    }
}
