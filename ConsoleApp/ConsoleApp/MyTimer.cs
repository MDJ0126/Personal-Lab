using System;
using System.Threading.Tasks;

/// <summary>
/// 프로그램 커스텀 타이머
/// </summary>
public static class MyTimer
{
    static DateTime startTime;
    static long millisecond = 0L;

    public static void Start()
    {
        startTime = DateTime.Now;
        TimeUpdater();
    }

    private static async void TimeUpdater()
    {
        while (true)
        {
            await Task.Delay(1);
            ++millisecond;
        }
    }

    public static DateTime NowTime => startTime.AddMilliseconds(millisecond);
}