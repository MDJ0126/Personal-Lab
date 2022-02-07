using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<Item> items = new List<Item>();

    public int Count => items.Count;

    public class Item
    {
        public T Data { get; private set; }
        public int Priority { get; set; } = 0;

        public Item(T data, int priority)
        {
            this.Data = data;
            this.Priority = priority;
        }
    }

    public void Enqueue(T data, int priority)
    {
        var item = new Item(data, priority);
        items.Add(item);
        MaxHeapInsertSort();
    }

    public void MaxHeapInsertSort()
    {
        int i = items.Count - 1;
        var item = items[i];
        while (i != 0 && item.Priority > items[(int)((i - 1) * 0.5f)].Priority)
        {
            items[i - 1] = items[(int)((i - 1) * 0.5f)];
            i = (int)((i - 1) * 0.5f);
        }
        items[i] = item;
    }

    public T Dequeue()
    {
        Item item = null;
        if (items.Count > 0)
            item = items[0];
        MaxHeapRemoveSort();
        return item.Data;
    }

    public void MaxHeapRemoveSort()
    {
        int parent = 0, child = 1;
        Item temp = items[items.Count - 1];
        items[0] = temp;
        items.RemoveAt(items.Count - 1);

        while (child < items.Count)
        {
            // 현재 노드의 자식 노드 중 더 큰 자식 노드를 찾는다. (루트 노드의 왼쪽 자식 노드(index: 2)부터 비교 시작)
            if (child + 1 < items.Count && (items[child].Priority) < items[child + 1].Priority)
            {
                child++;
            }

            // 자식 노드보다 마지막 노드가 크면, while문 중지
            if (temp.Priority >= items[child].Priority)
            {
                break;
            }

            // 더 큰 자식 노드보다 마지막 노드가 작으면, 부모 노드와 더 큰 자식 노드를 교환
            items[parent] = items[child];
            // 한 단계 아래로 이동
            parent = child;
            child *= 2;
        }

        if (items.Count > 0)
        {
            // 마지막 노드를 재구성한 위치에 삽입
            items[parent] = temp;
        }
    }
}