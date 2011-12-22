using System;


public class FastMutilSetItem
{
    internal int itemIndex;
}


internal class FastMultiSet<T> where T : FastMutilSetItem
{
    internal T[] datas = new T[0];

    public int Capacity
    {
        get
        {
            return _capacity;
        }

        set
        {
            if (value > _capacity)
                System.Array.Resize<T>(ref datas, value);
            _capacity = value;
        }
    }

    internal int _capacity = 0;


    public int Count
    {
        get
        {
            return _count;
        }
    }

    internal int _count = 0;

    public FastMultiSet()
    {
        Capacity = 8;
    }


    public int Add(T value)
    {
        if (_count == _capacity)
        {
            Capacity *= 2;
        }

        value.itemIndex = _count;
        datas[_count] = value;
        return _count++;
    }


    public void Remove(T value)
    {
        if (_count > 1)
        {
            RemoveAt(value.itemIndex);
        }
        else
            _count = 0;
    }

    

    public void RemoveAt(int index)
    {
        datas[index] = datas[_count - 1];
        _count--;
    }


    public void Clear()
    {
        _count = 0;
    }


    public bool Contains(T value)
    {
        foreach (T v in datas)
            if (v == value)
                return true;

        return false;
    }

    public T this[int index]
    {
        get
        {
            return datas[index];
        }
    }
}