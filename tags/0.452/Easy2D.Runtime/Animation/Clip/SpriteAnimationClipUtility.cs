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
            clip.Init();
            clip.CalcCurve();
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


        public static bool Upgrade(SpriteAnimationClip clip)
        {
            return clip.Upgrade();
        }
    }
}