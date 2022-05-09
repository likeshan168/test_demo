// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection();
services.AddSingleton<Channel<int>>(p => Channel.CreateUnbounded<int>());

var provider = services.BuildServiceProvider();

var myChannel = provider.GetRequiredService<Channel<int>>();

//var myChannel = Channel.CreateUnbounded<int>();


_ = Task.Factory.StartNew(async () =>
  {
      for (int i = 0; i < 10; i++)
      {
          await myChannel.Writer.WriteAsync(i);
      }

      myChannel.Writer.Complete();
  });



Task.Run(async () =>
{
    await foreach (var item in myChannel.Reader.ReadAllAsync())
    {
        Console.WriteLine(item);
        await Task.Delay(1000);
    }
});

Console.ReadLine();



