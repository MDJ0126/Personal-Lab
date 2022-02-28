using System;

public class Example
{
    [Flags]
    private enum eCheck : byte
    {
        None        = 0,
        Apple       = 1 << 0,   // 2의 1승
        Banana      = 1 << 1,   // 2의 2승
        Pineapple   = 1 << 2,   // 2의 3승
        All         = byte.MaxValue,
    }

    private eCheck check = eCheck.None;

    public void Main()
    {
        // 추가
        check |= eCheck.Apple;
        check |= eCheck.Banana;
        check |= eCheck.Pineapple;
        Console.WriteLine(check);   // = Apple | Banana | Pineapple;

        // Apple 있는지 체크
        bool isExist = (check & eCheck.Apple) != 0;
        Console.WriteLine($"Apple Exist Resut: {isExist}");

        // 제거
        check &= ~eCheck.Apple;
        check &= ~eCheck.Banana;
        check &= ~eCheck.Pineapple;
        Console.WriteLine(check);   // = None;

        // Apple 있는지 체크
        isExist = (check & eCheck.Apple) != 0;
        Console.WriteLine($"Apple Exist Resut: {isExist}");

        // Results :
        // Apple, Banana, Pineapple
        // Apple Exist Resut: True
        // None
        // Apple Exist Resut: False
    }
}