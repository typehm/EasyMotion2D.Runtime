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
                foreach (SpriteTransform.StateComponentPair pair in referenceList)
                    pair.applyTo.Refresh();
            }
        }





        /// <summary>
        /// 
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

                foreach (SpriteTransform.StateComponentPair pair in referenceList)
                    pair.applyTo.CalcWeight();
            }
        }




        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public float startTime
        {
            get
            {
                return curve.startTime;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        public float endTime
        {
            get
            {
                return curve.endTime;
            }
        }




        public bool isPaused
        {
            get
            {
                return _isPaused;
            }
        }







        internal byte[] componentEnable = null; 

        internal SpriteAnimationState(SpriteAnimationClip clip, SpriteAnimation animation, string name)
        {
            Init(clip, animation, name);
        }


        internal SpriteAnimationState(SpriteAnimationState other, string name )
        {
            curve = new AnimationLinearCurve(
                other.curve.startTime,
                other.curve.startValue,
                other.curve.endTime,
                other.curve.endValue);

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
        }


        internal void Init(SpriteAnimationClip clip, SpriteAnimation animation, string name)
        {
            // remove from SpriteTransform
            foreach (SpriteTransform.StateComponentPair pair in referenceList)
                pair.applyTo.DetachState(pair.state, pair.component);

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


            wrapMode = clip.wrapMode;

            _clip = clip;
            _animation = animation;


            normalizedSpeed = 1f / clip.length;


            componentEnable = new byte[clip.subComponents.Length + 1];
            for (int i = 0, e = componentEnable.Length; i < e; i++)
                componentEnable[i] = 1;


            this.name = name;
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
                    enabled = false;
                    time = 0f;
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

        public void AddMixingComponent( string componentPath, bool recursive)
        {
            int hash = componentPath.GetHashCode();
            SpriteAnimationComponent component = clip.GetComponentByPathHash(hash);

            if (component != null )
            {
                AddMixingComponent(component, recursive);

                ApplyEnableList();

                if ( enabled )
                    foreach (SpriteTransform.StateComponentPair pair in referenceList)
                        pair.applyTo.CalcWeight();
            }
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
                ec.recursive = true;
                enableList.Add(ec);
            }
        }


        public void RemoveMixingComponent(string componentPath)
        {
            int hash = componentPath.GetHashCode();

            for (int i = 0, e = enableList.Count; i < e; i++)
            {
                if (enableList[i].component._fullPathHash == hash)
                {
                    enableList.RemoveAt(i);
                    break;
                }
            }

            ApplyEnableList();
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