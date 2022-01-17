using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class Benchmark
{
    public delegate object Method();
    private class ExecuteRecord
    {
        public static int methodLength = 15;
        public static int secondsLength = 15;
        public static int rankLength = 5;
        public static int memoryLength = 10;

        public string methodName;
        public double ticks;
        public short rank;
        public long memory;
        public string message;

        public bool IsError => !string.IsNullOrEmpty(message);

        public string GetSeconds()
        {
            return $"{1000 * 1000 * 1000 * ticks / Stopwatch.Frequency:F3} ns";
        }

        public string GetMemory()
        {
            int mok = 0;
            long tempMem = memory < 8192 ? 8192 : memory;
            while (tempMem > 1024)
            {
                tempMem /= 1024;
                mok++;
            }
            switch (mok)
            {
                case 0:
                    return $"{tempMem} B";
                case 1:
                    return $"{tempMem} KB";
                case 2:
                    return $"{tempMem} MB";
                case 3:
                    return $"{tempMem} GB";
                case 4:
                    return $"{tempMem} TB";
                default:
                    return $"{memory} B";
            }
        }

        public static void Clear()
        {
            methodLength = 15;
            secondsLength = 15;
            rankLength   = 5;
            memoryLength = 10;
        }
    }

    private static List<ExecuteRecord> _executeRecords = new List<ExecuteRecord>();

    /// <summary>
    /// 밴치마크 시작 및 기록
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static string Start(Action method)
    {
        return _start(method.Method.Name, method);
    }

    ///// <summary>
    ///// 밴치마크 시작 및 기록 (.Net Core 3.1에서는 주석 풀어도 이상 없음)
    ///// </summary>
    ///// <param name="method"></param>
    ///// <returns></returns>
    //public static string Start(Method method)
    //{
    //    return _start(method.Method.Name, () => method());
    //}

    /// <summary>
    /// 메소드 실행 기록 가져오기
    /// </summary>
    /// <returns></returns>
    public static string GetRecord()
    {
        StringBuilder sb = new StringBuilder(1024);

        // Title
        sb.Append("\n[Benchmark Record]\n\n");
        sb.Append("Method".PadLeft(ExecuteRecord.methodLength)).Append(" |").Append("Run-time".PadLeft(ExecuteRecord.secondsLength)).Append(" |").Append("Memory".PadLeft(ExecuteRecord.memoryLength)).Append(" |").Append("Rank".PadLeft(ExecuteRecord.rankLength)).Append(" |").Append('\n');
        for (int i = 0; i < ExecuteRecord.methodLength; i++)
        {
            sb.Append('=');
        }
        sb.Append(" |");
        for (int i = 0; i < ExecuteRecord.secondsLength; i++)
        {
            sb.Append('=');
        }
        sb.Append(":|");
        for (int i = 0; i < ExecuteRecord.memoryLength; i++)
        {
            sb.Append('=');
        }
        sb.Append(":|");
        for (int i = 0; i < ExecuteRecord.rankLength; i++)
        {
            sb.Append('=');
        }
        sb.Append(":|");
        sb.Append('\n');

        // Rank calculation
        for (int i = 0; i < _executeRecords.Count; i++)
        {
            _executeRecords[i].rank = 1;
            for (int j = 0; j < _executeRecords.Count; j++)
            {
                if (_executeRecords[i].ticks > _executeRecords[j].ticks)
                    _executeRecords[i].rank++;
            }
        }

        for (int i = 0; i < _executeRecords.Count; i++)
        {
            if (_executeRecords[i].IsError)
            {
                for (int j = 0; j < _executeRecords.Count; j++)
                {
                    _executeRecords[j].rank--;
                }
                _executeRecords[i].rank = -1;
            }
        }

        // Contents
        for (int i = 0; i < _executeRecords.Count; i++)
        {
            if (_executeRecords[i].IsError)
            {
                sb.Append(_executeRecords[i].methodName.PadLeft(ExecuteRecord.methodLength)).Append(" |").Append(_executeRecords[i].message);
            }
            else
            {
                sb.Append(_executeRecords[i].methodName.PadLeft(ExecuteRecord.methodLength)).Append(" |").Append(_executeRecords[i].GetSeconds().PadLeft(ExecuteRecord.secondsLength)).Append(" |").Append($"{_executeRecords[i].GetMemory()}".PadLeft(ExecuteRecord.memoryLength)).Append(" |").Append(_executeRecords[i].rank.ToString().PadLeft(ExecuteRecord.rankLength)).Append(" |");
                if (_executeRecords[i].rank == 1)
                    sb.Append(" << Best!");
            }
            sb.Append('\n');
        }

        // Desciption
        sb.Append("\n * Legends * " +
            "\n - Method: Method name." +
            "\n - Result: Executed result content." +
            "\n - Rank: Rank from all method. " +
            "\n");

        // Return Record
        return sb.ToString();
    }

    /// <summary>
    /// 밴치마크 기록 초기화
    /// </summary>
    public static void Clear()
    {
        _executeRecords.Clear();
        ExecuteRecord.Clear();
    }

    #region internal Method

    private static Stopwatch _stopwatch = null;
    private static int MAX_LOOP_PROCCESS_COUNT = 50;

    static Benchmark()
    {
        _initialize();
    }

    private static void _initialize()
    {
        _stopwatch = Stopwatch.StartNew();
        Clear();
    }

    private static string _start(string methodName, Action method, int testCnt = 100000)
    {
        // Ready
        string message = string.Empty;
        bool isSuccess = true;

        // Start

        // 1. 함수 반복 실행으로 속도가 안정화될 때까지 반복
        double currentSeconds = 0;
        for (int i = 0; i < MAX_LOOP_PROCCESS_COUNT; i++)
        {
            _stopwatch.Restart();
            try
            {
                method.Invoke(); //method?.Invoke();
            }
            catch (Exception e)
            {
                message = "[Error] " + e.Message;
                isSuccess = false;
                break;
            }
            _stopwatch.Stop();

            double prevSeconds = currentSeconds;
            currentSeconds = _stopwatch.ElapsedTicks;

            // 오차 범위가 좁혀졌을때 반복 중지
            if (Math.Abs(prevSeconds - currentSeconds) < 10)
                break;
        }

        // 2. 함수 반복 실행으로 평균값 산정
        double ticks = 0f;
        long memory = 0;
        if (isSuccess)
        {
            for (int i = 0; i < testCnt; i++)
            {
                _stopwatch.Restart();
                method.Invoke();
                _stopwatch.Stop();
                ticks += _stopwatch.ElapsedTicks;
            }

            ticks = ticks / testCnt;

            GC.Collect();
            long beforeMem = GC.GetTotalMemory(false);
            method.Invoke();
            long afterMem = GC.GetTotalMemory(false);
            memory = afterMem - beforeMem;
        }

        // Caching Result Row
        _addRecord(methodName, ticks, memory, message);

        return message;
    }

    private static void _addRecord(string methodName, double ticks, long memory, string message = "")
    {
        if (ExecuteRecord.methodLength < methodName.Length)
            ExecuteRecord.methodLength = methodName.Length + 5;

        _executeRecords.Add(new ExecuteRecord { methodName = methodName, ticks = ticks, memory = memory, message = message });
    }

    #endregion
}