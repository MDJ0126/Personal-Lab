class Program
{
    private static void Main(string[] args)
    {
        PriorityQueue<int> priorityQueue = new PriorityQueue<int>();
        priorityQueue.Enqueue(1, 1);
        priorityQueue.Enqueue(2, 3);
    }
}