using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace EasyMotion2D
{


    /// <summary>
    /// The SpriteAnimationState gives full control over animation playing.
    /// </summary>
    [System.Serializable]
    public class SpriteAnimationState
    {


        internal bool _enabled = false;



        internal SpriteAnimationClip _clip;



        internal string _name;



        internal SpriteAnimation _animation;



        internal float _speed = 1f;



        internal float _normalizedSpeed = 0f;



        internal float lastTime = 0f;



        internal int lastFrameIndex = 0;


        internal float lastEvaluateTime = 0f;



        internal float _startTime = 0;



        internal int _layer = 0;



        internal float _endTime = 0f;



        internal bool useCurveInState = false;



        internal AnimationLinearCurve curve = null;



        internal bool _isPaused = false;



        internal float _weight = 1f;



        internal float blendStep = 0f;



        internal bool removeAfterStop = false;



        internal int parentComponentLayer = 0;


        internal SpriteTransform parentTransform = null;

        /// <summary>
        /// Enables / disables the animation.
        /// </summary>
        public bool enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                if (value == _enabled)
                    return;

                if (value)
                {
                    _animation.AddSprite(this);

                }
                else
                {
                    _animation.RemoveSprite(this);
                }

                _enabled = value;
                _isPaused = false;                    
            }
        }


        /// <summary>
        /// Wrapping mode of the animation.    
        /// </summary>
        public WrapMode wrapMode = WrapMode.Once;




        /// <summary>
        /// The current time of the animation
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        ///
        /// public class example : MonoBehaviour {
        ///     void Awake() {
        ///        animation["Walk"].time = 0.0F;
        ///     }
        /// }
        /// </code>
        /// </example>
        public float time = 0f;



        /// <summary>
        /// The normalized time of the animation.
        /// </summary>
        public float normalizedTime
        {
            get
            {
                if (length == 0f)
                    return 0;

                return time / length;
            }

            set
            {
                time = value * length;
            }
        }




        /// <summary>
        /// The playback speed of the animation. 1 is normal playback speed.
        /// </summary>
        public float speed
        {
            get
            {
                return _speed;
            }

            set
            {
                _speed = value;

                foreach (SpriteAnimationState s in subClipState)
                    s.speed = _speed;
            }
        }




        /// <summary>
        /// The normalized playback speed.
        /// </summary>
        public float normalizedSpeed
        {
            get
            {
                return _normalizedSpeed;
            }

            set
            {
                _normalizedSpeed = value;

                if (_normalizedSpeed == 0f)
                    time = 0;
                else
                {
                    float t = 1f / _normalizedSpeed;
                    speed = length / t;
                }
            }
        }




        /// <summary>
        /// The length of the animation clip in seconds. Read only.
        /// </summary>
        public float length
        {
            get
            {
                return curve.endTime - curve.startTime;
            }
        }





        /// <summary>
        /// The layer of the animation. When calculating the final blend weights, animations in higher layers will get their weights
        /// </summary>
        public int layer
        {
            get
            {
                return _layer;
            }

            set
            {
                _layer = value;

                foreach (SpriteAnimationState s in subClipState)
                    s.layer = _layer;

                foreach (SpriteTransform.StateComponentPair pair in referenceList)
                    pair.applyTo.Refresh();
            }
        }





        /// <summary>
        /// This calculates the blend weights for curves.
        /// </summary>
        public float weight
        {
            get 
            {
                return _weight;
            }

            set
            {
                _weight = value;


                foreach (SpriteAnimationState s in subClipState)
                    s.weight = value;

                foreach (SpriteTransform.StateComponentPair pair in referenceList)
                    pair.applyTo.CalcWeight();
            }
        }




        /// <summary>
        /// Which blend mode should be used?
        /// </summary>
        public AnimationBlendMode blendMode = AnimationBlendMode.Blend;






        /// <summary>
        /// The clip that is being played by this animation state. Read only.
        /// </summary>
        public SpriteAnimationClip clip
        {
            get
            {
                return _clip;
            }
        }




        /// <summary>
        /// The name of the animation
        /// </summary>
        public string name
        {
            get
            {
                return _name;
            }

            set
            {
                string oldName = _name;
                _name = value;
                _animation.SetStateName(this, oldName);
            }
        }







        /// <summary>
        /// The start animation time of state.
        /// </summary>
        public float startTime
        {
            get
            {
                return curve.startTime;
            }
        }




        /// <summary>
        /// The end animation time of state.
        /// </summary>
        public float endTime
        {
            get
            {
                return curve.endTime;
            }
        }



        /// <summary>
        /// Is clip paused.
        /// </summary>
        public bool isPaused
        {
            get
            {
                return _isPaused;
            }
        }


        internal bool isRemoving = false;

        internal SpriteAnimationState parentState = null;
        internal List<SpriteAnimationState> subClipState = new List<SpriteAnimationState>();


        internal byte[] componentEnable = null; 

        private SpriteAnimationState(SpriteAnimationClip clip, SpriteAnimation animation, string name)
        {
            Init(clip, animation, name);
        }


        internal void Clone(SpriteAnimationState other, string name )
        {
            ClearState();

            if (curve == null)
            {
                curve = new AnimationLinearCurve(
                    other.curve.startTime,
                    other.curve.startValue,
                    other.curve.endTime,
                    other.curve.endValue);
            }
            else
            {
                curve.SetTime(other.curve.startTime,
                    other.curve.startValue,
                    other.curve.endTime,
                    other.curve.endValue);
            }

            wrapMode = other.wrapMode;

            _clip = other.clip;
            _animation = other._animation;

            normalizedSpeed = other.normalizedSpeed;

            componentEnable = new byte[clip.subComponents.Length + 1];
            for (int i = 0, e = componentEnable.Length; i < e; i++)
                componentEnable[i] = 1;

            _name = name;
            _layer = other._layer;
            _weight = other._weight;

            blendMode = AnimationBlendMode.Blend;

            isRemoving = false;
        }



        static List<SpriteTransform.StateComponentPair> tmpBuffer = new List<SpriteTransform.StateComponentPair>();

        internal void fini()
        {          
            for (int i = 0, e = componentEnable.Length; i < e; i++)
                componentEnable[i] = 0;
            enableList.Clear();


            // remove from SpriteTransform
            tmpBuffer.AddRange(referenceList);
            foreach (SpriteTransform.StateComponentPair pair in tmpBuffer)
                pair.applyTo.DetachState(pair.state, pair.component);
            tmpBuffer.Clear();


            referenceList.Clear();
            subClipState.Clear();

            if (subClipState.Count > 0)
            {
                Debug.LogError("sub state not empty " + _name);
                subClipState.Clear();
            }

            _name = string.Empty;
            parentTransform = null;
            _clip = null;
            _animation = null;
            parentState = null;

            isRemoving = false;
        }




        internal void Init(SpriteAnimationClip clip, SpriteAnimation animation, string name)
        {
            ClearState();


            //set playing curve
            if (curve == null)
            {
                curve = new AnimationLinearCurve(
                    clip.playingCurve.startTime,
                    clip.playingCurve.startValue,
                    clip.playingCurve.endTime,
                    clip.playingCurve.endValue);
            }
            else
            {
                curve.SetTime(clip.playingCurve.startTime,
                    clip.playingCurve.startValue,
                    clip.playingCurve.endTime,
                    clip.playingCurve.endValue);
            }

            blendMode = AnimationBlendMode.Blend;

            wrapMode = clip.wrapMode;

            _clip = clip;
            _animation = animation;


            if (clip.length <= 0f)
                normalizedSpeed = 1f;
            else
                normalizedSpeed = 1f / length;


            if (componentEnable == null)
                componentEnable = new byte[clip.subComponents.Length + 1];

            else if (componentEnable.Length < clip.subComponents.Length + 1)
                componentEnable = new byte[clip.subComponents.Length + 1];


            for (int i = 0, e = componentEnable.Length; i < e; i++)
                componentEnable[i] = 1;


            this.name = name;
        }

        private void ClearState()
        {
            //internal bool _enabled = false;
            _enabled = false;
            //internal SpriteAnimationClip _clip;
            _clip = null;
            //internal string _name;
            _name = string.Empty;
            //internal SpriteAnimation _animation;
            _animation = null;
            //internal float _speed = 1f;
            _speed = 1f;
            //internal float _normalizedSpeed = 0f;
            _normalizedSpeed = 0f;
            //internal float lastTime = 0f;
            lastTime = 0f;
            //internal int lastFrameIndex = 0;
            lastFrameIndex = 0;
            //internal float lastEvaluateTime = 0f;
            lastEvaluateTime = 0f;
            //internal float _startTime = 0;
            _startTime = 0;
            //internal int _layer = 0;
            _layer = 0;
            //internal float _endTime = 0f;
            _endTime = 0f;
            //internal bool useCurveInState = false;
            useCurveInState = false;
            //internal AnimationLinearCurve curve = null;
            //internal bool _isPaused = false;
            _isPaused = false;
            //internal float _weight = 1f;
            _weight = 1f;
            //internal float blendStep = 0f;
            blendStep = 0f;
            //internal bool removeAfterStop = false;
            removeAfterStop = false;
            //internal int parentComponentLayer = 0;
            parentComponentLayer = 0;
            //internal SpriteTransform parentTransform = null;
            parentTransform = null;
        }

        private SpriteAnimationState()
        {
        }





        internal List<SpriteTransform.StateComponentPair> referenceList = new List<SpriteTransform.StateComponentPair>();






        internal bool needFade = false;
        internal float fadeBase = 0f;
        internal float fadeTarget = 0f;
        internal float fadeLength = 0f;
        internal float currFadeTime = 0f;
        internal bool needStop = false;

        internal void Fade(float delta)
        {
            currFadeTime += delta;

            if ( currFadeTime >= fadeLength )
            {
                needFade = false;
                if ( needStop )
                {
                    _animation.Stop(this);
                }

                weight = fadeTarget;
                return;
            }

            weight = Mathf.Lerp(fadeBase, fadeTarget, currFadeTime / fadeLength);
        }


        struct EnableComponent
        {
            public SpriteAnimationComponent component;
            public bool recursive;
        }

        List<EnableComponent> enableList = new List<EnableComponent>();



        /// <summary>
        /// Adds a transform which should be animated. This allows you to reduce the number of animations you have to create. <br/>
        /// For example you might have a handwaving animation. You might want to play the hand waving animation on a idle character or on a walking character. Either you have to create 2 hand waving animations one for idle, one for walking. By using mixing the hand waving animation will have full control of the shoulder. But the lower body will not be affected by it, and continue playing the idle or walk animation. Thus you only need one hand waving animation.<br/>
        /// If recursive is true all children of the mix transform will also be animated. If you don't call AddMixingTransform, all animation curves will be used.
        /// </summary>
        public void AddMixingComponent( string componentPath, bool recursive)
        {
            //int hash = componentPath.GetHashCode();
            int hash = EasyMotion2DUtility.GetHashCode( componentPath );
            SpriteAnimationComponent component = clip.GetComponentByPathHash(hash);

            if (component != null )
            {
                AddMixingComponent(component, recursive);

                ApplyEnableList();

                if (enabled)
                    foreach (SpriteTransform.StateComponentPair pair in referenceList)
                        pair.applyTo.CalcWeight();
            }
        }

        string DumpEnablelist()
        {
            string ret = "";

            foreach (byte b in this.componentEnable)
                ret += b.ToString();

            return ret;
        }


        void AddMixingComponent(SpriteAnimationComponent component, bool recursive)
        {
            bool found = false;


            foreach (EnableComponent _ec in enableList)
            {
                if ( _ec.component == component && _ec.recursive == recursive)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                EnableComponent ec;
                ec.component = component;
                ec.recursive = recursive;
                enableList.Add(ec);
            }
        }


        /// <summary>
        /// Removes a transform which should be animated.<br/>
        /// You can only pass bone path that have been added through AddMixingTransform function. If transform has been added as recursive, then it will be removed as recursive. Once you remove all mixing transforms added to animation state all curves become animated again.
        /// </summary>
        public void RemoveMixingComponent(string componentPath)
        {
            //int hash = componentPath.GetHashCode();
            int hash = EasyMotion2DUtility.GetHashCode(componentPath);
            for (int i = 0, e = enableList.Count; i < e; i++)
            {
                if (enableList[i].component._fullPathHash == hash)
                {
                    enableList.RemoveAt(i);
                    break;
                }
            }

            ApplyEnableList();


            foreach (SpriteTransform.StateComponentPair pair in referenceList)
            {
                pair.applyTo.CalcWeight();
            }
        }



        void ApplyEnableList()
        {
            if (enableList.Count == 0)
            {
                for (int i = 0, e = componentEnable.Length; i < e; i++)
                    componentEnable[i] = 1;

                return;
            }
            else
            {
                for (int i = 0, e = componentEnable.Length; i < e; i++)
                    componentEnable[i] = 0;
            }


            foreach (EnableComponent ec in enableList)
                applyEnableList(ec.component, ec.recursive);
        }


        void applyEnableList(SpriteAnimationComponent component, bool recursive)
        {
            componentEnable[1 + component.index] = 1;

            if (recursive)
                foreach (int idx in component.children)
                    applyEnableList(clip.subComponents[idx], recursive);
        }





        internal static List<SpriteAnimationState> stateCache = new List<SpriteAnimationState>();

        internal static SpriteAnimationState CreateState()
        {
            if (stateCache.Count == 0)
            {
                for (int i = 0; i < 32; i++)
                {
                    stateCache.Add(new SpriteAnimationState());
                }
            }

            int idx = stateCache.Count - 1;

            SpriteAnimationState ret = stateCache[idx];
            
            stateCache.RemoveAt(idx);

            return ret;
        }


        internal static void ReleaseState( SpriteAnimationState state )
        {
            if (state != null)
            {
                state.fini();
                stateCache.Add(state);
            }
        }
    }








    /// <summary>
    /// Internal class. You do not need to use this.
    /// </summary>
    internal class SpriteAnimationStateComparerByLayer : IComparer<SpriteAnimationState>
    {
        public int Compare(SpriteAnimationState lhs, SpriteAnimationState rhs)
        {
            return lhs.layer - rhs.layer;
        }

        public static SpriteAnimationStateComparerByLayer comparer = new SpriteAnimationStateComparerByLayer();
    }


}