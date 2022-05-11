// See https://aka.ms/new-console-template for more information
using System.IO.Pipes;

Console.WriteLine("Hello, World!");


using (StreamReader sr = new StreamReader(new AnonymousPipeClientStream(PipeDirection.In, args[0])))
{
    string line;
    while ((line = sr.ReadLine()) != null)
    {
        Console.WriteLine("Echo: {0}", line);
    }
}