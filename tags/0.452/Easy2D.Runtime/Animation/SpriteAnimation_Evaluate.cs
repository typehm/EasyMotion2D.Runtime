using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{



    public partial class SpriteAnimation : MonoBehaviour
    {




        void Update()
        {
            UpdateInternal(Time.smoothDeltaTime);
        }






        List<SpriteAnimationState> removeList = new List<SpriteAnimationState>();


        System.Predicate<SpriteAnimationState> playingQueueChecker = new System.Predicate<SpriteAnimationState>(checkPlayingQueue);
        static bool checkPlayingQueue(SpriteAnimationState item)
        {
            if ( ! item._animation.IsLayerPlaying(item.layer) )
            {
                item.time = 0f;
                item.weight = 1f;
                item.enabled = true;
                return true;
            }
            return false;
        }


        System.Predicate<SpriteAnimationQueueItem> crossFadeQueueChecker = new System.Predicate<SpriteAnimationQueueItem>(checkCrossFadeQueue);
        static bool checkCrossFadeQueue(SpriteAnimationQueueItem item)
        {
            SpriteAnimationState fadeState = item.playMode == PlayMode.StopAll ?
                item.state._animation.IsAllLayerCanFade(item.fadeLength)  :
                item.state._animation.IsLayerCanFade(item.state.layer, item.fadeLength);

            if (fadeState != null)
            {
                item.state._animation.CrossFade(item.state, item.fadeLength, item.playMode);
                return true;
            }

            return false;
        }



        public void UpdateInternal(float delta)
        {
            bool isPlaying = Application.isPlaying;

            foreach (SpriteAnimationState state in allStates)
            {
                if (state.needFade)
                    state.Fade(delta);


                if (!state.enabled)
                    continue;


                state.time += delta * state.speed;



                if ( (state.wrapMode == WrapMode.Once || state.wrapMode == WrapMode.Default) &&
                    ( (state.speed > 0f) ? state.time > state.length : state.time < 0f)
                    )
                {
                    if (state.removeAfterStop)
                    {
                        removeList.Add(state);
                    }

                    Stop(state.name);

                    //remove all state can be played and play it
                    if (playingQueueChecker != null)
                        playingQueue.RemoveAll( playingQueueChecker );
                    continue;
                }



                state.curve.wrapMode = state.wrapMode;

                float fIdx = state.curve.Evaluate(state.time);
                int idx = (int)fIdx;

                if ( smoothAnimation )
                    state.lastEvaluateTime = fIdx * state.clip.tick;
                else
                    state.lastEvaluateTime = idx * state.clip.tick;


                if (isPlaying && state.weight > 0f)
                {
                    FireAnimationEvent(state, idx);
                }


                state.lastFrameIndex = idx;
                state.lastTime = state.time;
            }



            //remove all state can be fade and fade it
            if ( crossFadeQueueChecker != null )
                crossFadeQueue.RemoveAll( crossFadeQueueChecker );


            //remove tmp state
            if ( removeList.Count > 0 )
            {
                foreach (SpriteAnimationState s in removeList)
                {
                    animationStates.Remove(s.name);
                    allStates.Remove(s);
                }
                removeList.Clear();
            }


            if (!spriteRenderer.visible)
                return;


            EvaluateCompoment();
            TransformComponent();

            foreach (SpriteAnimationEvent evt in evtList)
                gameObject.SendMessage(evt.functionName, evt, evt.messageOption);
            evtList.Clear();

            spriteRenderer.Apply();
        }











        static SpriteTransform parentTransform = new SpriteTransform();

        private void TransformComponent()
        {
            foreach (SpriteTransform rootTransform in stateRoot)
            {
                parentTransform.ResetTransform();
                TransformNode(rootTransform, parentTransform);
            }

            if (postTransformUpdate != null)
                postTransformUpdate(this);
        }





        private void EvaluateCompoment()
        {
            for (int i = 0; i < spriteRenderer.GetAttachSpriteCount(); i++)
            {
                SpriteTransform sprTransform = spriteRenderer.GetSpriteTransform(i);
                sprTransform.ZeroTransform();


                foreach (SpriteTransform.StateComponentPair pair in sprTransform.attachStateComponent)
                {
                    SpriteAnimationKeyFrame kf = null;

                    if (pair.state.blendMode == AnimationBlendMode.Additive)
                    {
                        //reset buffer
                        transformPropertys[1] = transformPropertys[2] = transformPropertys[3] = 0f;
                        transformPropertys[4] = transformPropertys[5] = transformPropertys[6] = transformPropertys[7] = transformPropertys[8] = transformPropertys[9] = 0f;
                        transformPropertys[10] = transformPropertys[11] = 0f;

                        kf = pair.component.EvaluateAddtive(pair.state.lastEvaluateTime, sprTransform, transformPropertys);
                    }
                    else
                    {
                        //reset buffer
                        transformPropertys[1] = transformPropertys[2] = transformPropertys[3] = 0f;
                        transformPropertys[4] = transformPropertys[5] = transformPropertys[6] = transformPropertys[7] = transformPropertys[8] = transformPropertys[9] = 1f;
                        transformPropertys[10] = transformPropertys[11] = 0f;

                        kf = pair.component.EvaluateBlending(pair.state.lastEvaluateTime, sprTransform, transformPropertys);
                    }

                    if (pair.applyTo.lastRefClip != kf.refClip)
                    {
                        if (pair.applyTo.lastRefClip != null)
                        {
                            string name = pair.state.name + "_" + pair.applyTo.lastRefClip.name;
                            SpriteAnimationState state = this[name];

                            if (state != null)
                                Stop(state);
                        }


                        if (kf.refClip != null)
                        {
                            string name = pair.state.name + "_" + kf.refClip.name;

                            if (!animationStates.ContainsKey(name))
                            {
                                AddClip(kf.refClip, name);
                                SpriteAnimationState state = this[name];

                                state.parentTransform = spriteRenderer.GetSpriteTransformByFullPathHash(pair.component._fullPathHash);
                                state.parentComponentLayer = (int)pair.component.layer;
                                state.removeAfterStop = true;

                                state.layer = pair.state.layer;
                                state.weight = pair.state.weight;
                                state.enabled = true;

                                state.lastTime = 0f;
                                state.lastFrameIndex = -1;
                                state.lastEvaluateTime = 0;


                                pair.applyTo.lastRefClip = kf.refClip;
                            }
                        }

                    }


                    float weight = pair.weight;

                    
                    {
                        sprTransform.position.x += transformPropertys[1] * weight;
                        sprTransform.position.y += transformPropertys[2] * weight;
                        sprTransform.rotation += transformPropertys[3] * weight;
                        sprTransform.scale.x += transformPropertys[4] * weight;
                        sprTransform.scale.y += transformPropertys[5] * weight;
                        sprTransform.color.r += transformPropertys[6] * weight;
                        sprTransform.color.g += transformPropertys[7] * weight;
                        sprTransform.color.b += transformPropertys[8] * weight;
                        sprTransform.color.a += transformPropertys[9] * weight;
                        sprTransform.shear.x += transformPropertys[10] * weight;
                        sprTransform.shear.y += transformPropertys[11] * weight;
                    }
                }


            }


            if (preTransformUpdate != null)
                preTransformUpdate(this);
        }





        private void TransformNode(SpriteTransform sTransform, SpriteTransform parentTransform)
        {
            float r = 0;
            float sin = 0;
            float cos = 0;

            float sx = 0;
            float sy = 0;

            {
                //TRS
                r = Mathf.Deg2Rad * (parentTransform.rotation);
                sin = Mathf.Sin(r);
                cos = Mathf.Cos(r);

                sx = sTransform.position.x;
                sy = sTransform.position.y;

                sTransform.position.x = parentTransform.position.x + (sx * cos - sy * sin) * parentTransform.scale.x;
                sTransform.position.y = parentTransform.position.y + (sx * sin + sy * cos) * parentTransform.scale.y;


                sTransform.scale.x *= parentTransform.scale.x;
                sTransform.scale.y *= parentTransform.scale.y;

                sTransform.rotation += parentTransform.rotation;
            }

            for (SpriteTransform iter = sTransform.firstChild; iter != null; iter = iter.next)
            {
                TransformNode(iter, sTransform);
            }
        }



        List<SpriteAnimationEvent> evtList = new List<SpriteAnimationEvent>();

        private void FireAnimationEvent(SpriteAnimationState state, int idx)
        {
            //计算当前时间与最后次更新的时间差
            float t = state.time - state.lastTime;
            //计算出步进了几个tick
            int step = (int)(t / state._clip.tick);
            //保存起始的帧索引
            int li = state.lastFrameIndex;
            //事件数组的长度
            int e = state._clip.events.Length;

            //搜索时间内的事件
            for (float st = state.lastTime; st < state.time; st += state._clip.tick)
            {
                //得到当前时间的索引
                int currFrameIdx = (int)state.curve.Evaluate(st);
                //检查是否和上次不一致,如果是则进行一个线性事件搜索
                if (li != currFrameIdx)
                {
                    for (int ei = 0; ei < e; ei++)
                    {
                        if (state._clip.events[ei].frameIndex == currFrameIdx)
                        {
                            SpriteAnimationEvent evt = state._clip.events[ei];
                            evtList.Add(evt);
                            break;
                        }
                    }
                }
                state.lastFrameIndex = li = currFrameIdx;
            }

            if (state.lastFrameIndex != idx)
            {

                for (int ei = 0; ei < e; ei++)
                {
                    if (state._clip.events[ei].frameIndex == idx)
                    {
                        SpriteAnimationEvent evt = state._clip.events[ei];
                        evtList.Add(evt);
                        break;
                    }
                }
            }
        }





        static float[] transformPropertys = new float[]
        {
            0f,//keyframe
            0f, 0f, //position
            0f, //rotation
            1f, 1f, //scale
            1f, 1f, 1f, 1f, //color
            0f, 0f, //shear
        };






        //static void 

        SpriteAnimatonDelegate preTransformUpdate;
        SpriteAnimatonDelegate postTransformUpdate;

        /// <summary>
        /// Add a delegate to handle animation updates.
        /// </summary>
        /// <param name="type">Handler type.</param>
        /// <param name="callback">A function delegate you want to use as handler.</param>
        public void AddComponentUpdateHandler(SpriteAnimationComponentUpdateType type, SpriteAnimatonDelegate callback)
        {
            if (type == SpriteAnimationComponentUpdateType.PreUpdateComponent)
            {
                //if (preTransformUpdate == null)
                //    preTransformUpdate = callback;
                //else
                    preTransformUpdate += callback;
            }

            else if (type == SpriteAnimationComponentUpdateType.PostUpdateComponent)
            {
                //if (postTransformUpdate == null)
                //    postTransformUpdate = new SpriteAnimatonDelegate(callback);
                //else
                    postTransformUpdate += callback;
            }
        }





        /// <summary>
        /// Remove a delegate to handle animation updates.
        /// </summary>
        /// <param name="type">Handler type.</param>
        /// <param name="callback">A function delegate you want to use as handler.</param>
        public void RemoveComponentUpdateHandler(SpriteAnimationComponentUpdateType type, SpriteAnimatonDelegate callback)
        {
            if (type == SpriteAnimationComponentUpdateType.PreUpdateComponent)
            {
                if (preTransformUpdate != null)
                    preTransformUpdate -= callback;
            }

            else if (type == SpriteAnimationComponentUpdateType.PostUpdateComponent)
            {
                if (postTransformUpdate != null)
                    postTransformUpdate -= callback;
            }
        }



        /// <summary>
        /// Clear all handler delegates in animation.
        /// </summary>
        /// <param name="type">Handler type.</param>
        public void ClearComponentUpdateHandler(SpriteAnimationComponentUpdateType type)
        {
            if (type == SpriteAnimationComponentUpdateType.PreUpdateComponent)
            {
                preTransformUpdate = null;
            }

            else if (type == SpriteAnimationComponentUpdateType.PostUpdateComponent)
            {
                postTransformUpdate = null;
            }
        }

    }














}