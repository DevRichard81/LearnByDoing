using Gutenberg.Configuration;
using Gutenberg.Types.File;
using Gutenberg;
using System.Text;
using LearnByDoingCli;
using System.Net.Sockets;
using Gutenberg.Types.NetworkSocket;

Console.WriteLine("---------------------------");
Console.WriteLine("Welcome to Larning by Doing");
Console.WriteLine("---------------------------");

//
ISamples mySample;
//SampleFile   mySampleFile;
//SampleSocket mySampleSocket;
//SamplePipe   mySamplePipe;
//
mySample = new SamplePipe();
mySample.Run();