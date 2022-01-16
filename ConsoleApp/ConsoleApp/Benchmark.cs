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
        public static int resultLength = 15;
        public static int rankLength = 5;

        public string methodName;
        public double seconds;
        public string result;
        public short rank;

        public bool IsError => seconds <= 0f;

        public static void Clear()
        {
            methodLength = 15;
            resultLength = 15;
            rankLength = 5;
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

    /// <summary>
    /// 밴치마크 시작 및 기록
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static string Start(Method method)
    {
        return _start(method.Method.Name, () => method());
    }

    /// <summary>
    /// 메소드 실행 기록 가져오기
    /// </summary>
    /// <returns></returns>
    public static string GetRecord()
    {
        StringBuilder sb = new StringBuilder(1024);

        // Title
        sb.Append("\n[Benchmark Record]\n\n");
        sb.Append("Method".PadLeft(ExecuteRecord.methodLength)).Append(" |").Append("Result".PadLeft(ExecuteRecord.resultLength)).Append(" |").Append("Rank".PadLeft(ExecuteRecord.rankLength)).Append(" |").Append('\n');
        for (int i = 0; i < ExecuteRecord.methodLength; i++)
        {
            sb.Append('=');
        }
        sb.Append(" |");
        for (int i = 0; i < ExecuteRecord.resultLength; i++)
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
                if (_executeRecords[i].seconds > _executeRecords[j].seconds)
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
                sb.Append(_executeRecords[i].methodName.PadLeft(ExecuteRecord.methodLength)).Append(" |").Append(_executeRecords[i].result.PadLeft(ExecuteRecord.resultLength)).Append(" |").Append(" ".PadLeft(ExecuteRecord.rankLength)).Append(" |");
            }
            else
            {
                sb.Append(_executeRecords[i].methodName.PadLeft(ExecuteRecord.methodLength)).Append(" |").Append(_executeRecords[i].result.PadLeft(ExecuteRecord.resultLength)).Append(" |").Append(_executeRecords[i].rank.ToString().PadLeft(ExecuteRecord.rankLength)).Append(" |");
                if (_executeRecords[i].rank == 1)
                    sb.Append(" << Best!");
            }
            sb.Append('\n');
        }

        // Desciption
        sb.Append("\n * Legends * " +
            "\n - Method: Method name." +
            "\n - Result: Executed result content." +
            "\n - Rank: Rank from all method. ");

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
        string result = string.Empty;
        double seconds = 0;
        bool isSuccess = true;

        // Start

        // 1. 함수 반복 실행으로 속도가 안정화될 때까지 반복
        for (int i = 0; i < MAX_LOOP_PROCCESS_COUNT; i++)
        {
            _stopwatch.Restart();
            try
            {
                method.Invoke(); //method?.Invoke();
            }
            catch (Exception e)
            {
                result = "[Error] " + e.Message;
                isSuccess = false;
                break;
            }
            _stopwatch.Stop();

            double prevSeconds = seconds;
            seconds = 1000 * 1000 * 1000 * _stopwatch.ElapsedTicks / Stopwatch.Frequency;

            // 오차 범위가 좁혀졌을때 반복 중지
            if (Math.Abs(prevSeconds - seconds) < 1000)
                break;
        }

        // 2. 함수 반복 실행으로 평균값 산정
        if (isSuccess)
        {
            seconds = 0f;
            for (int i = 0; i < testCnt; i++)
            {
                _stopwatch.Restart();
                method.Invoke();
                _stopwatch.Stop();
                seconds += 1000 * 1000 * 1000 * _stopwatch.ElapsedTicks / Stopwatch.Frequency;
            }
            result = (seconds / testCnt).ToString() + " ns.";
        }

        // Caching Result Row
        _addRecord(methodName, seconds, result);

        return result;
    }

    private static void _addRecord(string methodName, double seconds, string result = "")
    {
        if (ExecuteRecord.methodLength < methodName.Length)
            ExecuteRecord.methodLength = methodName.Length + 5;

        if (ExecuteRecord.resultLength < result.Length)
            ExecuteRecord.resultLength = result.Length + 5;

        _executeRecords.Add(new ExecuteRecord { methodName = methodName, seconds = seconds, result = result });
    }

    #endregion
}