using System;
using System.Collections.Generic;
using System.Text;

public static class Utils
{
    private static StringBuilder _sb = new StringBuilder(1024);

    public static string AppendString(params string[] text)
    {
        _sb.Length = 0;
        for (int i = 0; i < text.Length; i++)
        {
            _sb.Append(text[i]);
        }
        return _sb.ToString();
    }
}