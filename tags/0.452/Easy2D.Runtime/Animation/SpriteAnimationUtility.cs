using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{


    /// <summary>
    /// Utility functions for modifying <see cref="SpriteAnimation">SpriteAnimation</see> clips.<br/>
    /// Internal class. You do not need to use this.
    /// </summary>
    public class SpriteAnimationUtility
    {
        private SpriteAnimationUtility()
        {
        }

        /// <summary>
        /// Returns the array of SpriteAnimationClips that are referenced in the Animation component
        /// </summary>
        public static SpriteAnimationClip[] GetAnimationClips(SpriteAnimation animation)
        {
            return animation.GetClips();
        }

        /// <summary>
        /// Sets the array of SpriteAnimationClips to be referenced in the Animation component
        /// </summary>
        public static void SetAnimationClips(SpriteAnimation animation, SpriteAnimationClip[] clips)
        {
            animation.SetClips(clips);
        }
    }

}