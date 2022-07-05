using System;

class Program
{
    private static void Main(string[] args)
    {
        ShiftComparer shiftComparer = ShiftComparer.Init();
        shiftComparer.Add(1);                           // 1번째 추가
        Console.WriteLine(shiftComparer.IsExist(1));    // 1번째 존재 여부 확인 ==> True
        Console.WriteLine(shiftComparer.IsExist(2));    // 2번째 존재 여부 확인 ==> False
        shiftComparer.Remove(1);                        // 1번째 제거
        Console.WriteLine(shiftComparer.IsExist(1));    // 1번째 존재 여부 확인 ==> False
    }
}