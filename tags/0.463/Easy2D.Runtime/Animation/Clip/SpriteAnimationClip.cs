using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{

    







    /// <summary>
    /// Stores <see cref="SpriteAnimationComponent">animation component</see> based animations.
    /// </summary>
    public class SpriteAnimationClip : ScriptableObject
    {
        public static float currentFormatVersion = 0.451f;



        public float dataFormatVersion = 0.0f;



        /// <summary>
        /// Animation length in seconds (Read Only)
        /// </summary>
        public float length
        {
            get
            {
                return _length;
            }
        }

        [SerializeField]
        private float _length = 0f;



        /// <summary>
        /// Sets the default wrap mode used in the animation state.
        /// </summary>
        public WrapMode wrapMode = WrapMode.Once;



        /// <summary>
        /// Frame rate at which keyframes are sampled (Read Only)
        /// </summary>
        public int frameRate = 20;


        /// <summary>
        /// The root <see cref="SpriteAnimationComponent">component</see> in animation
        /// </summary>
        public SpriteAnimationComponent root = new SpriteAnimationComponent("root");
        


        public SpriteAnimationEvent[] events = new SpriteAnimationEvent[] { };



        public SpriteAnimationComponent[] subComponents = new SpriteAnimationComponent[1];




        [SerializeField]
        internal AnimationLinearCurve playingCurve = new AnimationLinearCurve(0f,0f,0f,0f);



        [HideInInspector]
        [SerializeField]
        internal float tick = 0f;



        [HideInInspector]
        [SerializeField]
        internal int maxFrameIndex = 0;



        //internal bool isInit = false;



        void OnEnable()
        {
            

            Init( false );
        }



        internal bool Upgrade()
        {
            if (dataFormatVersion < 0.43f)
            {
                try
                {
                    if (subComponents.Length == 0)
                    {
                        Debug.LogWarning("clip " + name + " need upgrade!");

                        List<SpriteAnimationComponent> components = new List<SpriteAnimationComponent>();
                        upgradeComponent(root, components);

                        subComponents = components.ToArray();
                    }
                    dataFormatVersion = 0.43f;
                }
                catch ( System.Exception e)
                {
                    Debug.LogError( e.Message + "\n" + "clip " + name + " upgrade to version " + currentFormatVersion + " faild!");
                    return false;
                }
            }


            if (dataFormatVersion == 0.43f || dataFormatVersion == 0.44f || dataFormatVersion == 0.45f)
            {
                try
                {
                    dataFormatVersion = currentFormatVersion;

                    foreach (SpriteAnimationComponent component in subComponents)
                    {
                        if (component.clip != null)
                        {
                            component._fullPathHash = EasyMotion2DUtility.GetHashCode(component._fullPath);

                            component.CalcCurve(null);
                        }
                    }

                    Debug.Log("clip " + name + " upgrade to version " + currentFormatVersion + " success!");
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message + "\n" + "clip " + name + " upgrade to version " + currentFormatVersion + " faild!");
                    return false;
                }
            }

            try
            {
                Init( !Application.isPlaying );
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message + "\n" + "clip " + name + " upgrade to version " + currentFormatVersion + " faild!");
                return false;
            }
            return true;
        }



        internal void upgradeComponent(SpriteAnimationComponent component, List<SpriteAnimationComponent> components)
        {
            if ( component != root && !components.Contains(component))
            {
                components.Add(component);
                component.index = components.Count - 1;
                component._fullPathHash = EasyMotion2DUtility.GetHashCode(component._fullPath);
            }

            List<int> cIdxList = new List<int>();

            foreach (SpriteAnimationComponent child in component.childs)
            {
                upgradeComponent(child, components);

                child.parent = component;
                cIdxList.Add(child.index);
            }

            component.children = cIdxList.ToArray();
        }



        /// <summary>
        /// Initialize clip internal state.
        /// </summary>
        /// <param name="calcComp">Is need re-init sub compoent curves?</param>
        public void Init(bool calcComp)
        {
            root.clip = this;
            root.index = -1;
            root._fullPath = "/root";
            root._fullPathHash = EasyMotion2DUtility.GetHashCode(root._fullPath);



            tick = 1f / frameRate;

            maxFrameIndex = getMaxComponentIndex( root);
            _length = maxFrameIndex * tick;



            CalcCurve(calcComp);
            CalcEvents();



            float l = length;
            float eIdx = (l / tick);
            playingCurve.SetTime(0, 0, _length, Mathf.Floor(eIdx) + 0.051f);
        }




        internal void CalcCurve(bool calcComp)
        {
            calcCurve(root, calcComp);
            _length = getMaxComponentIndex(root) * (1f / frameRate);
        }



        internal void CalcEvents()
        {
            float tick = 1f / frameRate;
            foreach (SpriteAnimationEvent evt in events)
                evt.time = evt.frameIndex * tick;
        }




        void calcCurve(SpriteAnimationComponent component, bool calcComp )
        {
            if (calcComp)
                component.CalcCurve( null );

            component._fullPath = component.GetFullPath();
            component._fullPathHash = EasyMotion2DUtility.GetHashCode( component._fullPath );

            foreach (int idx in component.children)
            {
                SpriteAnimationComponent comp = subComponents[idx];
                calcCurve(comp, calcComp);
            }
        }




        internal int getMaxComponentIndex(SpriteAnimationComponent component)
        {
            int ret = component._maxIndex = component.GetMaxIndex();

            foreach (int idx in component.children)
            {
                SpriteAnimationComponent comp = subComponents[idx];
                int r = getMaxComponentIndex(comp);
                if (ret < r)
                    ret = r;
            }

            return ret;
        }




        internal SpriteAnimationComponent GetComponentById(int id)
        {
            if (root.GetHashCode() == id)
                return root;

            for (int i = 0, e = subComponents.Length; i < e; i++)
            {
                if (subComponents[i].GetHashCode() == id)
                    return subComponents[i];
            }

            return null;
        }




        internal SpriteAnimationComponent GetComponentByPathHash(int hash)
        {
            if (root._fullPathHash == hash)
                return root;

            for (int i = 0, e = subComponents.Length; i < e; i++)
            {
                if (subComponents[i]._fullPathHash == hash)
                    return subComponents[i];
            }

            return null;
        }





        internal void AddChild(SpriteAnimationComponent comp)
        {
            List<SpriteAnimationComponent> tmp = new List<SpriteAnimationComponent>(subComponents);

            if (!tmp.Contains(comp))
            {
                tmp.Add(comp);
                subComponents = tmp.ToArray();

                comp.index = tmp.Count - 1;
            }
        }




        internal void RemoveChild(SpriteAnimationComponent comp)
        {
            int idx = comp.index;

            if (idx == -2)
                return;

            int len = subComponents.Length;
            int lastIdx = len - 1;

            //if only mine in children list, do not need to swap
            if (lastIdx != idx)
            {
                int oldIdx = subComponents[lastIdx].index;

                SpriteAnimationComponent tmpComp = subComponents[oldIdx];
                
                subComponents[idx] = tmpComp;                
                tmpComp.index = idx;

                

                //redirect children to new index
                foreach (int cIdx in tmpComp.children)
                {
                    if ( cIdx <= lastIdx )
                        subComponents[cIdx].parentIndex = idx;
                }


                

                //remove from parent
                if (tmpComp.parent != null)
                {
                    List<int> parentChildrenIdx = new List<int>(subComponents[idx].parent.children);

                    int i = parentChildrenIdx.IndexOf(oldIdx);
                    if ( i >= 0)
                        parentChildrenIdx[i] = idx;

                    subComponents[idx].parent.children = parentChildrenIdx.ToArray();
                }


                idx = subComponents.Length - 1;
            }

            List<SpriteAnimationComponent> tmp = new List<SpriteAnimationComponent>(subComponents);
            tmp.RemoveAt(idx);

            subComponents = tmp.ToArray();
            comp.index = -2;

        }


        /// <summary>
        /// Clone a SpriteAnimationClip.
        /// </summary>
        /// <param name="source">Clip source.</param>
        /// <param name="init">Is need init the clone instance.</param>
        /// <returns>The clone instance.</returns>
        public static SpriteAnimationClip Duplicate(SpriteAnimationClip source, bool init)
        {
            SpriteAnimationClip ret = ScriptableObject.CreateInstance<SpriteAnimationClip>();

            ret.name = source.name;
            ret.dataFormatVersion = source.dataFormatVersion;

            ret.frameRate = source.frameRate;
            ret.wrapMode = source.wrapMode;

            ret.events = source.events;

            ret.root = SpriteAnimationComponent.Duplicate(source.root);
            ret.root.clip = ret;

            ret.subComponents = new SpriteAnimationComponent[ source.subComponents.Length ];
            foreach (SpriteAnimationComponent comp in source.subComponents)
            {
                SpriteAnimationComponent tmpComp = SpriteAnimationComponent.Duplicate(comp);
                
                tmpComp.clip = ret;
                ret.subComponents[ tmpComp.index ] = tmpComp;
            }

            if ( init )
                ret.Init(false);

            return ret;
        }
    }


}