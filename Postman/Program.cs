using System.Text;
using Postman;
using PowerArgs;
using Project_Gutenberg;

class Program
{
    internal static IGutenberg? gutenberg;

    static void Main(string[] args)
    {
        Args.InvokeAction<PostmanArgs>(args);

        if(gutenberg != null)
        {
            gutenberg.Init();
            gutenberg.Start();

            bool isRunning = true;
            string line;
            while (isRunning)
            {
                if (Console.KeyAvailable)
                {
                    Console.WriteLine("Command:");
                    if((line = Console.ReadLine()) != null)
                    {
                        if (line.StartsWith("Exit"))
                        {
                            isRunning = false;
                        }
                        else
                        {
                            gutenberg.Put(Encoding.ASCII.GetBytes(line));
                        }
                    }
                }
                if (gutenberg.HasMessage())
                {
                    var recivedData = gutenberg.Get();
                    Console.WriteLine($"Received: [{Convert.ToString(recivedData)}][{BitConverter.ToString(recivedData)}]");
                }
                if (!gutenberg.HasConnection() && gutenberg.isServer)
                {
                    Console.WriteLine("No Connection open");
                    Thread.Sleep(500);
                }
                else if(!gutenberg.HasConnection() && !gutenberg.isServer)
                {
                    isRunning = false;
                    Console.WriteLine("No Connection open");
                }
                gutenberg.HasError();                
            }
            gutenberg.Terminated();
            gutenberg = null;
        }
        Console.WriteLine("Good Bye Postman");
    }
}