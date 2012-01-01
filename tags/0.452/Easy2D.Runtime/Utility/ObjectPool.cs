using System;
using UnityEngine;


namespace EasyMotion2D
{

    //internal interface IPoolAllocator<T> where T : FastMutilSetItem
    //{
    //}

    //internal class ObjectPool<T> where T : FastMutilSetItem, new() //, IPoolAllocator<T>
    //{
    //    static private FastMultiSet<T> datas = new FastMultiSet<T>();

    //    static void FillPool(int count)
    //    {
    //        for (int i = 0; i < count; i++)
    //            datas.Add(new T());
    //    }

    //    static ObjectPool()
    //    {
    //    }

    //    static public T Alloc()
    //    {
    //        if (datas.Count == 0)
    //            FillPool(64);

    //        T ret = datas[datas.Count - 1];
    //        datas.RemoveAt(ret.itemIndex);

    //        return ret;
    //    }


    //    static public void Free(T value)
    //    {
    //        datas.Add(value);
    //    }
    //}




}