//模拟多线程操作
var calculationHelper = new CalculationHelper();
int ordinaryValue = 0;
Random random = new Random();
ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false);

Thread thread1 = new Thread(new ThreadStart(Test));
thread1.Name = "线程1";
thread1.Start();

Thread thread2 = new Thread(new ThreadStart(Test));
thread2.Name = "线程2";
thread2.Start();

manualResetEvent.Set();

thread1.Join();
thread2.Join();

Console.WriteLine($"通过线程安全的方式计算结果：{calculationHelper.Total}，非线程安全计算出的结果为：{ordinaryValue}");

void Test()
{
    manualResetEvent.Wait();
    for (int i = 1; i <= 10000; i++)
    {
        //int rndValue = random.Next(10000);
        ordinaryValue += i;
        calculationHelper.AddTotal(i);
    }
}

/// <summary>
/// 计算帮助类
/// </summary>
public class CalculationHelper
{
    private int total = 0;
    public int Total { get { return total; } }

    /// <summary>
    /// 累加
    /// </summary>
    /// <param name="value">需要加的值</param>
    /// <returns></returns>
    public int AddTotal(int value)
    {
        if (value == 0)
        {
            return value;
        }
        int localValue, compuetedValue;
        do
        {
            localValue = total;
            compuetedValue = localValue + value;
        } while (localValue != Interlocked.CompareExchange(ref total, compuetedValue, localValue));//说明计算成功了

        return compuetedValue;
    }
}
