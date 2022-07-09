using System;

class Program
{
    private static void Main(string[] args)
    {
        ShiftComparer checks = ShiftComparer.Init();
        checks.Add(1);                           // 1번째 추가
        Console.WriteLine(checks.IsExist(1));    // 1번째 존재 여부 확인 ==> True
        Console.WriteLine(checks.IsExist(2));    // 2번째 존재 여부 확인 ==> False
        checks.Remove(1);                        // 1번째 제거
        Console.WriteLine(checks.IsExist(1));    // 1번째 존재 여부 확인 ==> False
    }
}