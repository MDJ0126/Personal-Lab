using System;
using System.Collections.Generic;
using System.Text;

public static class Logger
{
    public delegate void OnAddLog(string text);
    private static event OnAddLog onLog;
    public static event OnAddLog OnLog
    {
        add
        {
            onLog -= value;
            onLog += value;
        }
        remove
        {
            onLog -= value;
        }
    }

    private const int LOG_COUNT_MAX = 20;

    public static Queue<object> Logs { get; private set; } = new Queue<object>();

    private static StringBuilder sb = new StringBuilder();

    public static void Log(object obj)
    {
        sb.Length = 0;
        Add(sb.Append("[Log] ").Append(obj).ToString());
    }

    public static void Warning(object obj)
    {
        sb.Length = 0;
        Add(sb.Append("[LogWarning] ").Append(obj).ToString());
    }

    public static void Error(object obj)
    {
        sb.Length = 0;
        Add(sb.Append("[LogError] ").Append(obj).ToString());
    }

    public static string GetLogs()
    {
        sb.Length = 0;
        var enumerator = Logs.GetEnumerator();
        while (enumerator.MoveNext())
        {
            sb.Append(enumerator.Current).Append('\n');
        }
        return sb.ToString();
    }

    private static void Add(string text)
    {
        // 추가
        Logs.Enqueue(text);

        // 정리
        if (Logs.Count > LOG_COUNT_MAX)
            Logs.Dequeue();

        // Console 출력
        Console.WriteLine(text);

        // 로그 추가 이벤트 발생
        onLog?.Invoke(text);
    }
}