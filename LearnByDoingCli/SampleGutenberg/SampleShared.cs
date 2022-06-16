using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_Gutenberg;
using Project_Gutenberg.Configuration;

namespace LearnByDoingCli
{
    public class SampleShared
    {
        internal bool finish;
        internal IConfiguration? config;
        public Dictionary<string, IGutenberg>? Gutenbergs { get; init; }

        public virtual void Init()
        {
            Console.WriteLine("Start Init Prozess");
            if (Gutenbergs == null)
                return;

            foreach (var itm in Gutenbergs)
            {
                itm.Value.Init();
                Console.WriteLine(itm.Value.errorObject?.ToString());

                itm.Value.Start();
                Console.WriteLine(itm.Value.errorObject?.ToString());

                Console.WriteLine("Finish a Prozess");
            }

            Thread.Sleep(1000);
        }
        public virtual void ReportStatistics()
        {
            string Prefix;
            if (Gutenbergs == null)
                return;
            foreach (var itm in Gutenbergs)
            {
                Prefix = "[" + itm.Key + "]";
                Console.WriteLine(Prefix + itm.Value.statistic);
                Console.WriteLine(Prefix + itm.Value.errorObject);
            }
        }
        public virtual void InputReact()
        {
            Thread.Sleep(250);
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo result = Console.ReadKey();
                if (result.Key == ConsoleKey.Escape)
                    finish = true;
            }
        }
        protected static byte[]? ReceiveMessageIf(IGutenberg gutenberg)
        {
            if (gutenberg.HasMessage())
            {
                return gutenberg.Get();
            }
            return null;
        }
        

    }
}
