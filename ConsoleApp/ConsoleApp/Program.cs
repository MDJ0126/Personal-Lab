using System.Threading;

class Program
{
    private static void Main(string[] args)
    {
        TestClass testClass = new TestClass();
        for (int i = 0; i < 20; i++)
        {
            Thread thread = new Thread(testClass.Calculte);
            thread.Name = $"Thread ({i})";
            thread.Start();
        }
    }

    private class TestClass
    {
        //초기 실행 가능한 쓰레드 2개
        //최대 실행 가능한 쓰레드 2개
        Semaphore semaphore = new Semaphore(2, 2);

        public void Calculte()
        {
            // 뮤텍스 취득할 때까지 기다린다.
            semaphore.WaitOne();

            // 1초간 대기 (연산이 처리되는 부분이라 가정)
            Thread.Sleep(5000);

            // 뮤텍스 해제
            semaphore.Release();
        }
    }
}