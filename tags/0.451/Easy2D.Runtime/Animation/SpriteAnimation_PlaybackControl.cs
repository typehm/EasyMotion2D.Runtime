using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{

    public partial class SpriteAnimation : MonoBehaviour
    {

        /// <summary>
        /// Will start the default animation. The animation will be played abruptly without any blending.<br/>
        /// </summary>
        /// <returns>Will return false if animation can't be played (no default animation). </returns>
        /// <remarks>
        /// If the animation is already playing, other animations will be stopped but the animation will not rewind to the beginning.<br/>
        /// If the animation is not set to be looping it will be stopped and rewinded after playing.<br/>
        /// </remarks>
        public bool Play()
        {
            return Play(PlayMode.StopSameLayer);
        }


        /// <summary>
        /// Will start the default animation. The animation will be played abruptly without any blending.<br/>
        /// </summary>
        /// <param name="mode">How the other animations will stopped?</param>
        /// <returns>Will return false if animation can't be played (no default animation).</returns>
        /// <remarks>
        /// If mode is PlayMode.StopSameLayer then all animations in the same layer will be stopped. If mode is PlayMode.StopAll then all animations currently playing will be stopped.<br/>
        /// If the animation is already playing, other animations will be stopped but the animation will not rewind to the beginning.<br/>
        /// If the animation is not set to be looping it will be stopped and rewinded after playing.<br/>
        /// </remarks>
        public bool Play(PlayMode mode)
        {
            if ( clip == null )
                return false;

            return Play(clip.name, mode);
        }




        /// <summary>
        /// Will start animation with name animation. The animation will be played abruptly without any blending.<br/>
        /// </summary>
        /// <param name="name">The name of animation clip you want to play.</param>
        /// <returns>Will return false if animation can't be played (no animation clip).</returns>
        /// <remarks>
        /// If the animation is already playing, other animations will be stopped but the animation will not rewind to the beginning.<br/>
        /// If the animation is not set to be looping it will be stopped and rewinded after playing.<br/>
        /// </remarks>
        public bool Play(string name)
        {
            return Play(name, PlayMode.StopSameLayer);
        }



        /// <summary>
        /// Will start animation with name animation. The animation will be played abruptly without any blending.<br/>
        /// </summary>
        /// <param name="name">The name of animation clip you want to play.</param>
        /// <param name="mode">How the other animations will stopped?</param>
        /// <returns>Will return false if animation can't be played (no animation clip).</returns>
        /// <remarks>
        /// If mode is PlayMode.StopSameLayer then all animations in the same layer will be stopped. If mode is PlayMode.StopAll then all animations currently playing will be stopped.<br/>
        /// If the animation is already playing, other animations will be stopped but the animation will not rewind to the beginning.<br/>
        /// If the animation is not set to be looping it will be stopped and rewinded after playing.<br/>
        /// </remarks>
        public bool Play(string name, PlayMode mode)
        {
            SpriteAnimationState state = this[name];
            if (state != null)
            {
                if (mode == PlayMode.StopAll)
                    StopAll();
                else
                    StopLayer(state.layer);

                if (state.enabled)
                    return true;

                state.time = 0f;
                state.weight = 1f;
                state.enabled = true;

                state.lastTime = 0f;
                state.lastFrameIndex = -1;
                state.lastEvaluateTime = 0;

                return true;
            }

            return false;
        }








        List<SpriteTransform> stateRoot = new List<SpriteTransform>();


        internal void AddSprite(SpriteAnimationState state)
        {
            AddSprite(state, state.clip.root, state.parentTransform, 
                state.parentTransform == null ? 0 : state.parentTransform.component._fullPathHash );


            SpriteTransform sprTransfrom = spriteRenderer.GetSpriteTransformByFullPathHash( state.clip.root._fullPathHash);
            if ( state.parentTransform == null && !stateRoot.Contains(sprTransfrom))
                stateRoot.Add(sprTransfrom);
        }



        void AddSprite(SpriteAnimationState state, SpriteAnimationComponent component, SpriteTransform parentTransform, int parentHash )
        {
  
            SpriteTransform sprTransfrom = spriteRenderer.GetSpriteTransformByFullPathHash(component._fullPathHash + parentHash);

            if (sprTransfrom == null)
            {
                {
                    sprTransfrom = spriteRenderer.AttachSpriteComponent(component);

                    sprTransfrom.parent = parentTransform;

                    sprTransfrom.layer = (int)component.layer + state.parentComponentLayer;
                    sprTransfrom.visible = component.visible;
                }


                sprTransfrom.overrideHash = parentHash != 0 ? component._fullPathHash + parentHash : 0;


                if (parentTransform != null)
                {
                    if (parentTransform.firstChild == null)
                    {
                        parentTransform.firstChild = sprTransfrom;
                        parentTransform.lastChild = sprTransfrom;

                        sprTransfrom.prve = null;
                        sprTransfrom.next = null;
                    }
                    else
                    {
                        sprTransfrom.next = parentTransform.lastChild.next;
                        sprTransfrom.prve = parentTransform.lastChild;

                        parentTransform.lastChild.next = sprTransfrom;
                    }
                }
            }

            sprTransfrom.AttachState(state, component);

            for (int i = 0, e = component.children.Length; i < e; i++)
                AddSprite( state, state.clip.subComponents[ component.children[i] ], sprTransfrom, 0);
        }



        internal void RemoveSprite(SpriteAnimationState state)
        {
            RemoveSprite(state, state.clip.root, state.parentTransform,
                state.parentTransform == null ? 0 : state.parentTransform.component._fullPathHash );
        }


        void RemoveSprite(SpriteAnimationState state, SpriteAnimationComponent component, SpriteTransform parentTransform, int parentHash)
        {
            SpriteTransform sprTransfrom = spriteRenderer.GetSpriteTransformByFullPathHash(component._fullPathHash + parentHash );


            if (sprTransfrom != null)
            {
                sprTransfrom.DetachState(state, component);

                if (sprTransfrom.attachStateComponent.Count <= 0)
                {
                    if (sprTransfrom.prve != null)
                        sprTransfrom.prve.next = sprTransfrom.next;

                    if (sprTransfrom.next != null)
                        sprTransfrom.next.prve = sprTransfrom.prve;

                    if (sprTransfrom.parent != null && sprTransfrom.parent.firstChild == sprTransfrom)
                        sprTransfrom.parent.firstChild = sprTransfrom.next;

                    for (SpriteTransform child = sprTransfrom.firstChild; child != null; child = child.next)
                        child.parent = null;
                    {
                        sprTransfrom.parent = null;
                        sprTransfrom.next = null;
                        sprTransfrom.prve = null;
                        sprTransfrom.firstChild = null;
                        sprTransfrom.lastChild = null;
                    }

                    if (stateRoot.Contains(sprTransfrom))
                        stateRoot.Remove(sprTransfrom);

                    spriteRenderer.DetachSprite(sprTransfrom.id);
                }
            }

            for (int i = 0, e = component.children.Length; i < e; i++)
                RemoveSprite(state, state.clip.subComponents[component.children[i]], sprTransfrom, 0);
        }







        class SpriteAnimationQueueItem
        {
            public SpriteAnimationState state;
            public SpriteAnimationState[] waitingList = new SpriteAnimationState[0];
            public PlayMode playMode;
            public float fadeLength;
        }

        List<SpriteAnimationState> playingQueue = new List<SpriteAnimationState>();

        //Plays an animation after previous animations has finished playing.
        //The following queue modes are available:
        //If queue is QueueMode.CompleteOthers this animation will only start once all other animations have stopped playing.
        //If queue is QueueMode.PlayNow this animation will start playing immediately on a duplicated animation state.
        //After the animation has finished playing it will automatically clean itself up. Using the duplicated animation state after it has finished will result in an exception. 

        /// <summary>
        /// Plays an animation after previous animations has finished playing.<br/>
        /// For example you might play a specific sequeue of animations after each other.<br/>
        /// The animation state duplicates itself before playing thus you can fade between the same animation. This can be used to overlay two same animations. For example you might have a sword swing animation. The player slashes two times quickly after each other. You could rewind the animation and play from the beginning but then you will get a jump in the animation.<br/>
        /// </summary>
        /// <param name="name">The name of animation clip you want to play.</param>
        /// <returns>The duplicated animation state of the clip.</returns>
        public SpriteAnimationState PlayQueued(string name)
        {
            return PlayQueued( name, QueueMode.CompleteOthers, PlayMode.StopSameLayer);
        }


        /// <summary>
        /// Plays an animation after previous animations has finished playing. <br/>
        /// If queue is QueueMode.CompleteOthers this animation will only start once all other animations have stopped playing.<br/>
        /// If queue is QueueMode.PlayNow this animation will start playing immediately on a duplicated animation state.<br/>
        /// </summary>
        /// <param name="name">The name of animation clip you want to play.</param>
        /// <param name="queue">QueueMode control how to play the clip in queue.</param>
        /// <returns>The duplicated animation state of the clip.</returns>
        public SpriteAnimationState PlayQueued(string name, QueueMode queue)
        {
            return PlayQueued(name, queue, PlayMode.StopSameLayer);
        }


        int cloneID = 0;

        /// <summary>
        /// Plays an animation after previous animations has finished playing.<br/>
        /// If queue is QueueMode.CompleteOthers this animation will only start once all other animations have stopped playing.<br/>
        /// If queue is QueueMode.PlayNow this animation will start playing immediately on a duplicated animation state.<br/>
        /// if mode is PlayMode.StopSameLayer, animations in the same layer as animation will be faded out while animation is faded in. if mode is PlayMode.StopAll, all animations will be faded out while animation is faded in.<br/>
        /// After the animation has finished playing it will automatically clean itself up. Using the duplicated animation state after it has finished will result in an exception. 
        /// </summary>
        /// <param name="name">The name of animation clip you want to play.</param>
        /// <param name="queue">QueueMode control how to play the clip in queue.</param>
        /// <param name="mode">QueueMode control how to play the clip in queue.</param>
        /// <returns>The duplicated animation state of the clip.</returns>
        public SpriteAnimationState PlayQueued(string name, QueueMode queue, PlayMode mode)
        {
            SpriteAnimationState state = this[name];
            if (state == null)
                return null;

            bool isPlaying = IsLayerPlaying(state.layer);

            if (queue == QueueMode.PlayNow || !isPlaying )
            {
                string newName = state.name + " - Queued Clone " + cloneID++;

                SpriteAnimationState tmpState = new SpriteAnimationState(state, newName);
                tmpState.removeAfterStop = true;
                AddState(tmpState);

                Play(newName);
                return tmpState;
            }
            else
            {
                string newName = state.name + " - Queued Clone " + cloneID++;

                SpriteAnimationState tmpState = new SpriteAnimationState(state, newName);
                tmpState.removeAfterStop = true;
                AddState(tmpState);


                playingQueue.Add( tmpState );

                return tmpState;
            }

            return null;
        }










        //Blends the animation named animation towards targetWeight over the next time seconds.
        //Playback of other animations will not be affected.

        /// <summary>
        /// Blends the animation named animation weight towards 1 over the 0.3f seconds.<br/>
        /// Playback of other animations will not be affected.
        /// </summary>
        /// <param name="name">Blending clip name.</param>
        public void Blend(string name)
        {
            Blend(name, 1.0f, 0.3f);
        }


        /// <summary>
        /// Blends the animation named animation towards targetWeight over the 0.3f seconds.<br/>
        /// Playback of other animations will not be affected.
        /// </summary>
        /// <param name="name">Blending clip name.</param>
        /// <param name="targetWeight">Blending target weight.</param>
        public void Blend(string name, float targetWeight)
        {
            Blend(name, targetWeight, 0.3f);
        }


        /// <summary>
        /// Blends the animation named animation towards targetWeight over the fadeLength seconds.<br/>
        /// Playback of other animations will not be affected.
        /// </summary>
        /// <param name="name">Blending clip name.</param>
        /// <param name="targetWeight">Blending target weight.</param>
        /// <param name="fadeLength">Blending length in second.</param>
        public void Blend(string name, float targetWeight, float fadeLength)
        {
            Blend(name, targetWeight, fadeLength, false);
        }


        /// <summary>
        /// Blends the animation named animation towards targetWeight over the 0.3f seconds.<br/>
        /// Playback of other animations will not be affected.
        /// </summary>
        /// <param name="name">Blending clip name.</param>
        /// <param name="targetWeight">Blending target weight.</param>
        /// <param name="fadeLength">Blending length in second.</param>
        /// <param name="needStop">After fade, did need stop the clip playing?</param>
        public void Blend(string name, float targetWeight, float fadeLength, bool needStop)
        {
            SpriteAnimationState state = this[name];
            if (state != null)
                Blend(state, targetWeight, fadeLength, needStop);
        }

        void Blend(SpriteAnimationState state, float targetWeight, float fadeLength, bool needStop)
        {
            if (state.parentTransform == null)
            {
                state.needFade = true;
                state.fadeBase = state.weight;
                state.fadeTarget = targetWeight;
                state.fadeLength = fadeLength;
                state.currFadeTime = 0f;
                state.needStop = needStop;
            }
        }










        //Fades the animation with name animation in over a period of time seconds and fades other animations out.
        //if mode is PlayMode.StopSameLayer, animations in the same layer as animation will be faded out while animation is faded in. if mode is PlayMode.StopAll, all animations will be faded out while animation is faded in.
        //If the animation is not set to be looping it will be stopped and rewinded after playing. 

        /// <summary>
        /// Fades the animation with name animation in over 0.3 seconds and fades other animations out After fade out will stop all clips in same layer.<br/>
        /// If the animation is not set to be looping it will be stopped and rewinded after playing. <br/>
        /// </summary>
        /// <param name="name">The clip name that you want to fade in.</param>
        public void CrossFade(string name)
        {
            CrossFade(name, 0.3f, PlayMode.StopSameLayer);
        }



        /// <summary>
        /// Fades the animation with name animation in over fadeLength seconds and fades other animations out After fade out will stop all clips in same layer.<br/>
        /// If the animation is not set to be looping it will be stopped and rewinded after playing. <br/>
        /// </summary>
        /// <param name="name">The clip name that you want to fade in.</param>
        /// <param name="fadeLength">The fade in/out length in second.</param>
        public void CrossFade(string name, float fadeLength)
        {
            CrossFade(name, fadeLength, PlayMode.StopSameLayer);
        }


        /// <summary>
        /// Fades the animation with name animation in over a period of time seconds and fades other animations out.<br/>
        /// if mode is PlayMode.StopSameLayer, animations in the same layer as animation will be faded out while animation is faded in. if mode is PlayMode.StopAll, all animations will be faded out while animation is faded in.<br/>
        /// If the animation is not set to be looping it will be stopped and rewinded after playing. 
        /// </summary>
        /// <param name="name">The clip name that you want to fade in.</param>
        /// <param name="fadeLength">The fade in/out length in second.</param>
        /// <param name="mode">How to stop the other clips.</param>
        public void CrossFade(string name, float fadeLength, PlayMode mode)
        {
            SpriteAnimationState state = this[name];
            if (state != null)
                CrossFade(state, fadeLength, mode);
        }



        void CrossFade(SpriteAnimationState state, float fadeLength, PlayMode mode)
        {
            if (mode == PlayMode.StopAll)
            {
                foreach (SpriteAnimationState iter in allStates)
                {
                    if (iter.enabled)
                        Blend(iter, 0f, fadeLength, true);
                }
            }
            else
            {
                foreach (SpriteAnimationState iter in allStates)
                {
                    if (iter.enabled && iter.layer == state.layer)
                        Blend(iter, 0f, fadeLength, true);
                }
            }

            if (!state.enabled)
            {
                state.time = 0;
                state.weight = 0f;
                state.enabled = true;

                state.lastTime = 0f;
                state.lastFrameIndex = -1;
                state.lastEvaluateTime = 0;
            }

            Blend(state, 1f, fadeLength, false);
        }



        List<SpriteAnimationQueueItem> crossFadeQueue = new List<SpriteAnimationQueueItem>();

        /// <summary>
        /// Cross fades an animation after previous animations has finished playing.<br/>
        ///For example you might play a specific sequence of animations after each other.<br/>
        ///The animation duplicates itself before playing thus you can fade between the same animation. This can be used to overlay two same animations. For example you might have a sword swing animation. The player slashes two times quickly after each other. /You could rewind the animation and play from the beginning but then you will get a jump in the animation.<br/>
        ///The following queue modes are available:<br/>
        ///This animation will only start once all other animations have stopped playing.<br/>
        ///After the animation has finished playing it will automatically clean itself up. Using the duplicated animation state after it has finished will result in an exception. <br/>
        /// </summary>
        /// <param name="name">The clip name that you want to fade in.</param>
        /// <returns>The duplicated animation state of the clip.</returns>
        public SpriteAnimationState CrossFadeQueued(string name)
        {
            return CrossFadeQueued(name, 0.3f, QueueMode.CompleteOthers, PlayMode.StopSameLayer);
        }


        /// <summary>
        /// Cross fades an animation after previous animations has finished playing.<br/>
        ///This animation will only start once all other animations have stopped playing.<br/>
        ///After the animation has finished playing it will automatically clean itself up. Using the duplicated animation state after it has finished will result in an exception. <br/>
        /// </summary>
        /// <param name="name">The clip name that you want to fade in.</param>
        /// <param name="fadeLength">The fade in/out length in second.</param>
        /// <returns>The duplicated animation state of the clip.</returns>
        public SpriteAnimationState CrossFadeQueued(string name, float fadeLength)
        {
            return CrossFadeQueued(name, fadeLength, QueueMode.CompleteOthers, PlayMode.StopSameLayer);
        }


        /// <summary>
        /// Cross fades an animation after previous animations has finished playing.<br/>
        ///If queue is QueueMode.CompleteOthers this animation will only start once all other animations have stopped playing.<br/>
        ///If queue is QueueMode.PlayNow this animation will start playing immediately on a duplicated animation state.<br/>
        ///After the animation has finished playing it will automatically clean itself up. Using the duplicated animation state after it has finished will result in an exception. <br/>
        /// </summary>
        /// <param name="name">The clip name that you want to fade in.</param>
        /// <param name="fadeLength">The fade in/out length in second.</param>
        /// <param name="queue">How the clip start fade.</param>
        /// <returns>The duplicated animation state of the clip.</returns>
        public SpriteAnimationState CrossFadeQueued(string name, float fadeLength, QueueMode queue)
        {
            return CrossFadeQueued(name, fadeLength, queue, PlayMode.StopSameLayer);
        }



        /// <summary>
        /// Cross fades an animation after previous animations has finished playing.<br/>
        ///If queue is QueueMode.CompleteOthers this animation will only start once all other animations have stopped playing.<br/>
        ///If queue is QueueMode.PlayNow this animation will start playing immediately on a duplicated animation state.<br/>
        /// if mode is PlayMode.StopSameLayer, animations in the same layer as animation will be faded out while animation is faded in. if mode is PlayMode.StopAll, all animations will be faded out while animation is faded in.<br/>
        ///After the animation has finished playing it will automatically clean itself up. Using the duplicated animation state after it has finished will result in an exception. <br/>
        /// </summary>
        /// <param name="name">The clip name that you want to fade in.</param>
        /// <param name="fadeLength">The fade in/out length in second.</param>
        /// <param name="queue">How the clip start fade.</param>
        /// <param name="mode">How to stop the other clips.</param>
        /// <returns>The duplicated animation state of the clip.</returns>
        public SpriteAnimationState CrossFadeQueued(string name, float fadeLength, QueueMode queue, PlayMode mode)
        {
            SpriteAnimationState state = this[name];
            if (state == null)
                return null;

            bool isPlaying = IsLayerPlaying(state.layer);

            if (queue == QueueMode.PlayNow || !isPlaying)
            {
                string newName = state.name + " - Queued Clone " + cloneID++;

                SpriteAnimationState tmpState = new SpriteAnimationState(state, newName);
                tmpState.removeAfterStop = true;
                AddState(tmpState);

                CrossFade(tmpState, fadeLength, mode);

                return tmpState;
            }
            else
            {
                string newName = state.name + " - Queued Clone " + cloneID++;

                SpriteAnimationState tmpState = new SpriteAnimationState(state, newName);
                tmpState.removeAfterStop = true;
                AddState(tmpState);


                SpriteAnimationQueueItem item = new SpriteAnimationQueueItem();
                item.state = tmpState;
                item.playMode = mode;
                item.fadeLength = fadeLength;
                crossFadeQueue.Add(item);

                return tmpState;
            }

            return null;
        }







        /// <summary>
        /// Stops all playing animations that were started with this Animation.
        /// Stopping an animation also Rewinds it to the Start. 
        /// </summary>
        public void Stop()
        {
            StopAll();
        }


        /// <summary>
        /// Stops an animation named name.
        /// Stopping an animation also Rewinds it to the Start. 
        /// </summary>
        /// <remarks></remarks>
        /// <param name="name">The name of animation clip you want to stop.</param>
        public void Stop(string name)
        {
            SpriteAnimationState state = this[name];
            if (state != null)
                Stop(state);
        }



        /// <summary>
        /// Stops all playing animations that were started with this Animation.
        /// Stopping an animation also Rewinds it to the Start. 
        /// </summary>
        /// <remarks></remarks>
        public void StopAll()
        {
            foreach (SpriteAnimationState state in allStates)
                Stop(state);
        }


        void StopLayer(int layer)
        {
            foreach (SpriteAnimationState state in allStates)
            {
                if ( state.layer == layer )
                    Stop(state);
            }
        }


        void Stop( SpriteAnimationState state)
        {
            state.time = 0;
            state.weight = 0f;
            state.enabled = false;
        }










        /// <summary>
        /// Rewinds all animations
        /// </summary>
        /// <remarks></remarks>
        public void Rewind()
        {
            foreach (SpriteAnimationState state in allStates)
                Rewind(state);
        }


        /// <summary>
        /// Rewinds the animation named name.
        /// </summary>
        /// <param name="name">The animatino named name you want rewind.</param>
        /// <remarks></remarks>
        public void Rewind(string name)
        {
            SpriteAnimationState state = this[name];
            if (state != null)
                Rewind(state);
        }



        void Rewind(SpriteAnimationState state)
        {
            if (state.speed < 0f)
                state.time = state.clip.length;
            else
                state.time = 0f;
        }








        /// <summary>
        /// Pause a clip is playing.
        /// </summary>
        /// <param name="name">The name of animation clip you want to pause.</param>
        /// <remarks></remarks>
        public void Pause(string name)
        {
            SpriteAnimationState state = this[name];
            if (state != null)
            {
                state.enabled = false;
                state._isPaused = true;
            }
            else
            {
                Debug.LogError("Can not find a clip named " + name);
            }
        }



        /// <summary>
        /// Resume a clip. If not paused, will play the clip.
        /// </summary>
        /// <param name="name">The name of animation clip you want to resume.</param>
        /// <remarks></remarks>
        public void Resume(string name)
        {
            SpriteAnimationState state = this[name];
            if (state != null)
            {
                state.enabled = true;
            }
            else
            {
                Debug.LogError("Can not find a clip named " + name);
            }
        }








        /// <summary>
        /// Are we playing any animations?
        /// </summary>
        /// <remarks></remarks>
        public bool isPlaying
        {
            get
            {
                bool ret = false;
                foreach (SpriteAnimationState state in allStates)
                    ret |= state.enabled;
                return ret;
            }
        }


        /// <summary>
        /// Is the animation named name playing?
        /// </summary>
        /// <param name="name">Name of animation</param>
        /// <returns>True if the animation named name playing, False is not or animation no exist.</returns>
        /// <remarks></remarks>
        public bool IsPlaying(string name)
        {
            SpriteAnimationState state = this[name];
            return state == null ? false : state.enabled;
        }


        internal bool IsLayerPlaying(int layer)
        {
            foreach (SpriteAnimationState state in allStates)
                if (state.layer == layer && state.enabled)
                    return true;

            return false;
        }


        internal SpriteAnimationState IsLayerCanFade(int layer, float fadeLength)
        {
            int i = 0;
            SpriteAnimationState lastState = null;

            foreach (SpriteAnimationState state in allStates)
            {
                if ( state.layer == layer && state.enabled)
                {
                    i++;
                    lastState = state;
                }
            }

            if (i == 1 && lastState != null && (lastState.wrapMode == WrapMode.Default || lastState.wrapMode == WrapMode.Once))
            {
                if (lastState.time >= lastState.clip.length - fadeLength)
                    return lastState;
            }

            return null;
        }

        internal SpriteAnimationState IsAllLayerCanFade( float fadeLength )
        {
            int i = 0;
            SpriteAnimationState lastState = null;

            foreach (SpriteAnimationState state in allStates)
            {
                if (state.enabled)
                {
                    i++;
                    lastState = state;
                }
            }

            if (i == 1 && lastState != null && (lastState.wrapMode == WrapMode.Default || lastState.wrapMode == WrapMode.Once))
            {
                if (lastState.time >= lastState.clip.length - fadeLength)
                    return lastState;
            }

            return null;
        }
    }














}