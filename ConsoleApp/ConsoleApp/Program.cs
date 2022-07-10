using System;

class Program
{
    private static void Main(string[] args)
    {
        PriorityQueue<int> priorityQueue = new PriorityQueue<int>();
        priorityQueue.Enqueue(9, 1);
        priorityQueue.Enqueue(8, 2);
        priorityQueue.Enqueue(7, 3);
        priorityQueue.Enqueue(6, 0);
        while (priorityQueue.Count > 0)
        {
            Console.WriteLine(priorityQueue.Dequeue());
        }
    }
}