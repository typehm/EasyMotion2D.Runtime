using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{









    /// <summary>
    /// Decide component's render order in a SpriteAnimation in ascending.
    /// </summary>
    public enum SpriteAnimationComponentLayer
    {
        Layer0 = 0,
        Layer1,
        Layer2,
        Layer3,
        Layer4,
        Layer5,
        Layer6,
        Layer7,
        Layer8,
        Layer9,
        Layer10,
        Layer11,
        Layer12,
        Layer13,
        Layer14,
        Layer15,
        Layer16,
        Layer17,
        Layer18,
        Layer19,
        Layer20,
        Layer21,
        Layer22,
        Layer23,
        Layer24,
        Layer25,
        Layer26,
        Layer27,
        Layer28,
        Layer29,
        Layer30,
        Layer31
    }







    /// <summary>
    /// A component of a <see cref="SpriteAnimationClip">animation clip</see>. Stored all datas for a sub animation.<br/>
    /// Internal class. You do not need to use this.
    /// </summary>
    [System.Serializable]
    public class SpriteAnimationComponent
    {
        /// <summary>
        /// The name of component.
        /// </summary>
        public string name = "";

        /// <summary>
        /// The full path of component.
        /// </summary>
        public string fullPath
        {
            get
            {
                return _fullPath;
            }
        }

        /// <summary>
        /// The layer of component. This used to decide the rendering order of components in clip.
        /// </summary>
        public SpriteAnimationComponentLayer layer = SpriteAnimationComponentLayer.Layer4;

        /// <summary>
        /// All keyframes of the animation component.
        /// </summary>
        public SpriteAnimationKeyFrame[] keyFrames = new SpriteAnimationKeyFrame[1] { new SpriteAnimationKeyFrame() };

        /// <summary>
        /// All curves of the animation keyframes.
        /// </summary>
        public SpriteAnimationCurve[] curves = new SpriteAnimationCurve[0] { };


        [SerializeField]
        internal int parentIndex = -2;

        public int index = -2;

        /// <summary>
        /// 
        /// </summary>
        public int[] children = new int[] { };


        /// <summary>
        /// Reserved.
        /// </summary>
        [HideInInspector]
        public bool expand = false;

        /// <summary>
        /// Reserved.
        /// </summary>
        [HideInInspector]
        public bool showCurve = false;

        /// <summary>
        /// Reserved.
        /// </summary>
        [HideInInspector]
        public bool visible = true;



        [SerializeField]
        //[HideInInspector]
        internal string _fullPath = "";

        [SerializeField]
        internal int _fullPathHash = 0;


        internal int maxIndex
        {
            get
            {
                if (_maxIndex == -1)
                {
                    _maxIndex = GetMaxIndex();
                }

                return _maxIndex;
            }
        }

        internal int GetMaxIndex()
        {
            int idx = keyFrames.Length - 1;
            int ret = 0;

            if (idx >= 0)
            {
                if (keyFrames[idx].isRefClip)
                {
                    ret = keyFrames[idx].frameIndex;

                    SpriteAnimationClip refClip = keyFrames[idx].refClip;
                    int ml = refClip.getMaxComponentIndex(refClip.root);

                    float t_tick = 1f / (float)refClip.frameRate;
                    float tick = 1f / (float)clip.frameRate;

                    int l = (int)((ml * t_tick) / tick);
                    ret += (l + 0);
                }
                else
                    ret = keyFrames[idx].frameIndex + 0;
            }
            return ret;
        }


        [SerializeField]
        internal int _maxIndex = -1;


        [SerializeField]
        private int _length = 0;


        [SerializeField]
        private SpriteAnimationComponent _parent;


        /// <summary>
        /// The parent of the component.
        /// </summary>
        public SpriteAnimationComponent parent
        {
            get
            {
                return parentIndex == -2 ? null :
                    (parentIndex == -1 ? clip.root : clip.subComponents[parentIndex]);
            }

            set
            {
                //remove from old parent
                if (parentIndex != -2)
                {
                    parent.RemoveChild(this);
                }

                //set new parent
                parentIndex = value == null ? -2 : value.index;

                //attach to new parent
                if (value != null)
                {
                    value.AddChild(this);
                }

                //remove from clip
                else if (value == null && this != clip.root)
                {
                    List<SpriteAnimationComponent> tmp = new List<SpriteAnimationComponent>();
                    walkSubComponent(this, tmp);


                    foreach (SpriteAnimationComponent comp in tmp)
                        clip.RemoveChild(comp);
                }

                _fullPath = GetFullPath();
                _fullPathHash = _fullPath.GetHashCode();
            }
        }


        internal void walkSubComponent(SpriteAnimationComponent comp, List<SpriteAnimationComponent> compList)
        {
            foreach (int idx in comp.children)
                walkSubComponent(clip.subComponents[idx], compList);

            compList.Add(comp);
        }


        /// <summary>
        /// The owner clip of the component.
        /// </summary>
        public SpriteAnimationClip clip = null;


        /// <summary>
        /// The children of the component has.
        /// </summary>
        public SpriteAnimationComponent[] childs = new SpriteAnimationComponent[0] { };



        public SpriteAnimationComponent(string name)
        {
            this.name = name;
        }





        internal void AddChild(SpriteAnimationComponent child)
        {
            clip.AddChild(child);

            List<int> tmp = new List<int>(children);
            if (!tmp.Contains(child.index))
            {
                tmp.Add(child.index);
                children = tmp.ToArray();
            }
        }


        internal void RemoveChild(SpriteAnimationComponent child)
        {
            if (child == null)
                return;

            List<int> tmp = new List<int>(children);
            tmp.Remove(child.index);


            children = tmp.ToArray();
        }


        internal string GetFullPath()
        {
            string ret = "/" + name;

            if (parent != null)
                ret = parent.fullPath + ret;

            return ret;
        }


        /// <summary>
        /// Is component a child of this?
        /// </summary>
        public bool IsChild(SpriteAnimationComponent component)
        {
            if (component == null || component == this)
                return false;

            while ((component = component.parent) != null)
            {
                if (component == this)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Add a keyframe of component.
        /// </summary>
        public void AddKeyframe(SpriteAnimationKeyFrame kf)
        {
            float tick = 1f / clip.frameRate;
            kf.time = kf.frameIndex * tick;

            List<SpriteAnimationKeyFrame> tmp = new List<SpriteAnimationKeyFrame>(keyFrames);
            tmp.Add(kf);
            tmp.Sort(SpriteAnimationKeyFrameComparerByFrameIndex.comparer);
            keyFrames = tmp.ToArray();

            CalcCurve(kf);
        }

        /// <summary>
        /// Remove a keyframe of component.
        /// </summary>
        public void RemoveKeyframe(SpriteAnimationKeyFrame kf)
        {
            List<SpriteAnimationKeyFrame> tmp = new List<SpriteAnimationKeyFrame>(keyFrames);
            tmp.Remove(kf);
            tmp.Sort(SpriteAnimationKeyFrameComparerByFrameIndex.comparer);
            keyFrames = tmp.ToArray();

            CalcCurve(null);
        }

        /// <summary>
        /// Find a keyframe at index.
        /// </summary>
        /// <returns>If not found, return null.</returns>
        public SpriteAnimationKeyFrame GetKeyframeByIndex(int idx)
        {
            foreach (SpriteAnimationKeyFrame kf in keyFrames)
            {
                if (idx == kf.frameIndex)
                    return kf;
            }
            return null;
        }


        /// <summary>
        /// Find a keyframe at time.
        /// </summary>
        /// <returns>If not found, return null.</returns>
        public SpriteAnimationKeyFrame GetKeyframeByTime(float time)
        {
            foreach (SpriteAnimationKeyFrame kf in keyFrames)
            {
                if (time == kf.time)
                    return kf;
            }
            return null;
        }


        /// <summary>
        /// return keyframe closest index.
        /// </summary>
        /// <returns>Never return null.</returns>
        public SpriteAnimationKeyFrame GetClosestKeyframeByIndex(int idx)
        {
            foreach (SpriteAnimationKeyFrame kf in keyFrames)
            {
                if (idx <= kf.frameIndex)
                    return kf;
            }


            if (keyFrames.Length > 0 && keyFrames[keyFrames.Length - 1].frameIndex <= idx)
            {
                return keyFrames[keyFrames.Length - 1];
            }


            return null;
        }



        static float[] defaultValueArray = new float[]{
            0f,//keyframe
            0f, 0f, //position
            0f, //rotation
            1f, 1f, //scale
            1f, 1f, 1f, 1f, //color
            0f, 0f, //shear
        };

        float GetDefaultValue(SpriteAnimationCurveType type)
        {
            return defaultValueArray[(int)type];
        }


        void ApplyCurveChangeToKeyframe()
        {
        }

        /// <summary>
        /// Calculate the curves of component.
        /// </summary>
        public void CalcCurve(SpriteAnimationKeyFrame newKf)
        {
            _maxIndex = GetMaxIndex();

            List<SpriteAnimationKeyFrame> tmp = new List<SpriteAnimationKeyFrame>(keyFrames);
            tmp.Sort(SpriteAnimationKeyFrameComparerByFrameIndex.comparer);
            keyFrames = tmp.ToArray();

            ApplyCurveChangeToKeyframe();

            List<SpriteAnimationCurve> tmpCurves = new List<SpriteAnimationCurve>();
            for (int i = (int)SpriteAnimationCurveType.KeyframeIndex, e = (int)SpriteAnimationCurveType.ShearY; i <= e; i++)
            {
                SpriteAnimationCurve curve = new SpriteAnimationCurve();

                curve.type = (SpriteAnimationCurveType)i;
                curve.curve = new AnimationCurve();

                foreach (SpriteAnimationCurve _curve in curves)
                {
                    if (_curve.type == curve.type)
                        curve.interpolation = _curve.interpolation;
                }

                tmpCurves.Add(curve);
            }

            float tick = 1f / clip.frameRate;
            int idx = 0;

            tmpCurves[(int)SpriteAnimationCurveType.KeyframeIndex].curve = new AnimationCurve();
            tmpCurves[(int)SpriteAnimationCurveType.KeyframeIndex].interpolation = true;

            foreach (SpriteAnimationKeyFrame kf in keyFrames)
            {
                float time = kf.frameIndex * tick; ;
                kf.isSpriteValid = kf.sprite != null;

                bool needSmooth = kf == newKf;

                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.KeyframeIndex], time, idx + 0.005f, false, 0, false);

                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.PositionX], time, kf.position.x, true, 1, needSmooth);
                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.PositionY], time, kf.position.y, true, 0, needSmooth);

                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.Rotate], time, kf.rotation, true, 0, needSmooth);

                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.ScaleX], time, kf.scale.x, true, 0, needSmooth);
                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.ScaleY], time, kf.scale.y, true, 0, needSmooth);

                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.ShearX], time, kf.shear.x, true, 0, needSmooth);
                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.ShearY], time, kf.shear.y, true, 0, needSmooth);

                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.ColorR], time, kf.color.r, true, 0, needSmooth);
                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.ColorG], time, kf.color.g, true, 0, needSmooth);
                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.ColorB], time, kf.color.b, true, 0, needSmooth);
                AddKeyToCurve(tmpCurves[(int)SpriteAnimationCurveType.ColorA], time, kf.color.a, true, 0, needSmooth);

                idx++;
            }


            //for (int i = 0, e = tmpCurves[(int)SpriteAnimationCurveType.KeyframeIndex].curve.keys.Length; i < e; i++)
            //    tmpCurves[(int)SpriteAnimationCurveType.KeyframeIndex].curve.SmoothTangents(i, 1f);


            //remove curves if values never change
            tmpCurves.RemoveAll(delegate(SpriteAnimationCurve curve)
            {
                if (curve.type == SpriteAnimationCurveType.KeyframeIndex)
                    return false;

                bool ret = true;

                Keyframe fkf = curve.curve.keys[0];
                curve.defaultValue = fkf.value;

                bool isFkDef = fkf.value == GetDefaultValue(curve.type);

                foreach (Keyframe kf in curve.curve.keys)
                {
                    if (fkf.value != kf.value)
                        return false;
                }


                if (!isFkDef)
                {
                    curve.length = 0;
                    return false;
                }

                return ret;
            });


            foreach (SpriteAnimationCurve curve in tmpCurves)
            {
                if (curve.type == SpriteAnimationCurveType.KeyframeIndex)
                    continue;

                if (!curve.interpolation)
                {
                    Keyframe[] kfs = curve.curve.keys;
                    curve.curve = new AnimationCurve();

                    for (int i = 0, e = kfs.Length; i < e; i++)
                    {
                        Keyframe kf = kfs[i];
                        kf.inTangent = float.PositiveInfinity;
                        kf.outTangent = float.NegativeInfinity;

                        curve.curve.AddKey(kf);
                    }
                }
            }

            curves = tmpCurves.ToArray();
        }

        private void AddKeyToCurve(SpriteAnimationCurve curve, float time, float value, bool copyTangent, int tangentMode, bool smoothTangent)
        {
            Keyframe kf = new Keyframe(time, value);
            kf.tangentMode = 0;

            foreach (SpriteAnimationCurve _curve in curves)
            {
                if (_curve.type == curve.type)
                {
                    foreach (Keyframe _kf in _curve.curve.keys)
                    {
                        if (_kf.time == time && copyTangent)
                        {
                            kf.inTangent = _kf.inTangent;
                            kf.outTangent = _kf.outTangent;
                            kf.tangentMode = _kf.tangentMode;
                            break;
                        }
                    }
                    break;
                }
            }


            curve.curve.AddKey(kf);
            curve.length = curve.curve.keys.Length;


            if (smoothTangent)
            {
                //int i = 0;

                //foreach (Keyframe _kf in curve.curve.keys)
                //{
                //    if (_kf.time == time && copyTangent)
                //    {
                //        curve.curve.SmoothTangents(i, 1f);
                //        break;
                //    }
                //    i++;
                //}
            }
        }

        internal SpriteAnimationKeyFrame GetKeyframeAtTime(float time)
        {
            return keyFrames[(int)curves[0].curve.Evaluate(time)];
        }

        internal SpriteAnimationKeyFrame EvaluateAddtive(float time, SpriteTransform transform, float[] transformProperty)
        {
            int idx = (int)curves[0].curve.Evaluate(time);
            SpriteAnimationKeyFrame kf = keyFrames[idx];

            SpriteAnimationCurve curve = null;
            for (int i = 1, e = curves.Length; i < e; i++)
            {
                curve = curves[i];
                transformProperty[(int)curve.type] = curve.length == 0 ? 0f : (curve.curve.Evaluate(time) - curve.defaultValue);
            }
            return kf;
        }

        internal SpriteAnimationKeyFrame EvaluateBlending(float time, SpriteTransform transform, float[] transformProperty)
        {
            int idx = (int)curves[0].curve.Evaluate(time);
            SpriteAnimationKeyFrame kf = keyFrames[idx];

            if (!transform.isSpriteValid && kf.isSpriteValid )
            {
                transform._sprite = kf.sprite;
                transform.isSpriteValid = true;
            }

            float v = 0f;
            foreach (SpriteAnimationCurve curve in curves)
            {
                transformProperty[(int)curve.type] = curve.length == 0 ? curve.defaultValue : curve.curve.Evaluate(time);
            }
            return kf;
        }


        /// <summary>
        /// Evaluate the component state at time.
        /// </summary>
        public SpriteAnimationKeyFrame Evaluate(float time, ref Vector2 position, ref float rotation, ref Vector2 scale, ref Vector2 shear, ref Color color, ref Sprite spr, ref SpriteAnimationClip refClip)
        {
            SpriteAnimationKeyFrame ret = keyFrames[0];
            foreach (SpriteAnimationCurve curve in curves)
            {
                switch (curve.type)
                {
                    case SpriteAnimationCurveType.KeyframeIndex:
                        {
                            int idx = (int)curve.curve.Evaluate(time);
                            if ((keyFrames.Length - 1) >= idx)
                            {
                                ret = keyFrames[idx];
                                spr = ret.sprite;
                                refClip = ret.refClip;
                            }
                        }
                        break;

                    case SpriteAnimationCurveType.PositionX:
                        position.x = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.PositionY:
                        position.y = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.Rotate:
                        rotation = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.ScaleX:
                        scale.x = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.ScaleY:
                        scale.y = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.ShearX:
                        shear.x = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.ShearY:
                        shear.y = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.ColorR:
                        color.r = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.ColorG:
                        color.g = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.ColorB:
                        color.b = curve.curve.Evaluate(time);
                        break;

                    case SpriteAnimationCurveType.ColorA:
                        color.a = curve.curve.Evaluate(time);
                        break;
                }
            }
            return ret;
        }

    }












}