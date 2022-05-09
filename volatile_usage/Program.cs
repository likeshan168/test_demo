Worker worker = new Worker();
Thread workerThread = new Thread(worker.DoWork);
workerThread.Start();
Console.WriteLine("Main thread: starting worker thread...");

while (!workerThread.IsAlive) ;

Thread.Sleep(500);

worker.RequestStop();

workerThread.Join();
Console.WriteLine("Main thread：worker thread has terminated.");

public class Worker
{
    private bool _shouldStop;
    public void DoWork()
    {
        bool work = false;
        while (!_shouldStop)
        {
            work = !work;
        }
        Console.WriteLine("Work thread: terminating gracefully.");
    }

    public void RequestStop()
    {
        _shouldStop = true;
    }
}