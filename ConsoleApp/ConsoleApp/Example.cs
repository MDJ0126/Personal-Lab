public class Example
{
    private class ClassA
    {
        public string name = string.Empty;
        public int count = 0;
    }

    private void AddCount(ref ClassA classA)
    {
        classA.count++;
    }

    private void Start()
    {
        ClassA classA = new ClassA();
        AddCount(ref classA); // != AddCount(classA);
    }
}