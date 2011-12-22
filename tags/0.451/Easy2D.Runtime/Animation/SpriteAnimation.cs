using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{


    /// <summary>
    /// Which moment you want handle on sprite animation component update.
    /// </summary>
    public enum SpriteAnimationComponentUpdateType
    {
        /// <summary>
        /// Animation Component evaluated in animatiom component local space.
        /// </summary>
        PreUpdateComponent,

        /// <summary>
        /// Animation Component evaluated and transformed into gameobject local space.
        /// </summary>
        PostUpdateComponent,
    }





    /// <summary>
    /// The animation component is used to play back <see cref="SpriteAnimationClip">animations</see>.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteRenderer))]
    public partial class SpriteAnimation : MonoBehaviour
    {
        /// <summary>
        /// The default animation.
        /// </summary>
        public SpriteAnimationClip clip = null;

        /// <summary>
        /// Should the default animation clip automatically start playing on startup.
        /// </summary>
        public bool playAutomatically = true;

        /// <summary>
        /// When turned on, Easy2D might stop animating if it thinks that the results of the animation won't be visible to the user.(Not implemented)
        /// </summary>
        public bool animateOnlyIfVisible = false;

        /// <summary>
        /// Update by time, not by frame in clip. It will take more time to update animations, but animation will be smooth.
        /// </summary>
        public bool smoothAnimation = true;

        /// <summary>
        /// 
        /// </summary>
        public delegate void SpriteAnimatonDelegate( SpriteAnimation animation );


        private SpriteRenderer spriteRenderer = null;

        [SerializeField]
        private SpriteAnimationClip[] animations = new SpriteAnimationClip[] { };

        private Hashtable animationStates = new Hashtable();


        private SpriteAnimationState[] allStates = new SpriteAnimationState[] { };



        private List<string> playQueue = new List<string>();


        private bool _isInit = false;



        void resetClip()
        {
            if (animations != null)
            {
                StopAll();
                animationStates.Clear();

                foreach (SpriteAnimationClip clip in animations)
                    AddClip( clip );
            }
        }



        /// <summary>
        /// Init SpriteAnimation internal state.
        /// </summary>
        public void Init()
        {
            if ( Application.isPlaying && _isInit)
                return;


            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.Clear();
            //states.Reset(8);

            resetClip();

            _isInit = true;
        }



        void Start()
        {
            if (playAutomatically && clip != null)
                Play(clip.name);
        }



        void OnEnable()
        {
            Init();
        }



        void OnDestroy()
        {
            StopAll();
        }





        /// <summary>
        /// Returns the animation state named name.
        /// </summary>
        public SpriteAnimationState this[string name]
        {
            get
            {
                return animationStates.ContainsKey( name ) ?
                    animationStates[name] as SpriteAnimationState :
                    null;
            }
        }



        /// <summary>
        /// Set the playing range in clip.
        /// </summary>
        /// <param name="name">The name of animation clip you want to set.</param>
        /// <param name="startTime">The start time of range in second.</param>
        /// <param name="endTime">The end time of range in second.</param>
        //[Obsolete("Use cullingType instead")]
        public void SetClipRange( string name, float startTime, float endTime)
        {
            SpriteAnimationState state = this[name];

            if (state != null)
            {
                state.curve.SetTime( startTime, state.clip.playingCurve.Evaluate(startTime), 
                    endTime, state.clip.playingCurve.Evaluate(endTime));
            }
            else
            {
                Debug.LogError("Can not find a clip named " + name);
            }            
        }




        /// <summary>
        /// Adds a clip to the animation with clip name.<br/>
        /// If a clip with that name already exists it will be replaced with the new clip. 
        /// </summary>
        /// <param name="clip">The clip you want add.</param>
        public void AddClip(SpriteAnimationClip clip)
        {
            AddClip(clip, clip.name, 0, clip.maxFrameIndex, false);
        }






        /// <summary>
        /// Adds a clip to the animation with name newName.<br/>
        /// If a clip with that name already exists it will be replaced with the new clip. 
        /// </summary>
        /// <param name="clip">The clip you want add.</param>
        /// <param name="newName">The new name of clip.</param>
        public void AddClip(SpriteAnimationClip clip, string newName)
        {
            AddClip(clip, newName, 0, clip.maxFrameIndex, false);
        }





        /// <summary>
        /// Adds clip to the only play between firstFrame and lastFrame. The new clip will also be added to the animation with name newName.<br/>
        /// If a clip with that name already exists it will be replaced with the new clip. 
        /// </summary>
        /// <param name="clip">The clip you want add.</param>
        /// <param name="newName">The new name of clip.</param>
        /// <param name="firstFrame">The first frame the in clip use as start.</param>
        /// <param name="lastFrame">The last frame the in clip use as end.</param>
        public void AddClip(SpriteAnimationClip clip, string newName, int firstFrame, int lastFrame)
        {
            AddClip(clip, newName, firstFrame, lastFrame, false);
        }







        internal SpriteAnimationState AddClip(SpriteAnimationClip clip, string newName, int firstFrame, int lastFrame, bool addLoopFrame)
        {
            if (clip == null)
            {
                Debug.LogError("A null clip add to animation!");
                return null;
            }

            SpriteAnimationState state = animationStates[newName] as SpriteAnimationState;
            if (state == null)
            {
                state = new SpriteAnimationState(clip, this, newName);
                AddState(state);
            }
            else
            {
                state.Init(clip, this, newName);                
            }

            List<SpriteAnimationClip> tmpAnis = new List<SpriteAnimationClip>(animations);
            if (!tmpAnis.Contains(clip))
            {
                tmpAnis.Add(clip);
                animations = tmpAnis.ToArray();
            }


            return state;
        }




        /// <summary>
        /// Remove clip from the animation list.
        /// This willl remove the clip and any animation states based on it.
        /// </summary>
        public void RemoveClip(SpriteAnimationClip clip)
        {
            List<SpriteAnimationState> tmp = new List<SpriteAnimationState>(allStates);        
            tmp.RemoveAll( delegate( SpriteAnimationState state)
            {
                bool needRemove = state.clip == clip;
                if ( needRemove )
                    animationStates.Remove( state.name );
                return needRemove;
            });

            allStates = tmp.ToArray();
        }


        /// <summary>
        /// Remove clip from the animation list.
        /// This willl remove the animation state that match the name.
        /// </summary>
        /// <param name="clipName"></param>
        public void RemoveClip(string clipName)
        {
            if (animationStates.ContainsKey(clip.name))
            {
                SpriteAnimationState state = animationStates[clip.name] as SpriteAnimationState;
                RemoveState(state);
            }
        }


        /// <summary>
        /// Get the number of clips currently assigned to this animation
        /// </summary>
        /// <returns>The number of clips currently assigned to this animation</returns>
        public int GetClipCount()
        {
            return allStates.Length;
        }


        internal void SetClips(SpriteAnimationClip[] clips)
        {
            animations = clips;
        }




        internal SpriteAnimationClip[] GetClips()
        {
            return animations;
        }



        internal void SetStateName( SpriteAnimationState state, string oldName )
        {
            if ( oldName != null && animationStates.ContainsKey(oldName))
            {
                SpriteAnimationState tmpState = animationStates[oldName] as SpriteAnimationState;

                animationStates.Remove(oldName);
                animationStates.Add(state.name, tmpState);
            }
        }



        internal void AddState(SpriteAnimationState state)
        {
            animationStates.Add(state.name, state);

            List<SpriteAnimationState> tmp = new List<SpriteAnimationState>(allStates);
            tmp.Add(state);
            allStates = tmp.ToArray();
        }


        internal void RemoveState(SpriteAnimationState state)
        {
            animationStates.Remove(state.name);

            List<SpriteAnimationState> tmp = new List<SpriteAnimationState>(allStates);
            tmp.Remove(state);
            allStates = tmp.ToArray();
        }

    }














}