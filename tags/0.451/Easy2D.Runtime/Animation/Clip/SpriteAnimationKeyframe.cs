using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{







    /// <summary>
    /// A single keyframe that can be injected into an <see cref="SpriteAnimationComponent">animation component</see>.<br/>
    /// Internal class. You do not need to use this.
    /// </summary>
    [System.Serializable]
    public class SpriteAnimationKeyFrame
    {
        /// <summary>
        /// The frame index of keyframe.
        /// </summary>
        public int frameIndex = 0;



        /// <summary>
        /// The time of keyframe;
        /// </summary>
        public float time = 0f;



        /// <summary>
        /// The sprite of keyframe.
        /// </summary>
        public Sprite sprite = null;



        [SerializeField]
        internal bool isSpriteValid = false;



        /// <summary>
        /// The position of keyframe.
        /// </summary>
        public Vector2 position = Vector2.zero;



        /// <summary>
        /// The rotation of keyframe.
        /// </summary>
        public float rotation = 0f;



        /// <summary>
        /// The scale of keyframe.
        /// </summary>
        public Vector2 scale = Vector2.one;



        /// <summary>
        /// The shear of keyframe.
        /// </summary>
        public Vector2 shear = Vector2.zero;



        /// <summary>
        /// The color of keyframe.
        /// </summary>
        public Color color = Color.white;



        /// <summary>
        /// 
        /// </summary>
        public SpriteAnimationClip refClip = null;

        [SerializeField]
        internal bool isRefClip = false;



        public void SetClip(SpriteAnimationClip clip)
        {
            refClip = clip;
            isRefClip = refClip != null;
        }



        /// <summary>
        /// Clone all field from another keyframe.
        /// </summary>
        public void Clone(SpriteAnimationKeyFrame other)
        {
            if (other == null)
                return;

            //frameIndex = other.frameIndex;
            //time = other.time;
            sprite = other.sprite;
            position = other.position;
            rotation = other.rotation;
            scale = other.scale;
            shear = other.shear;
            color = other.color;
            refClip = other.refClip;
            isSpriteValid = other.isSpriteValid;
            isRefClip = other.isRefClip;
        }



        public SpriteAnimationKeyFrame()
        {
        }



        public SpriteAnimationKeyFrame(SpriteAnimationKeyFrame other)
        {
            Clone(other);
        }
    }




    /// <summary>
    /// Internal class. You do not need to use this.
    /// Sort keyframe by time
    /// </summary>
    internal class SpriteAnimationKeyFrameComparerByFrameIndex : IComparer<SpriteAnimationKeyFrame>
    {
        public int Compare(SpriteAnimationKeyFrame lhs, SpriteAnimationKeyFrame rhs)
        {
            return lhs.frameIndex - rhs.frameIndex;
        }

        public static SpriteAnimationKeyFrameComparerByFrameIndex comparer = new SpriteAnimationKeyFrameComparerByFrameIndex();
    }








}