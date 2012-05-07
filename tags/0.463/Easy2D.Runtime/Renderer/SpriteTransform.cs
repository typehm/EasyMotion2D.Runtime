using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{

    /// <summary>
    /// Position, rotation, scale, shear, color and layer of a sprite. 
    /// </summary>
    [System.Serializable]
    public class SpriteTransform
    {
        /// <summary>
        /// The sprite's position in world space.
        /// </summary>
        public Vector2 position = Vector2.zero;

        /// <summary>
        /// The sprite's rotation in world space.
        /// </summary>
        public float rotation = 0f;

        /// <summary>
        /// The sprite's scale in world space.
        /// </summary>
        public Vector2 scale = Vector2.one;

        /// <summary>
        /// The sprite's shear in world space.
        /// </summary>
        public Vector2 shear = Vector2.zero;

        /// <summary>
        /// The sprite's color.
        /// </summary>
        public Color color = Color.white;

        /// <summary>
        /// The sprite's sprite.
        /// </summary>
        public Sprite sprite
        {
            get
            {
                return _sprite;
            }

            set
            {
                _sprite = value;
                isSpriteValid = value != null;
            }
        }


        //internal direct use this member to save function calling time.
        //When you assigned this member, you should assign isSprireValid in same time
        //[HideInInspector]
        [SerializeField]
        internal Sprite _sprite = null;

        /// <summary>
        /// The layer of sprite.
        /// </summary>
        public int layer = 0;


        /// <summary>
        /// The SpriteAnimationComponent of SpriteTransform.
        /// </summary>
        public SpriteAnimationComponent component = null;

        /// <summary>
        /// Control the sprite visible in runtime
        /// </summary>
        public bool visible = true;


        /// <summary>
        /// Override Sprite can make SpriteRenderer use this sprite priority than SpriteTransform.sprite.
        /// </summary>
        public Sprite overrideSprite
        {
            get
            {
                return _override;
            }

            set
            {
                _override = value;
                _isOverride = value != null;
            }
        }

        internal Sprite _override = null;

        internal Vector3 worldPosition;

        /// <summary>
        /// Is thie SpriteTransform have a override sprite.
        /// </summary>
        public bool isOverride
        {
            get { return _isOverride; }
        }

        [HideInInspector]
        [SerializeField]
        internal bool _isOverride = false;

        /// <summary>
        /// The SpriteTransform's name.
        /// </summary>
        public string name = "transform";



        [HideInInspector]
        [SerializeField]
        //internal direct check this member to save function call time.
        internal bool isSpriteValid = false;



        internal SpritePrimitive primitive = null;

        internal int modifyMask = 0;


        /// <summary>
        /// Internal field.
        /// </summary>
        public int id = 0;


        /// <summary>
        /// Default constructor of SpriteTransform.
        /// </summary>
        public SpriteTransform()
        {
        }


        /// <summary>
        /// Copy constructor of SpriteTransform
        /// </summary>
        /// <param name="other">Copy source SpriteTransform.</param>
        public SpriteTransform(SpriteTransform other)
        {
            Clone(other);
        }

        /// <summary>
        /// Copy the field from other to self.
        /// </summary>
        /// <param name="other">Copy source SpriteTransform.</param>
        public void Clone(SpriteTransform other)
        {
            position = other.position;
            rotation = other.rotation;
            scale = other.scale;
            shear = other.shear;
            color = other.color;
            sprite = other.sprite;
            layer = other.layer;
            parent = other.parent;
            component = other.component;
            _override = other._override;
            _isOverride = other._isOverride;
        }

        internal void ResetTransform()
        {
            position.x = position.y = 0f;
            rotation = 0f;
            scale.x = scale.y = 1f;
            shear.x = shear.y = 0f;
            color.a = color.r = color.b = color.g = 1f;
            isSpriteValid = false;
        }

        internal void ZeroTransform()
        {
            position.x = position.y = 0f;
            rotation = 0f;
            scale.x = scale.y = 0f;
            shear.x = shear.y = 0f;
            color.a = color.r = color.b = color.g = 0f;
            isSpriteValid = false;
        }



        internal void Reset()
        {
            ResetTransform();
            layer = 0;
            parent = null;
            component = null;
        }


        /// <summary>
        /// Internal function.
        /// </summary>
        public Vector3[] GetTransformedPosition()
        {
            if ( primitive != null)
                return primitive.position;
            return null;
        }

        /// <summary>
        /// Transforms position from bone local space to gameObject local space
        /// Only available in PreUpdate hanlder in SpriteAnimation.
        /// </summary>
        public Vector2 TransformPoint(Vector2 position)
        {
            return GetLocalToWorldMatrix().MultiplyPoint( position);
        }

        /// <summary>
        /// Transforms position from gameObject local space to bone local space. The opposite of SpriteTransform.TransformPoint.
        /// Only available in PreUpdate hanlder in SpriteAnimation.
        /// </summary>
        public Vector2 InverseTransformPoint(Vector2 position)
        {
            return GetLocalToWorldMatrix().inverse.MultiplyPoint( position);
        }

        /// <summary>
        /// Rotates the transform so the forward vector points at target's current position.
        /// </summary>
        public void LookAt(Vector2 position)
        {
            LookAt(position, Vector2.right, false);
        }

        public void LookAt(Vector2 position, Vector2 direction)
        {
            LookAt(position, direction, false);
        }

        /// <summary>
        /// Rotates the transform so the forward vector points at target's current position.
        /// </summary>
        public void LookAt( Vector2 position, Vector2 direction, bool addtive )
        {
            Vector2 localPos = InverseTransformPoint( position ).normalized;
            Vector2 aimTo = Quaternion.Euler(0, 0, rotation) * direction;

            float a = Vector2.Angle( aimTo, localPos);
            a = direction.x > 0f ?
                ( localPos.y > aimTo.y ? a : -a ) :
                ( localPos.y > aimTo.y ? -a : a );

            rotation = addtive ? rotation + a : a;
        }



        /// <summary>
        /// Matrix that transforms a point from local space into gameobject local space
        /// Only available in PreUpdate hanlder in SpriteAnimation.
        /// </summary>
        public Matrix4x4 GetLocalToWorldMatrix()
        {
            Matrix4x4 mat = Matrix4x4.identity;
            SpriteTransform iter = this;
            
            while (iter != null)
            {
                mat = Matrix4x4.TRS(iter.position, Quaternion.Euler(0, 0, iter.rotation), new Vector3( iter.scale.x, iter.scale.y, 1f)) * mat;
                iter = iter.parent;
            }
            return mat;
        }

        /// <summary>
        /// The scale of the transform in gameobject local space.
        /// Only available in PreUpdate hanlder in SpriteAnimation.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetGlobalScale()
        {
            Vector2 ret = Vector2.one;
            SpriteTransform iter = this;

            while (iter != null)
            {
                ret = new Vector2(ret.x * iter.scale.x, ret.y * iter.scale.y);
                iter = iter.parent;
            }
            return ret;
        }

        /// <summary>
        /// The rotation of the transform in gameobject local space.\
        /// Only available in PreUpdate hanlder in SpriteAnimation.
        /// </summary>
        public float GetGlobalRotate()
        {
            float ret = 0;
            SpriteTransform iter = this;

            while (iter != null)
            {
                ret += iter.rotation;
                iter = iter.parent;
            }
            return ret;
        }


        internal int searchHash = 0;
        //internal SpriteAnimationClip lastRefClip = null;

        /// <summary>
        /// The parent of SpriteTransform.
        /// </summary>
        [System.NonSerialized]
        public SpriteTransform parent = null;

        [System.NonSerialized]
        internal SpriteTransform prve = null;

        [System.NonSerialized]
        internal SpriteTransform next = null;

        [System.NonSerialized]
        internal SpriteTransform firstChild = null;

        [System.NonSerialized]
        internal SpriteTransform lastChild = null;



        internal class StateComponentPair
        {
            public SpriteAnimationState state;
            public SpriteAnimationComponent component;
            public SpriteTransform applyTo;
            public float weight;
            public SpriteAnimationClip lastRefClip = null;
        };

        internal List<StateComponentPair> attachStateComponent = new List<StateComponentPair>();


        static List<StateComponentPair> pairCache = new List<StateComponentPair>();
        
        static StateComponentPair CreateComponentPair()
        {
            if (pairCache.Count == 0)
            {
                for (int i = 0; i < 32; i++)
                    pairCache.Add(new StateComponentPair());
            }


            int idx = pairCache.Count - 1;

            StateComponentPair ret = pairCache[idx];

            pairCache.RemoveAt(idx);
            return ret;
        }

        static void ReleaseComponentPair(StateComponentPair pair)
        {
            if (pair != null)
            {
                pair.applyTo = null;
                pair.component = null;
                pair.state = null;
                pair.lastRefClip = null;

                pairCache.Add(pair);
            }
        }




        internal void AttachState(SpriteAnimationState state, SpriteAnimationComponent component )
        {
            SpriteTransform.StateComponentPair pair = CreateComponentPair();
            pair.state = state;
            pair.component = component;
            pair.applyTo = this;
            pair.weight = 1f;

            attachStateComponent.Add(pair);
            state.referenceList.Add( pair );

            Refresh();
        }


        internal void DetachState(SpriteAnimationState state, SpriteAnimationComponent component )
        {
            foreach (SpriteTransform.StateComponentPair pair in attachStateComponent)
            {
                if (pair.state == state && pair.component == component)
                {
                    if (pair.lastRefClip != null)
                    {
                        string name = state.name + "_" + pair.component.fullPath + "_" + pair.lastRefClip.name;
                        SpriteAnimationState _state = state._animation[name];


                        if (_state != null)
                        {
                            state._animation.Stop(_state);
                        }

                        pair.lastRefClip = null;
                    }


                    attachStateComponent.Remove(pair);
                    state.referenceList.Remove(pair);

                    ReleaseComponentPair(pair);
                    break;
                }
            }

            Refresh();
        }


        internal void Refresh()
        {
            if (attachStateComponent.Count == 0)
                return;

            attachStateComponent.Sort(StateComponentPairComparerByLayer.comparer);

            CalcWeight();
        }


        struct LayerGroupInfo
        {
            public int startIndex;
            public int length;
            public int layer;
        }

        static LayerGroupInfo[] layerGroup = new LayerGroupInfo[32];
        static int layerGroupCount = 0;

        internal void CalcWeight()
        {
            attachStateComponent.Sort(StateComponentPairComparerByLayer.comparer);

            int lastLayer = attachStateComponent[0].state.layer;
            int lastLayerStart = 0;
            
            layerGroupCount = 0;

            int i = 0, e = 0;
            for ( i = 0, e = attachStateComponent.Count; i < e; i++)
            {
                StateComponentPair pair = attachStateComponent[i];

                if (lastLayer != pair.state.layer)
                {
                    int idx = layerGroupCount++;
                    layerGroup[idx].startIndex = lastLayerStart;
                    layerGroup[idx].length = i - lastLayerStart;
                    layerGroup[idx].layer = lastLayer;

                    lastLayerStart = i;
                    lastLayer = pair.state.layer;
                }
            }

            if (i - lastLayerStart > 0)
            {
                int idx = layerGroupCount++;
                layerGroup[idx].startIndex = lastLayerStart;
                layerGroup[idx].length = i - lastLayerStart;
                layerGroup[idx].layer = lastLayer;
            }


            float totalWeight = 1f;
            bool weightPassNext = layerGroupCount > 1;

            //string str = layerGroupCount.ToString() + "\n";

            for (i = 0; i < layerGroupCount; i++)
            {
                //str += "length:" + layerGroup[i].length + ", layer:" + layerGroup[i].layer.ToString() + "\n";
                totalWeight = CalcGroupWeight(layerGroup[i].startIndex, layerGroup[i].length, totalWeight, weightPassNext, layerGroup[i].layer);
            }

            //Debug.Log(str);
        }

        internal float CalcGroupWeight(int startIdx, int length, float totalWeight, bool weightPassNext, int layer )
        {
            if (totalWeight <= 0f)
            {
                for (int gi = startIdx; gi < startIdx + length; gi++)
                    attachStateComponent[gi].weight = 0f;
                return 0f;
            }



            float layerTotalWeight = 0f;
            for (int gi = startIdx; gi < startIdx + length; gi++)
            {
                SpriteTransform.StateComponentPair pair = attachStateComponent[gi];

                if (pair.state.blendMode == AnimationBlendMode.Blend)
                {
                    float w = pair.state.weight * pair.state.componentEnable[1 + pair.component.index];
                    layerTotalWeight += w;
                }
                else
                    pair.weight = pair.state.weight;
            }


            float layerWeight = layerTotalWeight >= 1f ? totalWeight :
                (weightPassNext ? layerTotalWeight : totalWeight);


            if (layerTotalWeight > 0f)
            {
                float iw = 1f / layerTotalWeight;
                for (int gi = startIdx; gi < startIdx + length; gi++)
                {
                    SpriteTransform.StateComponentPair pair = attachStateComponent[gi];
                    pair.weight = pair.state.weight * iw * layerWeight * pair.state.componentEnable[1 + pair.component.index];
                }
            }
            else
            {
                for (int gi = startIdx; gi < startIdx + length; gi++)
                {
                    SpriteTransform.StateComponentPair pair = attachStateComponent[gi];
                    if (pair.state.blendMode == AnimationBlendMode.Blend)
                        pair.weight = 0f;
                }
            }


            return totalWeight - layerWeight;
        }
    }



    /// <summary>
    /// Internal class. You do not need to use this.
    /// </summary>
    internal class StateComponentPairComparerByLayer : IComparer<SpriteTransform.StateComponentPair>
    {
        public int Compare(SpriteTransform.StateComponentPair lhs, SpriteTransform.StateComponentPair rhs)
        {
            return rhs.state.layer - lhs.state.layer;
        }

        public static StateComponentPairComparerByLayer comparer = new StateComponentPairComparerByLayer();
    }

}