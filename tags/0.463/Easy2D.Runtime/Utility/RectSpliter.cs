using UnityEngine;
using System.Collections;
using System.Collections.Generic;





namespace EasyMotion2D
{




    struct RectBlock
    {
        public int w;
        public int used;
    }

    /// <summary>
    /// Internal class.
    /// </summary>
    public class RectSpliter
    {
        private Rect rect;
        private RectBlock[] blocks;
        private int blockSize;
        private int width;
        private int height;
        private int cellSizeShift;

        public RectSpliter(Rect rc, int cellSize)
        {
            Reset(rc, cellSize);
        }


        void Reset(Rect rc, int cellSize)
        {
            cellSizeShift = cellSize / 2;

            int w = (int)rc.width;
            int h = (int)rc.height;

            width = (w & 0x3) != 0 ? (w >> cellSizeShift) + 1 : (w >> cellSizeShift);
            height = (h & 0x3) != 0 ? (h >> cellSizeShift) + 1 : (h >> cellSizeShift);
            blockSize = width * height;

            blocks = new RectBlock[blockSize];
            for (int x = 0; x < blockSize; x++)
            {
                blocks[x].w = width - x % width;
                blocks[x].used = 0;
            }

            rect = new Rect(0, 0, w, h);
        }

        bool GetRect(int x, int w, int h)
        {
            if (blocks[x].used != 0) return false;

            if (x / width + h > height)
                return false;

            for (int iy = 0; iy < h; iy++)
            {
                int t = x + iy * width;
                if (t > blockSize || blocks[t].used != 0 || blocks[t].w < w)
                    return false;
            }

            for (int sy = 0; sy < h; sy++)
            {
                for (int sx = 0; sx < w; sx++)
                {
                    int i = x + (sy * width) + sx;
                    blocks[i].used = 1;
                    blocks[i].w = 0;
                }
                int t = 1, ex = x & (width - 1);
                for (int fx = 0; fx < ex; fx++)
                {
                    int i = x + (sy * width) - fx - 1;
                    if (blocks[i].used != 0) break;
                    blocks[i].w = t;
                    t++;
                }
            }

            return true;
        }

        bool AllocRect(ref Rect rc)
        {
            int rw = (int)rc.width;
            int rh = (int)rc.height;

            int sizeMask = (1 << cellSizeShift) - 1;

            rw = (rw & sizeMask) != 0 ? (rw >> cellSizeShift) + 1 : (rw >> cellSizeShift);
            rh = (rh & sizeMask) != 0 ? (rh >> cellSizeShift) + 1 : (rh >> cellSizeShift);

            int mask = width - 1;

            for (int x = 0; x < blockSize; x++)
            {
                if (GetRect(x, rw, rh))
                {
                    rc.xMin = (x & mask) << cellSizeShift;
                    rc.yMin = (x / width) << cellSizeShift;
                    rc.xMax = ((x & mask) + rw) << cellSizeShift;
                    rc.yMax = ((x / width) + rh) << cellSizeShift;
                    return true;
                }
            }
            return false;
        }



        public static Rect[] Spliter(Rect[] rects, int maxSize, int padding, ref Rect ret, int startWidth, int startHeight,  bool resizeRect, bool isWHSame, int cellSize )
        {
            int w = 1;
            int h = 1;

            if (resizeRect)
            {
                foreach (Rect rc in rects)
                {
                    if (w < rc.width)
                        w = (int)rc.width;

                    if (h < rc.height)
                        h = (int)rc.height;
                }

                w = Mathf.NextPowerOfTwo(w);
                h = Mathf.NextPowerOfTwo(h);
            }
            else
            {
                w = startWidth;
                h = startHeight;
            }


            while (true)
            {
                List<Rect> rcs = new List<Rect>(rects);

                RectSpliter spliter = new RectSpliter(new Rect(0, 0, w, h), cellSize);

                bool isAllocFail = false;
                for (int i = 0; i < rcs.Count; i++)
                {
                    Rect tmp = rcs[i];
                    tmp.width += padding;
                    tmp.height += padding;

                    bool r = spliter.AllocRect(ref tmp);

                    if (!r)
                    {
                        isAllocFail = true;
                        break;
                    }
                    rcs[i] = tmp;
                }

                if (!isAllocFail)
                {
                    ret = spliter.rect;
                    return rcs.ToArray();
                }
                else
                {
                    if (!resizeRect)
                        return null;

                    if (!isWHSame)
                    {
                        if (w > h)
                            h *= 2;
                        else
                            w *= 2;
                    }
                    else
                    {
                        w *= 2;
                        h = w;
                    }
                }

                if (w > maxSize || h > maxSize)
                    return null;
            }

            return null;
        }

    }



}