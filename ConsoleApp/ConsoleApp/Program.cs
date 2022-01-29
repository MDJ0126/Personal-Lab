using System;
using System.Collections.Generic;
class Program
{
    private static List<string> records = new List<string>();
    static void Main(string[] args)
    {
        // 구독 (이벤트 발생마다 텍스트 기록)
        NotificationCenter.Subscribes += text => records.Add(text);

        // 캐릭터 생성
        Player player = new Player();
        Monster monster = new Monster();

        // 대결 시작
        while (!player.IsDead() && !monster.IsDead())
        {
            if (!player.IsDead())
                player.Attack(monster);

            if (!monster.IsDead())
                monster.Attack(player);
        }

        // 대결 결과
        for (int i = 0; i < records.Count; i++)
        {
            Console.WriteLine(records[i]);
        }
    }
}