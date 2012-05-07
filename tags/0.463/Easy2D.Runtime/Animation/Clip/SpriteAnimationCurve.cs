using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{



    [System.Serializable]
    internal class AnimationLinearCurve
    {
        public float startTime;
        public float endTime;
        public float startValue;
        public float endValue;

        public WrapMode wrapMode = WrapMode.Once;

        private float timeRange = 0f;
        private float invTimeRange = 0f;
        private bool isValid = true;

        public float Evaluate(float time)
        {
            if (!isValid)
                return Mathf.Lerp(startValue, endValue, 0);

            float ret = 0f;

            if (wrapMode == WrapMode.Loop)
            {
                ret = (timeRange + (time % timeRange)) * invTimeRange;
                ret %= 1f;
            }

            else if (wrapMode == WrapMode.PingPong)
            {
                float t = (Mathf.Abs(time) % (timeRange * 2f)) * invTimeRange;
                ret = t >= 1f ? (2f - t) : t;
            }

            else if (wrapMode == WrapMode.ClampForever)
            {
                ret = Mathf.Clamp01(time * invTimeRange);
            }

            else if (wrapMode == WrapMode.Default || wrapMode == WrapMode.Clamp || wrapMode == WrapMode.Once)
            {
                ret = time * invTimeRange;
            }


            ret = Mathf.Lerp(startValue, endValue, ret);

            return ret;
        }




        public AnimationLinearCurve(float startTime, float startValue, float endTime, float endValue)
        {
            SetTime(startTime, startValue, endTime, endValue);
        }




        public void SetTime(float startTime, float startValue, float endTime, float endValue)
        {
            this.startTime = startTime;
            this.startValue = startValue;
            this.endTime = endTime;
            this.endValue = endValue;

            timeRange = endTime - startTime;

            isValid = timeRange > 0f;

            if (isValid)
                invTimeRange = 1f / timeRange;
        }
    }









    /// <summary>
    /// Internal Enum. You do not need to use this.
    /// </summary>
    public enum SpriteAnimationCurveType
    {
        KeyframeIndex = 0,
        PositionX,
        PositionY,
        Rotate,
        ScaleX,
        ScaleY,
        ColorR,
        ColorG,
        ColorB,
        ColorA,
        ShearX,
        ShearY,
    }








    /// <summary>
    /// A wrapper of AnimationCurve 
    /// Internal class. You do not need to use this.
    /// </summary>
    [System.Serializable]
    public class SpriteAnimationCurve
    {
        public string name;


        public AnimationCurve curve;


        public SpriteAnimationCurveType type;


        public bool interpolation = true;


        [SerializeField]
        internal int length;


        [SerializeField]
        internal float defaultValue;



        public SpriteAnimationCurve()
        {
        }



        public SpriteAnimationCurve(SpriteAnimationCurve other)
        {
            this.name = other.name;
            this.type = other.type;
            this.curve = new AnimationCurve(other.curve.keys);

            this.interpolation = other.interpolation;
            this.length = other.length;
            this.defaultValue = other.defaultValue;
        }
    }




}