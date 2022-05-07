using LearnByDoingCli;

Console.WriteLine("----------------------------");
Console.WriteLine("Welcome to Learning by Doing");
Console.WriteLine("----------------------------");

//
//ISamples mySample = new SampleMendel();
//ISamples mySample = mySampleFile;
//
ISamples mySample = new SamplePipe();
//ISamples mySample = new SampleSocket();
mySample.Run();