using Gutenberg.Configuration;
using Gutenberg.Types.File;
using Gutenberg;

Console.WriteLine("---------------------------");
Console.WriteLine("Welcome to Larning by Doing");
Console.WriteLine("---------------------------");

ConfigurationFile config = new ConfigurationFile();
config.baseDirectory = @"D:\Gutenberg\";
config.incomeDirectory = "incoming";
config.outcomeDirectory = "outcoming";
config.errorDirectory = "error";


Gutenberg<ConfigurationFile, FileConnection> gutenberg = new Gutenberg<ConfigurationFile, FileConnection>().Configuration(config, new FileConnection());
gutenberg.Init();
if (gutenberg.errorObject != null)
    Console.WriteLine(gutenberg.errorObject.ToString());
gutenberg.Start();
if (gutenberg.errorObject != null)
    Console.WriteLine(gutenberg.errorObject.ToString());

bool finish = false;

while(!finish)
{
    Console.WriteLine(gutenberg.statistic.ToString());
    Thread.Sleep(1000);
}
