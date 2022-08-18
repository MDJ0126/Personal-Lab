using System;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        MyTimer.Start();
        Thread.Sleep(1000 * 60 * 60 * 12)	// 12시간 딜레이

        // OS 타이머의 현재 시간
        Console.WriteLine(DateTime.Now);

        // 직접 작성한 프로그램 타이머 현재 시간
        Console.WriteLine(MyTimer.NowTime);
    }
}
