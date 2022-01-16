using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Benchmark.Start(LoopTest1);
            Benchmark.Start(LoopTest2);
            Benchmark.Start(OutOfRangeException);
            Benchmark.Start(LoopTest3);
            Benchmark.Start(LoopTest4);
            Console.WriteLine(Benchmark.GetRecord());
        }

        static void OutOfRangeException()
        {
            int[] a = new int[1];
            a[2] = 20;
        }

        static void LoopTest1()
        {
            for (int i = 0; i < 1999; i++)
            {

            }
        }

        static void LoopTest2()
        {
            for (int i = 0; i < 9999; i++)
            {

            }
        }

        static void LoopTest3()
        {
            for (int i = 0; i < 999; i++)
            {

            }
        }

        static void LoopTest4()
        {
            for (int i = 0; i < 9999; i++)
            {

            }
        }
    }
}