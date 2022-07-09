using System;

public struct ShiftComparer
{
    private static uint DIVIND = 64;
    private ulong[] array;
    private int length;

    public static ShiftComparer Init(uint length = 128)
    {
        ShiftComparer shiftComparer;
        shiftComparer.length = (int)Math.Ceiling((float)length / DIVIND);
        shiftComparer.array = new ulong[length];
        return shiftComparer;
    }

    public void Add(uint shift)
    {
        uint index = shift / DIVIND;
        if (index < array.Length)
        {
            shift -= index * DIVIND;
            array[index] |= shift;
        }
    }

    public void Remove(uint shift)
    {
        uint index = shift / DIVIND;
        if (index < array.Length)
        {
            shift -= index * DIVIND;
            array[index] &= ~(ulong)shift;
        }
    }

    public bool IsExist(uint shift)
    {
        uint index = shift / DIVIND;
        if (index < array.Length)
        {
            shift -= index * DIVIND;
            return (array[index] & (ulong)shift) != 0;
        }
        return false;
    }

    public void Clear() => array = new ulong[length];
}