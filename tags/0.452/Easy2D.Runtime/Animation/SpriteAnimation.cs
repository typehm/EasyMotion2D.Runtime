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

        private Dictionary<string, SpriteAnimationState> animationStates = new Dictionary<string, SpriteAnimationState>();


        private List<SpriteAnimationState> allStates = new List<SpriteAnimationState>();



        private List<string> playQueue = new List<string>();


        private bool _isInit = false;



        void resetClip()
        {
            if (animations != null)
            {
                StopAll();

                removeList.Clear();
                evtList.Clear();

                foreach (SpriteAnimationState state in allStates)
                    removeList.Add(state);

                foreach (SpriteAnimationState state in removeList)
                    RemoveState(state);

                animationStates.Clear();
                removeList.Clear();
                stateRoot.Clear();
                spriteRenderer.Clear();

                foreach (SpriteAnimationClip clip in animations)
                    AddClip( clip );
            }
        }



        /// <summary>
        /// Init SpriteAnimation internal state.
        /// </summary>
        public void Init()
        {
            playingQueueChecker = new System.Predicate<SpriteAnimationState>(checkPlayingQueue);
            crossFadeQueueChecker = new System.Predicate<SpriteAnimationQueueItem>(checkCrossFadeQueue);
            removeAllClip = new System.Predicate<SpriteAnimationState>(removeClip);

            if ( Application.isPlaying && _isInit)
                return;


            spriteRenderer = GetComponent<SpriteRenderer>();
            //spriteRenderer.Clear();

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


            removeList.Clear();

            foreach (SpriteAnimationState state in allStates)
                removeList.Add(state);

            foreach (SpriteAnimationState state in removeList)
                RemoveState(state);
        }






        /// <summary>
        /// Returns the animation state named name.
        /// </summary>
        public SpriteAnimationState this[string name]
        {
            get
            {
                SpriteAnimationState ret = null;
                animationStates.TryGetValue(name, out ret);
                return ret;
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
            if (clip == null || (clip != null && !clip))
            {
                Debug.LogError("A null clip/missing clip add to animation!");
                return;
            }

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
            if (clip == null || (clip != null && !clip))
            {
                Debug.LogError("A null clip/missing clip add to animation!");
                return;
            }

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
            if (clip == null || (clip != null && !clip))
            {
                Debug.LogError("A null clip/missing clip add to animation!");
                return;
            }

            AddClip(clip, newName, firstFrame, lastFrame, false);
        }







        internal SpriteAnimationState AddClip(SpriteAnimationClip clip, string newName, int firstFrame, int lastFrame, bool addLoopFrame)
        {
            if (clip == null || ( clip != null && !clip ) )
            {
                Debug.LogError("A null clip/missing clip add to animation!");
                return null;
            }

            SpriteAnimationState state = this[newName];
            if ( state == null )
            {
                state = SpriteAnimationState.CreateState();
                state.Init(clip, this, newName);

                AddState(state);
            }
            else
            {
                state.Init(clip, this, newName);                
            }

            SetClipRange(newName, firstFrame * clip.tick, lastFrame * clip.tick);

            List<SpriteAnimationClip> tmpAnis = new List<SpriteAnimationClip>(animations);
            if (!tmpAnis.Contains(clip))
            {
                tmpAnis.Add(clip);
                animations = tmpAnis.ToArray();
            }


            return state;
        }




        System.Predicate<SpriteAnimationState> removeAllClip = new System.Predicate<SpriteAnimationState>(removeClip);

        static SpriteAnimationClip _removeClip = null;
        static bool removeClip(SpriteAnimationState state)
        {
            bool needRemove = state.clip == _removeClip;
            if (needRemove)
            {
                 state._animation.animationStates.Remove(state.name);
                SpriteAnimationState.ReleaseState(state);
            }
            return needRemove;
        }



        /// <summary>
        /// Remove clip from the animation list.
        /// This willl remove the clip and any animation states based on it.
        /// </summary>        
        public void RemoveClip(SpriteAnimationClip clip)
        {
            _removeClip = clip;
            allStates.RemoveAll(removeAllClip);
            _removeClip = null;
        }


        /// <summary>
        /// Remove clip from the animation list.
        /// This willl remove the animation state that match the name.
        /// </summary>
        /// <param name="clipName"></param>
        public void RemoveClip(string clipName)
        {
            SpriteAnimationState state = this[clipName];
            if ( state != null)
            {
                RemoveState(state);
            }
        }


        /// <summary>
        /// Get the number of clips currently assigned to this animation
        /// </summary>
        /// <returns>The number of clips currently assigned to this animation</returns>
        public int GetClipCount()
        {
            return allStates.Count;
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
                SpriteAnimationState tmpState = this[oldName];

                animationStates.Remove(oldName);
                animationStates.Add(state.name, tmpState);
            }
        }



        internal void AddState(SpriteAnimationState state)
        {
            animationStates.Add(state.name, state);  // here make unity3d hang 
            allStates.Add(state);
        }


        internal void RemoveState(SpriteAnimationState state)
        {
            if (state != null)
            {
                animationStates.Remove(state.name);
                allStates.Remove(state);

                SpriteAnimationState.ReleaseState(state);
            }
        }

    }














}