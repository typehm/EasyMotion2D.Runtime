using UnityEngine;
using System.Collections;

namespace EasyMotion2D
{


    internal struct SetIndexer<T>
    {
        public int nodeIndex;
        public int parentHashID;

        public T value;
        public bool isInit;

        public bool isValid
        {
            get
            {
                return isInit && nodeIndex != -1;
            }
        }

        public SetIndexer(T value)
        {
            nodeIndex = -1;
            this.value = value;
            isInit = true;
            parentHashID = 0;
        }

        public SetIndexer(int idx, T value, int hashid)
        {
            nodeIndex = idx;
            this.value = value;
            isInit = true;
            parentHashID = hashid;
        }

        public void Init()
        {
            nodeIndex = -1;
            isInit = true;
            parentHashID = 0;
        }

        public void Reset()
        {
            Init();
            value = default(T);
        }

        public string ToString()
        {
            return nodeIndex + "," + parentHashID + "," + isInit + "," + value.ToString(); 
        }

        public static SetIndexer<T> Null = new SetIndexer<T>();
    }






    internal class Set<T>
    {
        public int size;
        public int maxSize;

        public T[] datas;

        private int hashID = 0;

        public Set()
        {
            Reset(16);
            hashID = this.GetHashCode();
        }

        public void Reset(int size)
        {
            this.size = 0;
            datas = new T[size];
            maxSize = size;
        }

        public SetIndexer<T> Add(SetIndexer<T> data)
        {
            if (data.nodeIndex != -1)
                return SetIndexer<T>.Null;

            if (size == maxSize)
            {
                T[] tmp = new T[maxSize * 2];

                System.Array.Copy(datas, tmp, maxSize);
                datas = tmp;
                maxSize *= 2;
            }

            datas[size] = data.value;            
            return new SetIndexer<T>(size++, data.value, hashID);
        }

        public void Remove(SetIndexer<T> data)
        {
            int i = data.nodeIndex;
            if (i == -1 || data.parentHashID != this.hashID)
                return;

            int t = size - 1;
            if (i < maxSize && t < maxSize)
            {
                datas[i] = datas[t];
                datas[t] = default(T);
                size--;
            }

            return;
        }


        public void Clear(bool setNull)
        {
            if (setNull)
                System.Array.Clear(datas, 0, maxSize);

            size = 0;
        }

        public T this[int index]
        {
            get
            {
                return datas[index];
            }
        }
    }

}
