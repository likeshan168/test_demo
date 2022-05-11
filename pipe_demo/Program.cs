// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.IO.Pipes;

Console.WriteLine("Hello, World!");

Process process = new Process();
process.StartInfo.FileName = "pipe_child_demo.exe";

using (AnonymousPipeServerStream pipStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
{
    process.StartInfo.Arguments = pipStream.GetClientHandleAsString();
    process.StartInfo.UseShellExecute = false;
    process.Start();

    pipStream.DisposeLocalCopyOfClientHandle();

    using (StreamWriter sw = new StreamWriter(pipStream))
    {
        sw.AutoFlush = true;
        sw.WriteLine(Console.ReadLine());
    }
}

process.WaitForExit();
process.Close();