using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{



    public partial class SpriteAnimation : MonoBehaviour
    {
        void FixedUpdate()
        {
            UpdateInternal(Time.fixedDeltaTime);

            if (Application.isPlaying && updateMode == SpriteRendererUpdateMode.FixedUpdate )
                CalculateData();
        }


        void Update()
        {
            if (Application.isPlaying && updateMode == SpriteRendererUpdateMode.Update)
                CalculateData();
        }


        void LateUpdate()
        {
            if (Application.isPlaying && updateMode == SpriteRendererUpdateMode.LateUpdate)
                CalculateData();
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


        /// <summary>
        /// Update all animation state.
        /// </summary>
        /// <param name="delta">Delta time.</param>
        public void UpdateInternal(float delta)
        {
            bool isPlaying = Application.isPlaying;

            foreach (SpriteAnimationState state in allStates)
            {


                if (state.needFade)
                    state.Fade(delta);


                if (!state.enabled)
                    continue;


                if ( !state._isPaused )
                    state.time += delta * state.speed;



                if ( (state.wrapMode == WrapMode.Once || state.wrapMode == WrapMode.Default) &&
                    ( (state.speed > 0f) ? state.time > state.length : state.time < 0f)
                    )
                {
                    Stop(state);

                    //remove all state can be played and play it
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
            crossFadeQueue.RemoveAll( crossFadeQueueChecker );


            //remove tmp state
            clearRemoveList();


            if ( !isPlaying )
                CalculateData();
        }


        /// <summary>
        /// Calculate all animation data immediate.
        /// </summary>
        public void CalculateData()
        {
            if (!spriteRenderer.visible)
                return;


            EvaluateCompoment();
            TransformComponent();



            foreach (SpriteAnimationEvent evt in evtList)
                gameObject.SendMessage(evt.functionName, evt, evt.messageOption);
            evtList.Clear();


            if (autoApplyAfterUpdate)
                spriteRenderer.Apply();
        }













        private void EvaluateCompoment()
        {
            for (int i = 0; i < spriteRenderer.GetAttachSpriteCount(); i++)
            {
                SpriteTransform sprTransform = spriteRenderer.GetSpriteTransform(i);

                EvaluateTransform(sprTransform);
            }



            if (preTransformUpdate != null)
                preTransformUpdate(this);
        }




        internal void EvaluateTransform(SpriteTransform sprTransform)
        {
            sprTransform.ZeroTransform();


            for (int idx = 0; idx < sprTransform.attachStateComponent.Count; idx++)
            {
                SpriteTransform.StateComponentPair pair = sprTransform.attachStateComponent[idx];

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


                if (pair.lastRefClip != kf.refClip)
                {
                    if (pair.lastRefClip != null)
                    {
                        string name = pair.state.name + "_" + pair.component.fullPath + "_" + pair.lastRefClip.name;
                        SpriteAnimationState state = this[name];

                        if (state != null)
                        {
                            Stop(state);
                        }

                        pair.lastRefClip = null;
                    }


                    if (kf.refClip != null)
                    {
                        string name = pair.state.name + "_" + pair.component.fullPath + "_" + kf.refClip.name;

                        if (!animationStates.ContainsKey(name))
                        {
                            AddClip(kf.refClip, name);
                            SpriteAnimationState state = this[name];

                            state.parentState = pair.state;
                            state.parentTransform = pair.applyTo;
                            state.parentComponentLayer = (int)pair.component.layer;
                            state.removeAfterStop = true;

                            state.layer = pair.state.layer;
                            state.weight = pair.state.weight;

                            state.lastTime = 0f;
                            state.lastFrameIndex = -1;
                            state.lastEvaluateTime = 0;


                            pair.lastRefClip = kf.refClip;
                            state.parentState.subClipState.Add(state);

                            state.enabled = true;
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





        /// <summary>
        /// Apply the root bone motion to transform in local space.
        /// </summary>
        public bool applyRootToTransform = false;

        static SpriteTransform parentTransform = new SpriteTransform();

        private void TransformComponent()
        {
            Transform tran = transform;

            foreach (SpriteTransform rootTransform in stateRoot)
            {
                parentTransform.ResetTransform();

                if (applyRootToTransform)
                {
                    for (SpriteTransform iter = rootTransform.firstChild; iter != null; iter = iter.next)
                    {
                        TransformNode(iter, parentTransform);
                    }

                    Vector3 pos = rootTransform.position;
                    pos.z = transform.localPosition.z;
                    transform.localPosition = pos;
                    transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rootTransform.rotation));
                    transform.localScale = new Vector3(rootTransform.scale.x, rootTransform.scale.y, transform.localScale.z);

                    rootTransform.position = Vector2.zero;
                    rootTransform.rotation = 0;
                    rootTransform.scale = Vector2.one;
                    rootTransform.shear = Vector2.zero;
                    rootTransform.color = Color.white;
                }
                else
                    TransformNode(rootTransform, parentTransform);
            }

            if (postTransformUpdate != null)
                postTransformUpdate(this);
        }




        private void TransformNode(SpriteTransform sTransform, SpriteTransform parentTransform)
        {
            {
                //TRS
                float r = Mathf.Deg2Rad * (parentTransform.rotation);
                float sin = Mathf.Sin(r);
                float cos = Mathf.Cos(r);

                float sx = sTransform.position.x * parentTransform.scale.x;
                float sy = sTransform.position.y * parentTransform.scale.y;

                sTransform.position.x = parentTransform.position.x + (sx * cos - sy * sin);
                sTransform.position.y = parentTransform.position.y + (sx * sin + sy * cos);


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

        SpriteAnimatonDelegate preTransformUpdate = new SpriteAnimatonDelegate(dummyHandler);
        SpriteAnimatonDelegate postTransformUpdate = new SpriteAnimatonDelegate(dummyHandler);

        static void dummyHandler(SpriteAnimation ani)
        {
        }


        /// <summary>
        /// Add a delegate to handle animation updates.
        /// </summary>
        /// <param name="type">Handler type.</param>
        /// <param name="callback">A function delegate you want to use as handler.</param>
        public void AddComponentUpdateHandler(SpriteAnimationComponentUpdateType type, SpriteAnimatonDelegate callback)
        {
            if (type == SpriteAnimationComponentUpdateType.PreUpdateComponent)
            {
                preTransformUpdate += callback;
            }

            else if (type == SpriteAnimationComponentUpdateType.PostUpdateComponent)
            {
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
                preTransformUpdate -= callback;
            }

            else if (type == SpriteAnimationComponentUpdateType.PostUpdateComponent)
            {
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
                preTransformUpdate = new SpriteAnimatonDelegate(dummyHandler);
            }

            else if (type == SpriteAnimationComponentUpdateType.PostUpdateComponent)
            {
                postTransformUpdate = new SpriteAnimatonDelegate(dummyHandler);
            }
        }

    }














}