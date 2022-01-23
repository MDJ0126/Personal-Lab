using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var option = Option.Builder.Create()
                                .SetPlayBGM(true)
                                .SetEffectBGM(true)
                                .SetPush(true)
                                .Build();

            Console.WriteLine($"IsPlayBGM: {option.IsPlayBGM}");
            Console.WriteLine($"IsEffectBGM: {option.IsEffectBGM}");
            Console.WriteLine($"IsPush: {option.IsPush}");

            // result:
            //IsPlayBGM: True
            //IsEffectBGM: True
            //IsPush: True
        }
    }
}