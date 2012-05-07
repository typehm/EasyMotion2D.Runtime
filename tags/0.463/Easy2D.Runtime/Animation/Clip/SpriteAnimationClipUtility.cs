using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{











    /// <summary>
    /// Utility functions for modifying <see cref="SpriteAnimationClip">SpriteAnimationClip</see>.<br/>
    /// Internal class. You do not need to use this.
    /// </summary>
    public class SpriteAnimationClipUtility
    {
        private SpriteAnimationClipUtility()
        {
        }

        /// <summary>
        /// Recalculate all curves in SpriteAnimationCLip. Usually use in editor script when you modify keyframe data.
        /// </summary>
        public static void RecalcCurve(SpriteAnimationClip clip)
        {
            clip.Init( true );
        }

        public static void SetAnimationClipEvents(SpriteAnimationClip clip, SpriteAnimationEvent[] events)
        {
            clip.events = events;
            clip.CalcEvents();
        }

        public static SpriteAnimationEvent[] GetAnimationClipEvents(SpriteAnimationClip clip)
        {
            return clip.events;
        }


        public static SpriteAnimationEvent GetAnimationClipEventAtIndex(SpriteAnimationClip clip, int idx)
        {
            foreach (SpriteAnimationEvent evt in clip.events)
            {
                if (evt.frameIndex == idx)
                    return evt;
            }
            return null;
        }

        /// <summary>
        /// Internal delegate.
        /// </summary>
        public delegate bool ComponentDelegate(SpriteAnimationComponent comp);


        public static void EnumComponent(SpriteAnimationClip clip, ComponentDelegate callback, bool skipNotExpand)
        {
            EnumComponent(clip.root, callback, skipNotExpand);
        }


        internal static bool EnumComponent(SpriteAnimationComponent comp, ComponentDelegate callback, bool skipNotExpand)
        {
            if (!callback(comp))
                return false;

            if (!skipNotExpand || comp.expand)
                foreach (int idx in comp.children)
                    if (!EnumComponent(comp.clip.subComponents[idx], callback, skipNotExpand))
                        return false;

            return true;
        }



        public static SpriteAnimationComponent GetComponentByHashCode(SpriteAnimationClip clip, int hashcode)
        {
            return clip.GetComponentById(hashcode);
        }

        public static SpriteAnimationComponent GetComponentByPath(SpriteAnimationClip clip, string path)
        {
            return clip.GetComponentByPathHash( EasyMotion2DUtility.GetHashCode( path ));
        }

        public static bool Upgrade(SpriteAnimationClip clip)
        {
            return clip.Upgrade();
        }






        /// <summary>
        /// Clone a SpriteAnimationClip.
        /// </summary>
        /// <param name="clip">Source Clip.</param>
        /// <param name="init">Need initialize instance?</param>
        /// <returns>Instance of clip.</returns>
        public static SpriteAnimationClip Duplicate(SpriteAnimationClip clip, bool init)
        {
            return SpriteAnimationClip.Duplicate(clip, init);
        }

        /// <summary>
        /// Replace sprites in clip. Usually use to implement avatar charcter animation.
        /// </summary>
        /// <param name="clip">The clip need replace.</param>
        /// <param name="src">The source sprite array.</param>
        /// <param name="dest">The sprites want replace to.</param>
        public static void ReplaceSpriteInClipKeyframe(SpriteAnimationClip clip, Sprite[] src, Sprite[] dest)
        {
            ReplaceSpriteInComponent(clip.root, src, dest);
            foreach (SpriteAnimationComponent comp in clip.subComponents)
                ReplaceSpriteInComponent(comp, src, dest);
        }


        internal static void ReplaceSpriteInComponent(SpriteAnimationComponent component, Sprite[] src, Sprite[] dest)
        {
            foreach (SpriteAnimationKeyFrame kf in component.keyFrames)
            {
                int idx = -1;
                for (int i = 0; i < src.Length; i++)
                {
                    if (kf.sprite == src[i])
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0)
                    kf.sprite = dest[idx];
            }
        }





        /// <summary>
        /// Replace nested clip in clip. Usually use to implement avatar charcter animation.
        /// </summary>
        /// <param name="clip">The clip need replace.</param>
        /// <param name="src">The source clips array.</param>
        /// <param name="dest">The clips want replace to.</param>
        public static void ReplaceRefClipInClipKeyframe(SpriteAnimationClip clip, SpriteAnimationClip[] src, SpriteAnimationClip[] dest)
        {
            ReplaceRefClipInComponent(clip.root, src, dest);
            foreach (SpriteAnimationComponent comp in clip.subComponents)
                ReplaceRefClipInComponent(comp, src, dest);
        }


        internal static void ReplaceRefClipInComponent(SpriteAnimationComponent component, SpriteAnimationClip[] src, SpriteAnimationClip[] dest)
        {
            foreach (SpriteAnimationKeyFrame kf in component.keyFrames)
            {
                int idx = -1;
                for (int i = 0; i < src.Length; i++)
                {
                    if (kf.refClip == src[i])
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0)
                    kf.refClip = dest[idx];
            }
        }


    }
}