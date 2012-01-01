using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{



    /// <summary>
    /// Sprite current AABB in world space.
    /// </summary>
    [System.Serializable]
    public struct SpriteAABB
    {
        internal float position_minx;
        internal float position_miny;
        internal float position_maxx;
        internal float position_maxy;

        /// <summary>
        /// Rectangle of AABB.
        /// </summary>
        public Rect position
        {
            get
            {
                return new Rect(position_minx, position_miny, position_maxx - position_minx, position_maxy - position_miny);
            }
        }

        /// <summary>
        /// Center position of AABB.
        /// </summary>
        public Vector2 center;
    }






    public enum SpriteRendererAnchor
    {
        None = 0,

        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight,

        Custom,
    }







    /// <summary>
    /// Where the SpriteRenderer to call Apply().
    /// </summary>
    public enum SpriteRendererUpdateMode
    {
        /// <summary>
        /// Do not call Aplly() automatically. You should call Apply in you code.
        /// </summary>
        None = 0,

        /// <summary>
        /// Automatically call Apply() in Update.
        /// </summary>
        Update,

        /// <summary>
        /// Automatically call Apply() in LateUpdate.
        /// </summary>
        LateUpdate,

        /// <summary>
        /// Automatically call Apply() in FixedUpdate.
        /// </summary>
        FixedUpdate,
    }










    /// <summary>
    /// The sprite rendering plane.
    /// </summary>
    public enum SpritePlane
    {
        PlaneXY = 0,
        PlaneXZ,
        PlaneZY,
    }










    /// <summary>
    /// Internal class.
    /// </summary>
    [System.Serializable]
    public class SpriteTransformCullingGroup
    {
        public string name = "SpriteTransformCullingGroup";
        public Vector2 center = new Vector2(100, 100);
        public Vector2 size = new Vector2(100, 100);
        public float radius = 0f;
        public int[] sprIdx = new int[]{};
        public bool lastVisible = false;

        [System.NonSerialized]
        public bool isInit = false;
    }













    /// <summary>
    /// Render attached sprites.
    /// </summary>
    [ExecuteInEditMode]    
    public class SpriteRenderer : MonoBehaviour, SpriteRenderable
    {
        private SpriteRenderMode _renderMode = SpriteRenderMode.AlphaBlend;




        [HideInInspector]
        [SerializeField]
        protected int primitiveCount = 0;



        [HideInInspector]
        [SerializeField]
        protected int activePrimitiveCount = 0;


        [HideInInspector]
        [SerializeField]
        internal IntHolder _activePrimitiveCount = new IntHolder(0);



        [HideInInspector]
        [SerializeField]
        internal SpritePrimitive[] primitives = new SpritePrimitive[1] { new SpritePrimitive(1) };



        [HideInInspector]
        [SerializeField]
        protected SpriteTransform[] spriteTransforms = new SpriteTransform[1] { new SpriteTransform() };

        internal SpritePrimitiveGroup primitiveGroup = new SpritePrimitiveGroup();


        [SerializeField]
        protected bool _visible = true;



        protected float radius = 0f;


        /// <summary>
        /// The sprite's <see cref="SpriteRenderModeSetting">render mode</see>.
        /// </summary>
        public SpriteRenderModeSetting renderMode = null;
        


        /// <summary>
        /// The sprite's <see cref="Color">color</see>.
        /// </summary>
        public Color color = Color.white;




        /// <summary>
        /// The sprite's <see cref="SpriteRendererUpdateMode">update mode</see>
        /// </summary>
        public SpriteRendererUpdateMode updateMode = SpriteRendererUpdateMode.None;





        /// <summary>
        /// The sprite's depth in layer.
        /// </summary>
        public int depth = 0;


        /// <summary>
        /// Is apply the real depth to depth axis in transform.
        /// </summary>
        public bool applyDepthToTransform = false;

        /// <summary>
        /// Renderer's AABB bounding box in world space.
        /// </summary>
        public SpriteAABB boundingAABB;


        /// <summary>
        /// The sprite rendering plane.
        /// </summary>
        public SpritePlane plane = SpritePlane.PlaneXY;

        /// <summary>
        /// Sizing factor.
        /// </summary>
        public float scaleFactor = 1f;


        /// <summary>
        /// LocalScale to instead localScale of transform.
        /// Recommand use this, because it will not break the draw batch when you using the SpriteMeshRenderer.
        /// </summary>
        public Vector2 localScale = Vector2.one;


        public SpriteRendererAnchor anchor = SpriteRendererAnchor.None;
        public Vector2 pivot = Vector2.zero;

        [HideInInspector]
        [SerializeField]
        internal bool commitToSceneManager = true;


        protected SpriteRenderMode lastRM = SpriteRenderMode.None;
        protected Vector3 transformedVec = Vector3.zero;
        static protected Vector3 vecOne = Vector3.one;

        protected Color __colorLT = Color.white;
        protected Color __colorRT = Color.white;
        protected Color __colorLB = Color.white;
        protected Color __colorRB = Color.white;



        protected Transform _transform;


        private bool isAttachAnimation = false;

        private bool isUpdated = false;

        protected uint gameObjectLayer = 0;


        protected bool behaviourEnabled = false;
        protected bool isInit = false;


        /// <summary>
        /// Use for show debug information.
        /// </summary>
        public bool showBounding = false;

        /// <summary>
        /// Use for show debug information.
        /// </summary>
        public bool showAttachSpriteBounding = false;

        /// <summary>
        /// Use for show debug information.
        /// </summary>
        public bool showCullingGroupBounding = false;


        /// <summary>
        /// Internal field.
        /// </summary>
        [HideInInspector]
        public SpriteTransformCullingGroup[] cullingGroup = new SpriteTransformCullingGroup[]{};
        private bool isCullingGroup = false;


        internal bool applyLayerToZ = true;

        /// <summary>
        /// Set this renderer visible in any camera.
        /// </summary>
        public bool visible
        {
            get { return _visible; }
            set
            {
                _visible = value && enabled;
                primitiveGroup.visible = _visible && primitiveGroup.visible;
            }
        }


        internal class OverrideSprite
        {
            public int pathHash;
            public Sprite sprite;
        }
        internal List<OverrideSprite> overrideList = new List<OverrideSprite>();



        /// <summary>
        /// Get a attached <see cref="Sprite"> Sprite</see> in renderer by index.
        /// </summary>
        public Sprite GetAttachSprite(int idx)
        {
            return spriteTransforms[idx]._sprite;
        }




        /// <summary>
        /// Get how many sprites attached.
        /// </summary>
        public int GetAttachSpriteCount()
        {
            return activePrimitiveCount;
        }




        private void resetRenderer()
        {
            gameObjectLayer = (uint)gameObject.layer;


            _activePrimitiveCount.value = activePrimitiveCount;

            primitiveGroup.count = _activePrimitiveCount;
            primitiveGroup.owner = this.gameObject;
            primitiveGroup.visible = true;
            primitiveGroup.renderable = this;
            primitiveGroup.layer = (int)gameObjectLayer;


            Resize(activePrimitiveCount != 0 ? activePrimitiveCount : 1);
            isInit = true;

            if ( commitToSceneManager )
                primitiveGroup.AddToLayer((int)gameObjectLayer);

            FixTransforms();
        }


        int instanceID = 0;

        /// <summary>
        /// Initialize the SpriteRenderer. You do not need call it directly.
        /// </summary>
        public void Init()
        {
            primitiveCount = primitives.Length;
            _transform = transform;
            instanceID = gameObject.GetInstanceID();


            if ( Application.isPlaying && !isInit)
                resetRenderer();
            else if (!Application.isPlaying)
                resetRenderer();

            isCullingGroup = cullingGroup != null && cullingGroup.Length > 0;


            for (int i = 0; i < primitives.Length; i++)
            {
                primitives[i].ownerID = instanceID;
                spriteTransforms[i].primitive = primitives[i];
            }


            Apply();
        }




        void FixTransforms()
        {
            for (int i = 0, e = activePrimitiveCount; i < e; i++)
                spriteTransforms[i].sprite = spriteTransforms[i]._sprite;
        }





        /// <summary>
        /// Clear all attached sprites.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < activePrimitiveCount; i++)
            {
                SpriteTransform tran = spriteTransforms[i];
                tran.parent = null;
                tran.firstChild = null;
                tran.lastChild = null;
                tran.next = null;
                tran.prve = null;
                tran.lastRefClip = null;
                tran.overrideSprite = null;
                tran.component = null;
                tran.overrideHash = 0;
            }

            _activePrimitiveCount.value = activePrimitiveCount = 0;
            overrideList.Clear();
        }





        /// <summary>
        /// Set the max <see cref="Sprite">Sprite</see>'s count can be attach to renderer.
        /// </summary>
        /// <param name="maxSprites">How many sprite the renderer can be attach.</param>
        public void Resize( int maxSprites )
        {
            int layer = gameObject.layer;
            int oldLen = primitives.Length;


            if (oldLen < maxSprites)
            {
                System.Array.Resize<SpritePrimitive>(ref primitives, maxSprites * 2);
                System.Array.Resize<SpriteTransform>(ref spriteTransforms, maxSprites * 2);

                int ownerID = instanceID;
                for (int i = oldLen; i < primitives.Length; i++)
                {
                    SpritePrimitive pri = new SpritePrimitive(1);
                    pri.ownerID = ownerID;
                    primitives[i] = pri;

                    spriteTransforms[i] = new SpriteTransform();
                    spriteTransforms[i].ResetTransform();
                }
            }

            primitiveCount = primitives.Length;

            primitiveGroup.primitives = primitives;
        }






        /// <summary>
        /// Attach a <see cref="Sprite"> Sprite</see> to renderer.
        /// </summary>
        /// <param name="sprite">Attach <see cref="Sprite"> Sprite</see> to renderer.</param>
        /// <returns>Attached <see cref="Sprite"> Sprite</see>'s index in renderer.</returns>
        public int AttachSprite(Sprite sprite)
        {
            SpriteTransform ret = AttachSpriteComponent(null);
            ret.sprite = sprite;
            return ret.id;
        }


        internal SpriteTransform AttachSpriteComponent(SpriteAnimationComponent component)
        {
            int idx = activePrimitiveCount++;

            SpriteTransform ret = spriteTransforms[idx];

            ret.id = idx;
            ret.primitive = primitives[idx];
            ret.ResetTransform();

            ret.component = component;
            if ( component != null )
                ret.name = component.name;

            if (component != null && overrideList.Count > 0)
            {
                for (int i = 0; i < overrideList.Count; i++)
                {
                    if (overrideList[i].pathHash == component._fullPathHash)
                    {
                        ret.overrideSprite = overrideList[i].sprite;
                        overrideList.RemoveAt(i);
                        break;
                    }
                }
            }

            if (activePrimitiveCount == primitiveCount)
                Resize(activePrimitiveCount + 1);

            _activePrimitiveCount.value = activePrimitiveCount;

            return ret;
        }



        internal void DetachSprite( int idx )
        {
            int lastIdx = activePrimitiveCount - 1;

            SpriteTransform tmpSprTransform = spriteTransforms[idx];

            if (tmpSprTransform.overrideSprite != null && tmpSprTransform.component != null)
            {
                OverrideSprite os = new OverrideSprite();

                os.pathHash = tmpSprTransform.component._fullPathHash;
                os.sprite = tmpSprTransform.overrideSprite;
                overrideList.Add(os);
            }


            tmpSprTransform.parent = null;
            tmpSprTransform.firstChild = null;
            tmpSprTransform.lastChild = null;
            tmpSprTransform.next = null;
            tmpSprTransform.prve = null;
            tmpSprTransform.lastRefClip = null;
            tmpSprTransform.component = null;
            tmpSprTransform.overrideHash = 0;
            tmpSprTransform.overrideSprite = null;



            if (lastIdx != idx)
            {
                SpritePrimitive tmpPri = primitives[idx];
                
                spriteTransforms[idx] = spriteTransforms[lastIdx];
                spriteTransforms[idx].id = idx;
                primitives[idx] = primitives[lastIdx];
                
                tmpSprTransform.id = lastIdx;                

                spriteTransforms[lastIdx] = tmpSprTransform;
                primitives[lastIdx] = tmpPri;
            }


            activePrimitiveCount--;
            _activePrimitiveCount.value = activePrimitiveCount;

        }










        /// <summary>
        /// Get an attached <see cref="Sprite"> Sprite</see>'s <see cref="SpriteTransform">SpriteTransform</see> at index.
        /// </summary>
        /// <param name="index">Sprite's index in renderer.</param>
        /// <returns>The <see cref="Sprite"> Sprite</see>'s <see cref="SpriteTransform">SpriteTransform</see>.</returns>
        public SpriteTransform GetSpriteTransform(int index)
        {
            return spriteTransforms[index];
        }







        /// <summary>
        /// Get an attached <see cref="Sprite"> Sprite</see>'s <see cref="SpriteTransform">SpriteTransform</see> with name. Usually use to find a animation component.
        /// </summary>
        /// <param name="name">Sprite's name in renderer. If name contains '/' means find sprite by fullpath in animation clip, else will find sprite by name in component of clip or the sprite attached in renderer by your code.</param>
        /// <returns>The <see cref="Sprite"> Sprite</see>'s <see cref="SpriteTransform">SpriteTransform</see>.</returns>
        public SpriteTransform GetSpriteTransform(string name)
        {
            bool isPath = name.Contains("/"); 

            for (int i = 0, e = activePrimitiveCount; i < e; i++)
            {
                if (isPath)
                {
                    if (spriteTransforms[i].component != null && spriteTransforms[i].component.fullPath == name)
                        return spriteTransforms[i];
                }
                else
                {
                    if (spriteTransforms[i].name == name)
                        return spriteTransforms[i];
                }
            }

            return null;
        }



        internal SpriteTransform GetSpriteTransformByFullPathHash(int hash)
        {
            for (int i = 0, e = activePrimitiveCount; i < e; i++)
            {
                if (spriteTransforms[i].overrideHash != 0 && spriteTransforms[i].overrideHash == hash)
                    return spriteTransforms[i];

                if (spriteTransforms[i].component != null && spriteTransforms[i].component._fullPathHash == hash)
                    return spriteTransforms[i];
            }

            return null;
        }


        internal bool isApply = false;



        /// <summary>
        /// Apply the Transform's data to renderer.
        /// </summary>
        public void Apply()
        {
            if (renderMode != null)
                _renderMode = renderMode.renderMode;
            else
                return;


            //Vector3 tmp = transform.TransformPoint(vecOne);
            //if (tmp == transformedVec && _renderMode == lastRM)
            //    return;

            //transformedVec = tmp;
            //lastRM = _renderMode;

            primitiveGroup.visible = _visible && behaviourEnabled;

            if (!_visible || !behaviourEnabled)
                return;


            depth = Mathf.Clamp(depth, 0, 511);

            int layer = gameObject.layer; ;
            gameObjectLayer = (uint)layer;
            Vector3 tranPos = _transform.position;


            {
                Vector3 parentScale = Vector3.one;
                Transform p = _transform.parent;

                while (p != null)
                {
                    parentScale = Vector3.Scale( parentScale, p.localScale);
                    p = p.parent;
                }

                Vector2 scale = commitToSceneManager ? Vector2.Scale(parentScale, _transform.localScale) : Vector2.one;

                //
                scale *= scaleFactor;
                scale = Vector3.Scale(scale, localScale);


                bool firstTransform = true;

                if (plane == SpritePlane.PlaneXY)
                {
                    boundingAABB.position_maxx = boundingAABB.position_minx = tranPos.x;
                    boundingAABB.position_maxy = boundingAABB.position_miny = tranPos.y;

                    Vector2 targetOrig = commitToSceneManager ? new Vector2(tranPos.x, tranPos.y) : Vector2.zero;



                    float rz = commitToSceneManager ? _transform.eulerAngles.z : 0f;

                    for (int i = 0; i < activePrimitiveCount; i++)
                    {
                        spriteTransforms[i].id = i;
                        firstTransform = SetupSpritePrimitiveXY(primitives[i], spriteTransforms[i], targetOrig, rz, scale, layer, firstTransform);
                    }

                    if (applyDepthToTransform)
                    {
                        _transform.position = new Vector3(tranPos.x, tranPos.y, layer * 64f + depth * 0.125f);
                    }
                }
                else if (plane == SpritePlane.PlaneZY)
                {
                    boundingAABB.position_maxx = boundingAABB.position_minx = tranPos.z;
                    boundingAABB.position_maxy = boundingAABB.position_miny = tranPos.y;

                    Vector2 targetOrig = commitToSceneManager ? new Vector2(tranPos.z, tranPos.y) : Vector2.zero;
                    


                    float rx = commitToSceneManager ? _transform.eulerAngles.x : 0f;


                    for (int i = 0; i < activePrimitiveCount; i++)
                    {
                        spriteTransforms[i].id = i;
                        firstTransform = SetupSpritePrimitiveZY(primitives[i], spriteTransforms[i], targetOrig, rx, scale, layer, firstTransform);
                    }

                    if (applyDepthToTransform)
                    {
                        _transform.position = new Vector3(-layer * 64f + depth * 0.125f, tranPos.y, tranPos.z);
                    }
                }
                else
                {
                    boundingAABB.position_maxx = boundingAABB.position_minx = tranPos.x;
                    boundingAABB.position_maxy = boundingAABB.position_miny = tranPos.z;

                    Vector2 targetOrig = commitToSceneManager ? new Vector2(tranPos.x, tranPos.z) : Vector2.zero;
                    


                    float ry = commitToSceneManager ? _transform.eulerAngles.y : 0f;


                    for (int i = 0; i < activePrimitiveCount; i++)
                    {
                        spriteTransforms[i].id = i;
                        firstTransform = SetupSpritePrimitiveXZ(primitives[i], spriteTransforms[i], targetOrig, ry, scale, layer, firstTransform);
                    }

                    if (applyDepthToTransform)
                    {
                        _transform.position = new Vector3(tranPos.x, -layer * 64f + depth * 0.125f, tranPos.z);
                    }
                }
            }




            if (anchor != SpriteRendererAnchor.None)
            {
                Vector2 _pos = vecOne;

                switch (anchor)
                {
                    case SpriteRendererAnchor.Custom:
                        _pos.x = boundingAABB.position_minx + pivot.x;
                        _pos.y = boundingAABB.position_maxy + pivot.y;
                        break;

                    case SpriteRendererAnchor.TopLeft:
                        _pos.x = boundingAABB.position_minx;
                        _pos.y = boundingAABB.position_maxy;
                        break;
                    case SpriteRendererAnchor.TopCenter:
                        _pos.x = (boundingAABB.position_maxx + boundingAABB.position_minx) * 0.5f;
                        _pos.y = boundingAABB.position_maxy;
                        break;
                    case SpriteRendererAnchor.TopRight:
                        _pos.x = boundingAABB.position_maxx;
                        _pos.y = boundingAABB.position_maxy;
                        break;

                    case SpriteRendererAnchor.MiddleLeft:
                        _pos.x = boundingAABB.position_minx;
                        _pos.y = (boundingAABB.position_maxy + boundingAABB.position_miny) * 0.5f;
                        break;
                    case SpriteRendererAnchor.MiddleCenter:
                        _pos.x = (boundingAABB.position_maxx + boundingAABB.position_minx) * 0.5f;
                        _pos.y = (boundingAABB.position_maxy + boundingAABB.position_miny) * 0.5f;
                        break;
                    case SpriteRendererAnchor.MiddleRight:
                        _pos.x = boundingAABB.position_maxx;
                        _pos.y = (boundingAABB.position_maxy + boundingAABB.position_miny) * 0.5f;
                        break;

                    case SpriteRendererAnchor.BottomLeft:
                        _pos.x = boundingAABB.position_minx;
                        _pos.y = boundingAABB.position_miny;
                        break;
                    case SpriteRendererAnchor.BottomCenter:
                        _pos.x = (boundingAABB.position_maxx + boundingAABB.position_minx) * 0.5f;
                        _pos.y = boundingAABB.position_miny;
                        break;
                    case SpriteRendererAnchor.BottomRight:
                        _pos.x = boundingAABB.position_maxx;
                        _pos.y = boundingAABB.position_miny;
                        break;
                }

                if (plane == SpritePlane.PlaneXY)
                {
                    anchorOffset.x = tranPos.x - _pos.x;
                    anchorOffset.y = tranPos.y - _pos.y;
                    anchorOffset.z = 0f;
                }
                else if (plane == SpritePlane.PlaneXZ)
                {
                    anchorOffset.x = tranPos.x - _pos.x;
                    anchorOffset.y = 0f;
                    anchorOffset.z = tranPos.y - _pos.y;
                }
                else if (plane == SpritePlane.PlaneZY)
                {
                    anchorOffset.x = 0f;
                    anchorOffset.y = tranPos.y - _pos.y;
                    anchorOffset.z = tranPos.x - _pos.x;
                }

                boundingAABB.position_minx += (tranPos.x - _pos.x);
                boundingAABB.position_maxx += (tranPos.x - _pos.x);
                boundingAABB.position_miny += (tranPos.y - _pos.y);
                boundingAABB.position_maxy += (tranPos.y - _pos.y);

                ApplyAnchor(anchorOffset);
            }



            float w = (boundingAABB.position_maxx - boundingAABB.position_minx) * 0.5f;
            float h = (boundingAABB.position_maxy - boundingAABB.position_miny) * 0.5f;

            radius = Mathf.Sqrt(w * w + h * h);
            boundingAABB.center.x = (boundingAABB.position_maxx + boundingAABB.position_minx) * 0.5f;
            boundingAABB.center.y = (boundingAABB.position_maxy + boundingAABB.position_miny) * 0.5f;


            isUpdated = true;
            isApply = true;
        }

        Vector3 anchorOffset = Vector3.zero;



        static internal ulong calcSortKey(ulong texId, SpritePrimitive pri, uint gameObjectLayer, uint depthInLayer, uint subLayer, SpriteRenderModeSetting renderMode, bool applyLayerToZ)
        {
            ulong ret = 0;
            ulong idx = (ulong)renderMode.renderMode & 0xf;

            uint layer = applyLayerToZ ? 
                ( (gameObjectLayer <= 0xf ? 0 : gameObjectLayer) & 0xf) : 0;

            if (renderMode.sortByDepth)
            {
                uint z = (uint)depthInLayer & 0x1ff;

                float _z = (layer * 64) +
                    ((float)z * 0.125f) +
                    (float)subLayer * 0.00390625f;

                pri.z = _z;

                uint oid = ((uint)pri.ownerID) & 0x7fffffff;
                                                                        // usage                 start    bits
                ret = (0x1ul << 63) |                                   // isSemiTransparent     63       1
                    ((0xful - layer) << 59) |                           // gameObjectLayer       58 - 62  4
                    ((0x1fful - z) << 49) |                             // transform z          48 - 57  9
                    ( oid << 15) |                     // group by owner       15 - 47  32
                    ((0x1ful - subLayer) << 10) |                        // sublayer             11 - 14  3
                    (idx << 6) |                                        // rendermode           7 -  10  4
                    (texId)                                             // texture              0  -  6  7     
                ;
            }
            else
            {
                uint z = (uint)depthInLayer & 0x1ff;

                float _z = (layer * 64) +
                    ((float)z * 0.125f) +
                    (float)subLayer * 0.0078125f;

                pri.z = _z;

                ret = (idx << 59) |
                    (texId << 52);

            }
            return ret;
        }



        bool SetupSpritePrimitiveXY(SpritePrimitive pri, SpriteTransform sTransform, Vector2 parentPosition, float parentRotation, Vector2 parentScale, int layer, bool isFirst)
        {
            sTransform.primitive = pri;


            if (!sTransform.visible || (!sTransform.isSpriteValid && !sTransform.isOverride))
            {
                pri.visible = false;
                return isFirst;
            }



            Sprite spr = sTransform.isOverride ? sTransform._override : sTransform._sprite;


            layer = (layer - 0xf <= 0 ? 0 : layer) & 0xf;

            pri.compareKey = calcSortKey((ulong)spr.texID, pri, gameObjectLayer, (uint)depth, (uint)sTransform.layer, renderMode, applyLayerToZ);

            //TRS
            {
                float r = Mathf.Deg2Rad * (sTransform.rotation);
                float sin = Mathf.Sin(r);
                float cos = Mathf.Cos(r);

                float pr = Mathf.Deg2Rad * (parentRotation);
                float psin = Mathf.Sin(pr);
                float pcos = Mathf.Cos(pr);


                float sx = 0;
                float sy = 0;
                float tx = 0;
                float ty = 0;
                float psx = 0;
                float psy = 0;
                float px = 0;
                float py = 0;


                for (int i = 0, e = spr.vertices.Length; i < e; i++)
                {
                    sx = (spr.vertices[i].x + (spr.vertices[i].y * -sTransform.shear.y)) * sTransform.scale.x;
                    sy = (spr.vertices[i].y + (spr.vertices[i].x * -sTransform.shear.x)) * sTransform.scale.y;

                    tx = (sx * cos - sy * sin);
                    ty = (sx * sin + sy * cos);

                    psx = (sTransform.position.x + tx) * parentScale.x;
                    psy = (sTransform.position.y + ty) * parentScale.y;

                    tx = parentPosition.x + (psx * pcos - psy * psin);
                    ty = parentPosition.y + (psx * psin + psy * pcos);

                    pri.position[i].x = tx;
                    pri.position[i].y = ty;
                    pri.position[i].z = pri.z;

                    //Vector2 
                    if (isFirst)
                    {
                        boundingAABB.position_minx = tx;
                        boundingAABB.position_miny = ty;
                        boundingAABB.position_maxx = tx;
                        boundingAABB.position_maxy = ty;
                        isFirst = false;
                    }
                    else
                    {
                        boundingAABB.position_minx = tx < boundingAABB.position_minx ? tx : boundingAABB.position_minx;
                        boundingAABB.position_miny = ty < boundingAABB.position_miny ? ty : boundingAABB.position_miny;
                        boundingAABB.position_maxx = tx > boundingAABB.position_maxx ? tx : boundingAABB.position_maxx;
                        boundingAABB.position_maxy = ty > boundingAABB.position_maxy ? ty : boundingAABB.position_maxy;
                    }
                }
            }


            {
                float tmpColorR = color.r * sTransform.color.r;
                float tmpColorG = color.g * sTransform.color.g;
                float tmpColorB = color.b * sTransform.color.b;
                float tmpColorA = color.a * sTransform.color.a;

                pri.color[3].a = tmpColorA * __colorLB.a;
                pri.color[3].r = tmpColorR * __colorLB.r;
                pri.color[3].g = tmpColorG * __colorLB.g;
                pri.color[3].b = tmpColorB * __colorLB.b;

                pri.color[2].a = tmpColorA * __colorRB.a;
                pri.color[2].r = tmpColorR * __colorRB.r;
                pri.color[2].g = tmpColorG * __colorRB.g;
                pri.color[2].b = tmpColorB * __colorRB.b;

                pri.color[1].a = tmpColorA * __colorRT.a;
                pri.color[1].r = tmpColorR * __colorRT.r;
                pri.color[1].g = tmpColorG * __colorRT.g;
                pri.color[1].b = tmpColorB * __colorRT.b;

                pri.color[0].a = tmpColorA * __colorLT.a;
                pri.color[0].r = tmpColorR * __colorLT.r;
                pri.color[0].g = tmpColorG * __colorLT.g;
                pri.color[0].b = tmpColorB * __colorLT.b;
            }

            pri.renderMode = _renderMode;

            {
                pri.uv[0] = spr.uvs[0];
                pri.uv[1] = spr.uvs[1];
                pri.uv[2] = spr.uvs[2];
                pri.uv[3] = spr.uvs[3];

                pri.texture = spr.image;
                pri.texId = spr.texID;
            }

            pri.visible = _visible;

            return isFirst;
        }


        bool SetupSpritePrimitiveZY(SpritePrimitive pri, SpriteTransform sTransform, Vector2 parentPosition, float parentRotation, Vector2 parentScale, int layer,  bool isFirst)
        {
            sTransform.primitive = pri;

            if (!sTransform.visible || (!sTransform.isSpriteValid && !sTransform.isOverride))
            {
                pri.visible = false;
                return isFirst;
            }

            Sprite spr = sTransform.isOverride ? sTransform._override : sTransform._sprite;



            layer = (layer - 0xf <= 0 ? 0 : layer) & 0xf;

            float z = (layer << 3) | sTransform.layer;
            pri.compareKey = calcSortKey((ulong)spr.texID, pri, gameObjectLayer, (uint)depth, (uint)sTransform.layer, renderMode, applyLayerToZ);

            //TRS
            {
                float rz = -pri.z;
                float r = Mathf.Deg2Rad * (sTransform.rotation);
                float sin = Mathf.Sin(r);
                float cos = Mathf.Cos(r);

                float pr = Mathf.Deg2Rad * (parentRotation);
                float psin = Mathf.Sin(pr);
                float pcos = Mathf.Cos(pr);


                float sx = 0;
                float sy = 0;
                float tx = 0;
                float ty = 0;
                float psx = 0;
                float psy = 0;
                float px = 0;
                float py = 0;


                for (int i = 0, e = spr.vertices.Length; i < e; i++)
                {
                    sx = (spr.vertices[i].x + (spr.vertices[i].y * -sTransform.shear.y)) * sTransform.scale.x;
                    sy = (spr.vertices[i].y + (spr.vertices[i].x * -sTransform.shear.x)) * sTransform.scale.y;

                    tx = (sx * cos - sy * sin);
                    ty = (sx * sin + sy * cos);

                    psx = (sTransform.position.x + tx) * parentScale.x;
                    psy = (sTransform.position.y + ty) * parentScale.y;

                    tx = parentPosition.x + (psx * pcos - psy * psin);
                    ty = parentPosition.y + (psx * psin + psy * pcos);

                    pri.position[i].z = tx;
                    pri.position[i].y = ty;
                    pri.position[i].x = rz;

                    //Vector2 
                    if (isFirst)
                    {
                        boundingAABB.position_minx = tx;
                        boundingAABB.position_miny = ty;
                        boundingAABB.position_maxx = tx;
                        boundingAABB.position_maxy = ty;
                        isFirst = false;
                    }
                    else
                    {
                        boundingAABB.position_minx = tx < boundingAABB.position_minx ? tx : boundingAABB.position_minx;
                        boundingAABB.position_miny = ty < boundingAABB.position_miny ? ty : boundingAABB.position_miny;
                        boundingAABB.position_maxx = tx > boundingAABB.position_maxx ? tx : boundingAABB.position_maxx;
                        boundingAABB.position_maxy = ty > boundingAABB.position_maxy ? ty : boundingAABB.position_maxy;
                    }
                }
            }

            {
                float tmpColorR = color.r * sTransform.color.r;
                float tmpColorG = color.g * sTransform.color.g;
                float tmpColorB = color.b * sTransform.color.b;
                float tmpColorA = color.a * sTransform.color.a;

                pri.color[3].a = tmpColorA * __colorLB.a;
                pri.color[3].r = tmpColorR * __colorLB.r;
                pri.color[3].g = tmpColorG * __colorLB.g;
                pri.color[3].b = tmpColorB * __colorLB.b;

                pri.color[2].a = tmpColorA * __colorRB.a;
                pri.color[2].r = tmpColorR * __colorRB.r;
                pri.color[2].g = tmpColorG * __colorRB.g;
                pri.color[2].b = tmpColorB * __colorRB.b;

                pri.color[1].a = tmpColorA * __colorRT.a;
                pri.color[1].r = tmpColorR * __colorRT.r;
                pri.color[1].g = tmpColorG * __colorRT.g;
                pri.color[1].b = tmpColorB * __colorRT.b;

                pri.color[0].a = tmpColorA * __colorLT.a;
                pri.color[0].r = tmpColorR * __colorLT.r;
                pri.color[0].g = tmpColorG * __colorLT.g;
                pri.color[0].b = tmpColorB * __colorLT.b;
            }

            pri.renderMode = _renderMode;

            //if (pri.texId != spr.texID)
            {
                pri.uv[0] = spr.uvs[0];
                pri.uv[1] = spr.uvs[1];
                pri.uv[2] = spr.uvs[2];
                pri.uv[3] = spr.uvs[3];

                pri.texture = spr.image;
                pri.texId = spr.texID;
            }

            pri.visible = _visible;

            return isFirst;
        }



        bool SetupSpritePrimitiveXZ(SpritePrimitive pri, SpriteTransform sTransform, Vector2 parentPosition, float parentRotation, Vector2 parentScale, int layer,  bool isFirst)
        {
            sTransform.primitive = pri;

            if ( !sTransform.visible || (!sTransform.isSpriteValid && !sTransform.isOverride) )
            {
                pri.visible = false;
                return isFirst;
            }

            Sprite spr = sTransform.isOverride ? sTransform._override : sTransform._sprite;


            layer = (layer - 0xf <= 0 ? 0 : layer) & 0xf;

            float z = (layer << 3) | sTransform.layer;
            pri.compareKey = calcSortKey((ulong)spr.texID, pri, gameObjectLayer, (uint)depth, (uint)sTransform.layer, renderMode, applyLayerToZ);

            //TRS
            {
                float ry = -pri.z;

                float r = Mathf.Deg2Rad * (sTransform.rotation);
                float sin = Mathf.Sin(r);
                float cos = Mathf.Cos(r);

                float pr = Mathf.Deg2Rad * (parentRotation);
                float psin = Mathf.Sin(pr);
                float pcos = Mathf.Cos(pr);


                float sx = 0;
                float sy = 0;
                float tx = 0;
                float ty = 0;
                float psx = 0;
                float psy = 0;
                float px = 0;
                float py = 0;


                for (int i = 0, e = spr.vertices.Length; i < e; i++)
                {
                    sx = (spr.vertices[i].x + (spr.vertices[i].y * -sTransform.shear.y)) * sTransform.scale.x;
                    sy = (spr.vertices[i].y + (spr.vertices[i].x * -sTransform.shear.x)) * sTransform.scale.y;

                    tx = (sx * cos - sy * sin);
                    ty = (sx * sin + sy * cos);

                    psx = (sTransform.position.x + tx) * parentScale.x;
                    psy = (sTransform.position.y + ty) * parentScale.y;

                    tx = parentPosition.x + (psx * pcos - psy * psin);
                    ty = parentPosition.y + (psx * psin + psy * pcos);

                    pri.position[i].x = tx;
                    pri.position[i].z = ty;
                    pri.position[i].y = ry;

                    //Vector2 
                    if (isFirst)
                    {
                        boundingAABB.position_minx = tx;
                        boundingAABB.position_miny = ty;
                        boundingAABB.position_maxx = tx;
                        boundingAABB.position_maxy = ty;
                        isFirst = false;
                    }
                    else
                    {
                        boundingAABB.position_minx = tx < boundingAABB.position_minx ? tx : boundingAABB.position_minx;
                        boundingAABB.position_miny = ty < boundingAABB.position_miny ? ty : boundingAABB.position_miny;
                        boundingAABB.position_maxx = tx > boundingAABB.position_maxx ? tx : boundingAABB.position_maxx;
                        boundingAABB.position_maxy = ty > boundingAABB.position_maxy ? ty : boundingAABB.position_maxy;
                    }
                }
            }

            {
                float tmpColorR = color.r * sTransform.color.r;
                float tmpColorG = color.g * sTransform.color.g;
                float tmpColorB = color.b * sTransform.color.b;
                float tmpColorA = color.a * sTransform.color.a;

                pri.color[3].a = tmpColorA * __colorLB.a;
                pri.color[3].r = tmpColorR * __colorLB.r;
                pri.color[3].g = tmpColorG * __colorLB.g;
                pri.color[3].b = tmpColorB * __colorLB.b;

                pri.color[2].a = tmpColorA * __colorRB.a;
                pri.color[2].r = tmpColorR * __colorRB.r;
                pri.color[2].g = tmpColorG * __colorRB.g;
                pri.color[2].b = tmpColorB * __colorRB.b;

                pri.color[1].a = tmpColorA * __colorRT.a;
                pri.color[1].r = tmpColorR * __colorRT.r;
                pri.color[1].g = tmpColorG * __colorRT.g;
                pri.color[1].b = tmpColorB * __colorRT.b;

                pri.color[0].a = tmpColorA * __colorLT.a;
                pri.color[0].r = tmpColorR * __colorLT.r;
                pri.color[0].g = tmpColorG * __colorLT.g;
                pri.color[0].b = tmpColorB * __colorLT.b;
            }

            pri.renderMode = _renderMode;

            //if (pri.texId != spr.texID)
            {
                pri.uv[0] = spr.uvs[0];
                pri.uv[1] = spr.uvs[1];
                pri.uv[2] = spr.uvs[2];
                pri.uv[3] = spr.uvs[3];

                pri.texture = spr.image;
                pri.texId = spr.texID;
            }

            pri.visible = _visible;

            return isFirst;
        }






        void ApplyAnchor(Vector3 offset)
        {
            for (int i = 0; i < activePrimitiveCount; i++)
            {
                SpritePrimitive pri = primitives[i];
                pri.position[0].x += offset.x;
                pri.position[0].y += offset.y;
                pri.position[0].z += offset.z;

                pri.position[1].x += offset.x;
                pri.position[1].y += offset.y;
                pri.position[1].z += offset.z;

                pri.position[2].x += offset.x;
                pri.position[2].y += offset.y;
                pri.position[2].z += offset.z;

                pri.position[3].x += offset.x;
                pri.position[3].y += offset.y;
                pri.position[3].z += offset.z;
            }
        }











        internal void OnEnable()
        {
            behaviourEnabled = true;
            Init();
        }



        void OnDisable()
        {
            behaviourEnabled = false;
        }



        void OnDestroy()
        {
            int layer = gameObject.layer;

            if ( commitToSceneManager )
                primitiveGroup.RemoveFromLayer(layer);
        }



        void drawBoundingBox(Vector3[] position, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(position[0], position[1]);
            Gizmos.DrawLine(position[1], position[2]);
            Gizmos.DrawLine(position[2], position[3]);
            Gizmos.DrawLine(position[3], position[0]);
        }



        void Update()
        {
            if (updateMode == SpriteRendererUpdateMode.Update )
            {
                if ( !isUpdated )
                    Apply();

                isUpdated = false;
            }
        }

        void LateUpdate()
        {
            if (updateMode == SpriteRendererUpdateMode.LateUpdate )
            {
                if (!isUpdated)
                    Apply();

                isUpdated = false;
            }
        }

        void FixedUpdate()
        {
            if (updateMode == SpriteRendererUpdateMode.FixedUpdate )
            {
                if (!isUpdated)
                    Apply();

                isUpdated = false;
            }
        }



        static Vector3[] drawingBox = new Vector3[4];

        //draw sprite bounding box
        void OnDrawGizmos()
        {
            if (showAttachSpriteBounding)
            {
                for (int i = 0; i < activePrimitiveCount; i++)
                {
                    if (spriteTransforms[i].visible && spriteTransforms[i].isSpriteValid)
                        drawBoundingBox(primitives[i].position, Color.white);
                }
            }

            if (showCullingGroupBounding)
            {
                foreach (SpriteTransformCullingGroup group in cullingGroup)
                {
                    Vector3 center = Vector3.zero;
                    float groupRadius = 0;

                    if (plane == SpritePlane.PlaneXY)
                    {
                        float rw = group.size.x * transform.localScale.x;
                        float rh = group.size.y * transform.localScale.y;

                        center = transform.TransformPoint(group.center);
                        Vector3 half = new Vector3(rw * 0.5f, rh * 0.5f, 0);
                        groupRadius = half.magnitude;

                        Vector2 lt = center - half;
                        Vector2 rb = center + half;

                        drawingBox[0] = lt;

                        drawingBox[1].x = rb.x;
                        drawingBox[1].y = lt.y;
                        drawingBox[1].z = center.z;

                        drawingBox[2] = rb;


                        drawingBox[3].x = lt.x;
                        drawingBox[3].y = rb.y;
                        drawingBox[3].z = center.z;
                    }

                    else  if (plane == SpritePlane.PlaneXZ)
                    {
                        float rw = group.size.x * transform.localScale.x;
                        float rh = group.size.y * transform.localScale.z;

                        center = transform.TransformPoint( new Vector3( group.center.x, 0, group.center.y ) );
                        Vector3 half = new Vector3(rw * 0.5f, 0f, rh * 0.5f );
                        groupRadius = half.magnitude;


                        Vector3 lt = center - half;
                        Vector3 rb = center + half;

                        drawingBox[0] = lt;

                        drawingBox[1].x = rb.x;
                        drawingBox[1].y = center.y;
                        drawingBox[1].z = lt.z;

                        drawingBox[2] = rb;

                        drawingBox[3].x = lt.x;
                        drawingBox[3].y = 0f;
                        drawingBox[3].z = rb.z;
                    }


                    else if (plane == SpritePlane.PlaneZY)
                    {
                        float rw = group.size.x * transform.localScale.z;
                        float rh = group.size.y * transform.localScale.y;

                        center = transform.TransformPoint(new Vector3(0, group.center.y, group.center.x));
                        Vector3 half = new Vector3(0f, rh * 0.5f, rw * 0.5f);
                        groupRadius = half.magnitude;


                        Vector3 lt = center - half;
                        Vector3 rb = center + half;

                        drawingBox[0] = lt;

                        drawingBox[1].x = center.x;
                        drawingBox[1].y = lt.y;
                        drawingBox[1].z = rb.z;

                        drawingBox[2] = rb;

                        drawingBox[3].x = center.x;
                        drawingBox[3].y = rb.y;
                        drawingBox[3].z = lt.z;
                    }

                    drawBoundingBox(drawingBox, Color.green);
                    Gizmos.DrawWireSphere( center, groupRadius );
                }
            }

            if (!showBounding)
                return;


            if (plane == SpritePlane.PlaneXY)
            {
                drawingBox[0].x = boundingAABB.position_minx;
                drawingBox[0].y = boundingAABB.position_miny;
                drawingBox[0].z = 0f;

                drawingBox[1].x = boundingAABB.position_maxx;
                drawingBox[1].y = boundingAABB.position_miny;
                drawingBox[1].z = 0f;

                drawingBox[2].x = boundingAABB.position_maxx;
                drawingBox[2].y = boundingAABB.position_maxy;
                drawingBox[2].z = 0f;

                drawingBox[3].x = boundingAABB.position_minx;
                drawingBox[3].y = boundingAABB.position_maxy;
                drawingBox[3].z = 0f;

                drawBoundingBox(drawingBox, Color.green);

                Gizmos.DrawWireSphere(boundingAABB.center, radius);
            }

            if (plane == SpritePlane.PlaneZY)
            {
                drawingBox[0].z = boundingAABB.position_minx;
                drawingBox[0].y = boundingAABB.position_miny;
                drawingBox[0].x = 0f;

                drawingBox[1].z = boundingAABB.position_maxx;
                drawingBox[1].y = boundingAABB.position_miny;
                drawingBox[1].x = 0f;

                drawingBox[2].z = boundingAABB.position_maxx;
                drawingBox[2].y = boundingAABB.position_maxy;
                drawingBox[2].x = 0f;

                drawingBox[3].z = boundingAABB.position_minx;
                drawingBox[3].y = boundingAABB.position_maxy;
                drawingBox[3].x = 0f;

                drawBoundingBox(drawingBox, Color.green);
                Gizmos.DrawWireSphere(new Vector3(0, boundingAABB.center.y, boundingAABB.center.x), radius);
            }

            if (plane == SpritePlane.PlaneXZ)
            {
                drawingBox[0].x = boundingAABB.position_minx;
                drawingBox[0].z = boundingAABB.position_miny;
                drawingBox[0].y = 0f;

                drawingBox[1].x = boundingAABB.position_maxx;
                drawingBox[1].z = boundingAABB.position_miny;
                drawingBox[1].y = 0f;

                drawingBox[2].x = boundingAABB.position_maxx;
                drawingBox[2].z = boundingAABB.position_maxy;
                drawingBox[2].y = 0f;

                drawingBox[3].x = boundingAABB.position_minx;
                drawingBox[3].z = boundingAABB.position_maxy;
                drawingBox[3].y = 0f;

                drawBoundingBox(drawingBox, Color.green);
                Gizmos.DrawWireSphere(new Vector3(boundingAABB.center.x, 0, boundingAABB.center.y), radius);
            }
        }







        /// <summary>
        /// Internal method. You do not to need use this.
        /// </summary>
        public void DoClipping(Vector3 position, float radius)
        {
            Vector2 cameraCenter = Vector2.zero;

            if (plane == SpritePlane.PlaneXY)
            {
                cameraCenter.x = position.x;
                cameraCenter.y = position.y;
            }
            else if (plane == SpritePlane.PlaneXZ)
            {
                cameraCenter.x = position.x;
                cameraCenter.y = position.z;
            }
            else if (plane == SpritePlane.PlaneZY)
            {
                cameraCenter.x = position.z;
                cameraCenter.y = position.y;
            }



            if (!isCullingGroup)
            {
                bool needClipping = !_visible ||
                    !behaviourEnabled ||
                    Vector2.Distance( cameraCenter, boundingAABB.center) > (radius + this.radius);

                primitiveGroup.visible = _visible && behaviourEnabled && !needClipping;
            }
            else
            {
                primitiveGroup.visible = true;
                Vector3 tmpScale = _transform.localScale;
                Vector2 tmpGroupPos = Vector2.zero;

                if (plane == SpritePlane.PlaneXY)
                {
                    float xs = tmpScale.x;
                    float ys = tmpScale.y;

                    Vector2 groupCenter = Vector2.zero;

                    foreach (SpriteTransformCullingGroup group in cullingGroup)
                    {
                        Vector3 pos = transform.TransformPoint(group.center);
                        float groupRadius = new Vector2(group.size.x * xs * 0.5f, group.size.y * ys * 0.5f).magnitude;

                        tmpGroupPos.x = pos.x;
                        tmpGroupPos.y = pos.y;
                        bool vis = Vector2.Distance(cameraCenter, tmpGroupPos) <= (radius + groupRadius);

                        if (!group.isInit || group.lastVisible != vis)
                        {
                            for (int i = 0, e = group.sprIdx.Length; i < e; i++)
                            {
                                int idx = group.sprIdx[i];
                                SpriteTransform tran = GetSpriteTransform(idx);
                                tran.visible = vis;
                                primitives[idx].visible = vis;
                            }
                            group.lastVisible = vis;
                            group.isInit = true;
                        }
                    }
                }

                else if (plane == SpritePlane.PlaneXZ)
                {
                    float xs = tmpScale.x;
                    float zs = tmpScale.z;

                    Vector2 groupCenter = Vector2.zero;

                    foreach (SpriteTransformCullingGroup group in cullingGroup)
                    {
                        Vector3 pos = transform.TransformPoint(new Vector3(group.center.x, 0, group.center.y));

                        float groupRadius = new Vector2(group.size.x * xs * 0.5f, group.size.y * zs * 0.5f).magnitude;

                        tmpGroupPos.x = pos.x;
                        tmpGroupPos.y = pos.z;
                        bool vis = Vector2.Distance(cameraCenter, tmpGroupPos) <= (radius + groupRadius);

                        if (!group.isInit || group.lastVisible != vis)
                        {
                            for (int i = 0, e = group.sprIdx.Length; i < e; i++)
                            {
                                int idx = group.sprIdx[i];
                                SpriteTransform tran = GetSpriteTransform(idx);
                                tran.visible = vis;
                                primitives[idx].visible = vis;
                            }
                            group.lastVisible = vis;
                            group.isInit = true;
                        }
                    }
                }


                else if (plane == SpritePlane.PlaneZY)
                {
                    float zs = tmpScale.z;
                    float ys = tmpScale.y;

                    Vector2 groupCenter = Vector2.zero;

                    foreach (SpriteTransformCullingGroup group in cullingGroup)
                    {
                        Vector3 pos = transform.TransformPoint(new Vector3(0, group.center.y, group.center.x));

                        float groupRadius = new Vector2(group.size.x * zs * 0.5f, group.size.y * ys * 0.5f).magnitude;

                        tmpGroupPos.x = pos.z;
                        tmpGroupPos.y = pos.y;
                        bool vis = Vector2.Distance(cameraCenter, tmpGroupPos) <= (radius + groupRadius);

                        if (!group.isInit || group.lastVisible != vis)
                        {
                            for (int i = 0, e = group.sprIdx.Length; i < e; i++)
                            {
                                int idx = group.sprIdx[i];
                                SpriteTransform tran = GetSpriteTransform(idx);
                                tran.visible = vis;
                                primitives[idx].visible = vis;
                            }
                            group.lastVisible = vis;
                            group.isInit = true;
                        }
                    }
                }
            }
        }




        void Reset()
        {
            UnityEngine.Object[] rms = Resources.FindObjectsOfTypeAll(typeof(SpriteRenderModeSetting));
            foreach (SpriteRenderModeSetting rm in rms)
            {
                if (rm.renderMode == SpriteRenderMode.AlphaBlend)
                {
                    renderMode = rm;
                    break;
                }
            }


            UnityEngine.Object[] sprs = Resources.FindObjectsOfTypeAll(typeof(Sprite));
            foreach (Sprite sp in sprs)
            {
                if (sp.name == "DefaultSpriteRendererSprite")
                {
                    Clear();
                    AttachSprite(sp);
                    Apply();

                    //foreach (Camera cam in Camera.allCameras)
                    //{
                    //    Camera2D c2d = cam.GetComponent<Camera2D>();
                    //    if (c2d != null)
                    //        c2d.Render();
                    //}
                    break;
                }
            }
        }
    }


}