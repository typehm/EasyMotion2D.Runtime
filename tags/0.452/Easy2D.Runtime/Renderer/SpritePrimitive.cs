using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace EasyMotion2D
{

    /// <summary>
    /// Internal class. You do not need to use this.
    /// </summary>
    [System.Serializable]
    internal class SpritePrimitive
    {
        public float z;

        public SpriteRenderMode renderMode;

        public Texture texture;
        public uint texId;

        public Vector3[] position = null;
        public Vector2[] uv = null;
        public Color[] color = null;

        public int size = 0;
        private int bufLength = 0;

        public bool visible = true;


        public int ownerID;
        public ulong compareKey = 0;



        private SpritePrimitive()
        {
            Resize(1, false);
        }

        public SpritePrimitive(int quadCnt)
        {
            Resize(quadCnt, false);
        }

        public void Resize(int quadSize, bool copyOldData)
        {
            if (quadSize > bufLength)
            {
                int oldCnt = size;
                Vector3[] _position = position;
                Vector2[] _uv = uv;
                Color[] _color = color;

                bufLength = Mathf.NextPowerOfTwo(quadSize);
                int cnt = 4 * bufLength;
                position = new Vector3[cnt];
                uv = new Vector2[cnt];
                color = new Color[cnt];

                if (copyOldData && _position != null)
                {
                    int oc = oldCnt << 2;
                    System.Array.Copy(_position, position, oc);
                    System.Array.Copy(_uv, uv, oc);
                    System.Array.Copy(_position, position, oc);
                }
            }

            size = quadSize;
        }

        public void SetColor(Color col)
        {
            for (int i = 0, e = size << 2; i < e; i++)
                color[i] = col;
        }

        override public string ToString()
        {
            return " compareKey:" + compareKey.ToString() + " z:" + z + " renderMode:" + renderMode + " tex:" + texId;
        }
    }






    /// <summary>
    /// Internal class. You do not need to use this.
    /// </summary>
    internal class SpritePrimitiveComparer : IComparer<SpritePrimitive>
    {
        public int Compare(SpritePrimitive lhs, SpritePrimitive rhs)
        {
            long r = (long)(lhs.compareKey - rhs.compareKey);
            return r > 0l ? 1 :
                (r < 0l ? -1 : 0);
        }

        public static SpritePrimitiveComparer comparer = new SpritePrimitiveComparer();
    }









        ///// <summary>
        ///// 非递归快速排序
        ///// 核心思想：将每次分治的两个序列的高位和低位入栈
        ///// 每次都从栈中获取一对高位和低位，分别处理。
        ///// 处理过程是：选取高位作为基准位置，从低位开始向
        ///// 高位遍历，如果比基准元素小，那么和第i个交换，如
        ///// 果有交换，那么i++，等一遍遍历完成后，如果i的位置
        ///// 不等于基准位置，那么所选的基准位置的值不是最大的
        ///// 而这时候i的位置之前的元素都比基准值小，那么i的位置
        ///// 应该是基准值，将i所在位置的值和基准位置进行交换。
        ///// 这时候，在i的左右就将序列分成两部分了，一部分比i所
        ///// 在位置值小，一部分比i所在位置值大的，然后再次将前
        ///// 面一部分和后面一部分的高位和低位分别入栈，再次选
        ///// 择基准位置，直到所选择的区间大小小于2，就可以不用
        ///// 入栈了。
        ///// </summary>
        ///// <param name="ary">要排序的数组</param>
        //public void NonrecursiveQuickSort(int[] ary)
        //{
        //    //如果数组中只有1一个元素或空数组，那就没必要排序了。
        //    if (ary.Length<2)
        //    {
        //        return;
        //    }
        //    //数组栈：记录着高位和低位的值
        //    int[,] stack = new int[2,ary.Length];
        //    //栈顶部位置
        //    int top = 0;
        //    //低位，高位，循环变量，基准点
        //    //将数组的高位和低位位置入栈
        //    stack[1, top] = ary.Length-1;
        //    stack[0, top] = 0;
        //    top++;
        //    //要是栈顶不空，那么继续
        //    while (top != 0)
        //    {
        //        //将高位和低位出栈
        //        //低位：排序开始的位置
        //        top--;
        //        int low = stack[0,top];
        //        //高位：排序结束的位置
        //        int high = stack[1,top];
        //        //将高位作为基准位置
        //        //基准位置
        //        int pivot = high;
        //        int i = low;
        //        for (int j = low; j < high; j++)
        //        {
        //            //如果某个元素小于基准位置上的值
        //            //那么将其和第i位交换，交换完成后
        //            //将低位也就是i前进一位，也就是一
        //            //轮循环下来以后，比基准位小的都
        //            //到前面去了，如果这次选的基准位
        //            //就是最大值，那么i最后应该和基准
        //            //位重合，如果不重合，那么基准位
        //            //应该就不是最大值，因为此时在i之
        //            //前的数据都是比基准位的值还小的
        //            //那么将基准位的值放到i所在的地方
        //            if (ary[j] <= ary[pivot])
        //            {
        //                int temp = ary[j];
        //                ary[j] = ary[i];
        //                ary[i] = temp;
        //                i++;
        //            }
        //        }
        //        //如果i不是基准位，那么基准位选的就不是最大值
        //        //而i的前面放的都是比基准位小的值，那么基准位
        //        //的值应该放到i所在的位置上
        //        if (i != pivot)
        //        {
        //            int temp = ary[i];
        //            ary[i] = ary[pivot];
        //            ary[pivot] = temp;
        //        }
        //        //下面这一段是保存现场的，一轮下来可能保存4个值，其实就是两个高位，两个低位
        //        //当i-low小于等于1的时候，就不往栈中放了，这就是外层while循环能结束的原因
        //        //如果从低位到i之间的元素个数多于一个，那么需要再次排序
        //        if (i - low > 1)
        //        {
        //            //此时不排i的原因是i位置上的元素已经确定了，i前面的都是比i小的，i后面的都是比i大的
        //            //所以此处i-1
        //            //存高位
        //            stack[1,top] = i - 1;
        //            //存低位
        //            stack[0,top] = low;
        //            top++;
        //        }
        //        //当high-i小于等于1的时候，就不往栈中放了，这就是外层while循环能结束的原因
        //        //如果从i到高位之间的元素个数多于一个，那么需要再次排序
        //        if (high - i > 1)
        //        {
        //            //此时不排i的原因是i位置上的元素已经确定了，i前面的都是比i小的，i后面的都是比i大的
        //            //存高位
        //            stack[1,top] = high;
        //            //所以此处i+1
        //            //存低位
        //            stack[0,top] = i + 1;
        //            top++;
        //        }
        //    }
        //}


}