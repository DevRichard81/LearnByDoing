using LearnByDoingCli;

Console.WriteLine("----------------------------");
Console.WriteLine("Welcome to Learning by Doing");
Console.WriteLine("----------------------------");

//
// Mendel
//ISamples mySample = new SampleMendel();
//
// Gutenberg
//ISamples mySample = mySampleFile;
//ISamples mySample = new SamplePipe();
//ISamples mySample = new SampleSocket();
//ISamples mySample = new SampleSocketAsync();
//
// CSharpMagic
ISamples mySample = new SampleCSharpMagic();
//
mySample.Run();