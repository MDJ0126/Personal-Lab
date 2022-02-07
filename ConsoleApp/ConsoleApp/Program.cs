using System;

class Program
{
    private static void Main(string[] args)
    {
        PriorityQueue<int> priorityQueue = new PriorityQueue<int>();
        priorityQueue.Enqueue(9, 9);
        priorityQueue.Enqueue(6, 6);
        priorityQueue.Enqueue(7, 7);
        priorityQueue.Enqueue(5, 5);
        //priorityQueue.Enqueue(3, 3);
        //priorityQueue.Enqueue(1, 1);
        priorityQueue.Enqueue(8, 8);
        //priorityQueue.Enqueue(2, 2);
        //priorityQueue.Enqueue(4, 4);

        while (priorityQueue.Count > 0)
        {
            Console.WriteLine(priorityQueue.Dequeue());
        }
    }
}