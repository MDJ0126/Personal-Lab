using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    class Program
    {
        static List<string> list = new List<string>();
        private static void Main(string[] args)
        {
            for (int i = 0; i < 500; i++)
            {
                list.Add(i.ToString());
            }
            Benchmark.Start(StringJoin);
            Benchmark.Start(StringBuilder);
            Console.WriteLine(Benchmark.GetRecord());
        }

        private static string StringJoin()
        {
            return string.Join(" ", list);
        }

        private static string StringBuilder()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append(list[i]);
                builder.Append(" ");
            }
            return builder.ToString();
        }
    }
}
