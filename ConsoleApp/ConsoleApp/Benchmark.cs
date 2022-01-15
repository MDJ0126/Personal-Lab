using System;
using System.Collections.Generic;
using System.Text;

public class Benchmark
{
    private class ExecuteRecord
    {
        public static int methodLength = 15;
        public static int resultLength = 15;
        public static int rankLength = 5;

        public string methodName;
        public double seconds;
        public string result;
        public short rank;

        public string GetRecord()
        {
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            else
            {
                return seconds.ToString("F6") + " s.";
            }
        }

        public static void Clear()
        {
            methodLength = 15;
            resultLength = 15;
            rankLength = 5;
        }
    }

    private static List<ExecuteRecord> _executeRecords = new List<ExecuteRecord>();
    
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
            if (!string.IsNullOrEmpty(_executeRecords[i].result))
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
            if (!string.IsNullOrEmpty(_executeRecords[i].result))
            {
                sb.Append(_executeRecords[i].methodName.PadLeft(ExecuteRecord.methodLength)).Append(" |").Append(_executeRecords[i].GetRecord().PadLeft(ExecuteRecord.resultLength)).Append(" |").Append(" ".PadLeft(ExecuteRecord.rankLength)).Append(" |").Append('\n');
            }
            else
            {
                sb.Append(_executeRecords[i].methodName.PadLeft(ExecuteRecord.methodLength)).Append(" |").Append(_executeRecords[i].GetRecord().PadLeft(ExecuteRecord.resultLength)).Append(" |").Append(_executeRecords[i].rank.ToString().PadLeft(ExecuteRecord.rankLength)).Append(" |").Append('\n');
            }
        }

        // Desciption
        sb.Append("\n * Legends * " +
            "\n - Method: Method name." +
            "\n - Result: Executed result content." +
            "\n - Rank: Rank from all method. ");

        // Return Record
        return sb.ToString();
    }

    public static string Start(Action method)
    {
        // Ready
        DateTime startTime = DateTime.Now;
        double seconds = 0f;
        string result = string.Empty;

        // Start
        try
        {
            method.Invoke(); //method?.Invoke();
            seconds = (DateTime.Now - startTime).TotalSeconds;
        }
        catch (Exception e)
        {
            result = "[Error] " + e.Message;
        }

        // Caching Result Row
        AddRow(method.Method.Name, seconds, result);

        return result;
    }

    private static void AddRow(string methodName, double seconds, string result = "")
    {
        if (ExecuteRecord.methodLength < methodName.Length)
            ExecuteRecord.methodLength = methodName.Length + 5;

        if (!string.IsNullOrEmpty(result))
        {
            if (ExecuteRecord.resultLength < result.Length)
                ExecuteRecord.resultLength = result.Length + 5;
        }
        else
        {
            if (ExecuteRecord.resultLength < (seconds.ToString("F6") + " s.").Length)
                ExecuteRecord.resultLength = (seconds.ToString("F6") + " s.").Length + 5;
        }

        _executeRecords.Add(new ExecuteRecord { methodName = methodName, seconds = seconds, result = result });
    }

    public static void Clear()
    {
        _executeRecords.Clear();
        ExecuteRecord.Clear();
    }
}