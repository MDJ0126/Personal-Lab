using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<Item> items = new List<Item>();

    public class Item
    {
        public T Data       { get; private set; }
        public int Priority { get; set; } = 0;
        public Item Parent  { get; set; } = null;
        public Item Left    { get; set; } = null;
        public Item Right   { get; set; } = null;

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
        int i = items.Count;
        var item = items[i - 1];
        while (i != 1 && item.Priority > items[(int)(i * 0.5)].Priority)
        {
            items[i] = items[(int)(i * 0.5)];
            i = (int)(i * 0.5);
        }
        items[i] = item;
    }

    public T Dequeue()
    {
        Item item = null;
        if (items.Count > 0)
        {
            int index = items.Count - 1;
            item = items[index];
            items.RemoveAt(index);
        }
        MaxHeapRemoveSort();
        return item.Data;
    }

    public void MaxHeapRemoveSort()
    {
        //// 최대 힙(max heap) 삭제 함수
        //element delete_max_heap(HeapType* h)
        //{
        //    int parent, child;
        //    element item, temp;

        //    item = h->heap[1]; // 루트 노드 값을 반환하기 위해 item에 할당
        //    temp = h->heap[(h->heap_size)--]; // 마지막 노드를 temp에 할당하고 힙 크기를 하나 감소
        //    parent = 1;
        //    child = 2;

        //    while (child <= h->heap_size)
        //    {
        //        // 현재 노드의 자식 노드 중 더 큰 자식 노드를 찾는다. (루트 노드의 왼쪽 자식 노드(index: 2)부터 비교 시작)
        //        if ((child < h->heap_size) && ((h->heap[child].key) < h->heap[child + 1].key))
        //        {
        //            child++;
        //        }
        //        // 더 큰 자식 노드보다 마지막 노드가 크면, while문 중지
        //        if (temp.key >= h->heap[child].key)
        //        {
        //            break;
        //        }

        //        // 더 큰 자식 노드보다 마지막 노드가 작으면, 부모 노드와 더 큰 자식 노드를 교환
        //        h->heap[parent] = h->heap[child];
        //        // 한 단계 아래로 이동
        //        parent = child;
        //        child *= 2;
        //    }

        //    // 마지막 노드를 재구성한 위치에 삽입
        //    h->heap[parent] = temp;
        //    // 최댓값(루트 노드 값)을 반환
        //    return item;
        //}
    }
}