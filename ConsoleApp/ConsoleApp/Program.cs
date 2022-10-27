using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        GetMulipleBoolean(out bool item1, out bool item2);
        Console.WriteLine(item1);   // true;
        Console.WriteLine(item2);   // false;
    }

    public static void GetMulipleBoolean(out bool item1, out bool item2)
    {
        item1 = true;
        item2 = false;
    }
}
