using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{









    /// <summary>
    /// Event holder. Member store parameters defined in event dialog.
    /// </summary>
    [System.Serializable]
    public class SpriteAnimationEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public string stringParameter;


        /// <summary>
        /// 
        /// </summary>
        public int intParameter;



        /// <summary>
        /// 
        /// </summary>
        public float floatParameter;



        /// <summary>
        /// 
        /// </summary>
        public Object objectReferenceParameter;


        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public float time;



        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public int frameIndex;


        /// <summary>
        /// 
        /// </summary>
        public string functionName;


        /// <summary>
        /// 
        /// </summary>
        public SendMessageOptions messageOption = SendMessageOptions.DontRequireReceiver;



        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public SpriteAnimationState animationState;
    }






}