using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace EasyMotion2D
{

    internal class FixedArrayAllocator<T>
    {
        public struct FixedArrayInfo
        {
            public int fixedSize;
            public T[] array;
        }

        static List<FixedArrayInfo> arrays = new List<FixedArrayInfo>();

        public static void Clear()
        {
            arrays.Clear();
        }

        public static void AddArray(int minSize, int maxSize, int baseStep, int ratio)
        {
            for (int i = minSize; i <= maxSize; i++)
            {
                int l = baseStep;
                
                bool found = false;
                foreach (FixedArrayInfo _i in arrays)
                {
                    if (_i.fixedSize == l)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    continue;
                

                FixedArrayInfo info = new FixedArrayInfo();
                info.fixedSize = l;
                arrays.Add(info);

                baseStep *= ratio;
            }
        }

        public static void AddArray( int minSize, int maxSize, int step)
        {
            for (int i = minSize; i <= maxSize; i += step)
            {
                FixedArrayInfo tmp = GetArray(i);
                if (tmp.fixedSize != 0)
                    continue;

                FixedArrayInfo info = new FixedArrayInfo();
                info.fixedSize = i;
                arrays.Add(info);
            }
        }


        public static FixedArrayInfo GetArray(int size)
        {
            FixedArrayInfo ret = new FixedArrayInfo();
            for( int i = 0,e = arrays.Count; i < e ; i++)
            {
                FixedArrayInfo info = arrays[i];
                if (info.fixedSize >= size)
                {
                    if (info.array == null)
                    {
                        info.array = new T[info.fixedSize];
                        arrays[i] = info;
                    }
                    return info;
                }
            }
            return ret;
        }
    }








    [System.Serializable]
    internal class SpriteRenderModeGroup
    {
        public bool isValid = false;
        public SpriteRenderModeSetting setting = null;
        public Hashtable materials = new Hashtable();
    }




    internal class SpriteMaterialManager
    {
        static SpriteRenderModeGroup[] renderModeGroup = new SpriteRenderModeGroup[16];

        static internal void UnregisterRenderMode(SpriteRenderModeSetting settings)
        {
            SpriteRenderModeGroup group = renderModeGroup[ (int)settings.renderMode];
            foreach( Material mat in group.materials.Values)
            {
                if ( Application.isPlaying)
                    Object.Destroy( mat);
                else
                    Object.DestroyImmediate(mat);
            }
        }

        static internal void RegisterRenderMode(SpriteRenderModeSetting[] settings)
        {
            foreach (SpriteRenderModeSetting setting in settings)
                RegisterRenderMode(setting);
        }


        static internal void RegisterRenderMode(SpriteRenderModeSetting setting)
        {
            if (setting == null) return;
            int idx = (int)setting.renderMode;
            SpriteRenderModeGroup group = renderModeGroup[idx];
            if (group == null)
            {
                renderModeGroup[idx] = group = new SpriteRenderModeGroup();
                group.isValid = true;
            }
            group.setting = setting;
        }


        static public Material GetMaterial(SpriteRenderMode rm, Texture tex)
        {
            int idx = rm == SpriteRenderMode.None ? 0 : (int)rm;
            SpriteRenderModeGroup group = renderModeGroup[idx];
            Material ret = null;

            if (group != null)
            {
                if ((ret = group.materials[tex] as Material) == null)
                {
                    ret = new Material(group.setting.shader);
                    ret.mainTexture = tex;
                    group.materials[tex] = ret;
                    group.setting.materials.Add(ret);
                }
            }
            else
            {
                Debug.LogError("unknown RenderMode " + rm);
            }
            return ret;
        }

        internal static bool IsSemiTransparent(SpriteRenderMode rm)
        {
            SpriteRenderModeGroup group = renderModeGroup[(int)rm];
            return group != null ? group.setting.sortByDepth : false;
        }
    }






















    class SubMeshInfo
    {
        public int triangleCnt;
        public int indexBase;
        public Material mat;
    }



    /// <summary>
    /// Render all commited sprite with clipping, dynamic batching and depth sorting.<br/>
    /// Internal class. You do not need to use this.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SpriteBatchRenderer : MonoBehaviour
    {
        /// <summary>
        /// This is used to select gameObjects in scene who will be render.
        /// </summary>
        public LayerMask cullingMask;

        /// <summary>
        /// The commited sprite's count with last rendering.
        /// </summary>
        public int lastCommitSpriteConnt = 0;

        /// <summary>
        /// The commited triangle's count with last rendering.
        /// </summary>
        public int lastCommitTriangleCount = 0;



        private MeshFilter meshFilter = null;
        internal Mesh renderMesh = null;
        private int subMeshCount = 0;


        static private Vector3[] vertices = null;
        static private Vector2[] UVs = null;
        static private Color[] colors = null;
        static private int[] indices = null;
        static private SubMeshInfo[] subMeshs = null;
        static internal SpritePrimitive[] _primitives = null;
        static private int[] sortKeys = null;



        void Awake()
        {
            //gameObject.hideFlags = HideFlags.NotEditable;// | HideFlags.HideInHierarchy;
            //Init();
        }


        static SpriteRenderModeGroup[] renderModeGroup = new SpriteRenderModeGroup[16];
        static bool[] isSemiTransparentFlag = new bool[16];

        internal bool isInit = false;

        public int maxPrimitiveCount = 2048;

        internal void Init( int maxPrimitiveCount)
        {
            renderer.materials = new Material[0];


            //init component
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            renderMesh = meshFilter.sharedMesh;


            this.maxPrimitiveCount = maxPrimitiveCount;
            int count = maxPrimitiveCount;


            if (vertices == null || count > vertices.Length )
                //init vertices, uv, color
                vertices = new Vector3[count * 4];

            if (UVs == null || count > UVs.Length)
                UVs = new Vector2[count * 4];

            if (colors == null || count > colors.Length)   
                colors = new Color[count * 4];

            if (indices == null || count > indices.Length)
            {
                //init index with triangle list
                indices = new int[count * 6];
                for (int i = 0; i < count; i++)
                {
                    int srcIdxBase = i * 4;
                    int idxBase = i * 6;

                    indices[idxBase + 0] = srcIdxBase + 0;
                    indices[idxBase + 1] = srcIdxBase + 1;
                    indices[idxBase + 2] = srcIdxBase + 2;
                    indices[idxBase + 3] = srcIdxBase + 0;
                    indices[idxBase + 4] = srcIdxBase + 2;
                    indices[idxBase + 5] = srcIdxBase + 3;
                }
            }



            //init submesh
            subMeshCount = 0;


            if (subMeshs == null )
            {
                subMeshs = new SubMeshInfo[512];
                for (int i = 0, l = subMeshs.Length; i < l; i++)
                    subMeshs[i] = new SubMeshInfo();
            }

            if (_primitives == null || count > _primitives.Length)
                //init primitive pool
                _primitives = new SpritePrimitive[count];

            if (sortKeys == null || count > sortKeys.Length)
                sortKeys = new int[count];


            FixedArrayAllocator<int>.AddArray(1, 8, 6, 2);
            FixedArrayAllocator<int>.AddArray(256 * 6, count * 6, 256 * 6);

            vertices[0] = new Vector3(-1000000f, -1000000f, -100000f);
            vertices[1] = new Vector3(1000000f, -1000000f, -100000f);
            vertices[2] = new Vector3(-1000000f, 1000000f, 100000f);
            vertices[3] = new Vector3(1000000f, 1000000f, 100000f);

            renderMesh.vertices = vertices;
            renderMesh.uv = UVs;
            renderMesh.colors = colors;
            renderMesh.triangles = new int[] { 0, 1, 2, 1, 2, 3 };

            renderMesh.RecalculateBounds();

            isInit = true;
        }



        int getAndSortPrimitives( ref int visTriangles, bool isClipping, Vector3 cameraPos, float radius)
        {
            int priCount = getPrimitive(0, isClipping, cameraPos, radius);
            
            //get all primitives to be render
            if (priCount == 0)
                return priCount;

            //sort it
            System.Array.Sort(_primitives, 0, priCount, SpritePrimitiveComparer.comparer);
           
            return groupPrimitive(priCount, ref visTriangles);
        }

        virtual protected int getPrimitive(int priCount, bool isClipping, Vector3 cameraPos, float radius )
        {
            int[] layerid = SpriteManager.GetLayersID();


            if (_primitives == null)
                Init(maxPrimitiveCount);

            int queueMaxLength = _primitives.Length;


            for (int layerIdx = 0, e = layerid.Length; layerIdx < e; layerIdx++)
            {
                int id = layerid[layerIdx];
                int t = cullingMask.value & (1 << id);

                if (t != 0)
                {
                    SpriteLayer layer = SpriteManager.GetLayer(id);
                    
                    int l = layer.size;
                    if (l == 0) continue;

                    if (isClipping)
                    {
                        int ls = layer.size;
                        for (int ri = 0; ri < l; ri++)
                        {
                            layer[ri].renderable.DoClipping(cameraPos, radius);
                        }
                    }

                    for (int gourpIdx = 0; gourpIdx < l; gourpIdx++)
                    {
                        SpritePrimitiveGroup group = layer.baseContainer.datas[gourpIdx];

                        if (!group.visible)
                            continue;

                        for( int pi =0; pi < group.count.value; pi++)
                        {
                            SpritePrimitive pri = group.primitives[pi];
                            if (pri.visible == false || pri.texId == 0)
                                continue;

                            _primitives[priCount] = pri;
                            priCount++;

                            if (priCount >= queueMaxLength)
                            {
                                Debug.Log("Commited SpritePrimitive's count is more than " + queueMaxLength + "!.\n" +
                                    "if in editor, you should disable some layer.\n" +
                                    "if in runtime, you should enable Camera2D clipping..\n");

                                return priCount;
                            }
                        }
                    }
                }
            }

            return priCount;
        }


        private int groupPrimitive(int priCount, ref int visTriangles)
        {
            subMeshCount = 0;

            int vertexIdx = 0;
            Texture lastTex = null;
            uint lastTexId = 0;
            int triangleCnt = 0;
            int startIdx = 0;
            SpriteRenderMode lastSRM = SpriteRenderMode.None;


            SpritePrimitive primitive = null;
            int visPriCount = 0;

            for (int pi = 0; pi < priCount; pi++)
            {
                primitive = _primitives[pi];
                if (lastSRM != primitive.renderMode || lastTexId != primitive.texId)
                {
                    if (lastTexId != 0)
                    {
                        SubMeshInfo info = subMeshs[subMeshCount];

                        info.indexBase = startIdx;
                        info.triangleCnt = triangleCnt;
                        info.mat = SpriteMaterialManager.GetMaterial(lastSRM, lastTex);

                        subMeshCount++;

                        startIdx = (vertexIdx >> 2) * 6;
                        visTriangles += triangleCnt;
                        triangleCnt = 0;
                    }

                    lastTexId = primitive.texId;
                    lastTex = primitive.texture;
                    lastSRM = primitive.renderMode;
                }

                int c = primitive.size << 2;
                for (int i = 0; i < c; i++)
                {
                    int idx =  vertexIdx + i;
                    vertices[idx] = primitive.position[i];
                    UVs[idx] = primitive.uv[i];
                    colors[idx] = primitive.color[i];
                }

                vertexIdx += c;
                triangleCnt += (c >> 1);
                visPriCount++;
            }

            {
                SubMeshInfo _info = subMeshs[subMeshCount];

                _info.indexBase = startIdx;
                _info.triangleCnt = triangleCnt;
                _info.mat = SpriteMaterialManager.GetMaterial(lastSRM, lastTex);

                visTriangles += triangleCnt;
            }

            subMeshCount++;           
            return visPriCount;
        }


        internal void CommitToRender(bool isClipping, Vector3 cameraPos, float radius)
        {
            UpdateSpriteMesh( isClipping, cameraPos, radius);
        }


        private Vector3[] _vertices = new Vector3[128];
        private Vector2[] _uvs = new Vector2[128];
        private Color[] _colors = new Color[128];


        void UpdateSpriteMesh(bool isClipping, Vector3 cameraPos, float radius)
        {
            int triangles = 0;
            int priCount = getAndSortPrimitives(ref triangles, isClipping, cameraPos, radius);

            lastCommitSpriteConnt = priCount;
            lastCommitTriangleCount = triangles;

            if (priCount == 0)
            {
                if (renderMesh != null && renderMesh.vertexCount > 0)
                    renderMesh.subMeshCount = 0;

                return;
            }


            //set vertices, uvs, colors
            int l = priCount * 4;
            int arrlen = _vertices.Length;
            if (l > arrlen  || l < arrlen >> 1 )
            {
                if (l > arrlen)
                    arrlen = l + 128;
                else if (l < arrlen >> 1)
                    arrlen = arrlen >> 1;

                _vertices = new Vector3[arrlen];
                _uvs = new Vector2[arrlen];
                _colors = new Color[arrlen];
            }

            renderMesh.subMeshCount = 0;

            System.Array.Copy(vertices, _vertices, l);
            System.Array.Copy(UVs, _uvs, l);
            System.Array.Copy(colors, _colors, l);

            renderMesh.vertices = _vertices;
            renderMesh.colors = _colors;
            renderMesh.uv = _uvs;



            int c = 0;

            //set submesh count
            renderMesh.subMeshCount = subMeshCount;
            Material[] mats = new Material[subMeshCount];

            //set triangles
            for (int i = 0; i < subMeshCount; i++)
            {
                SubMeshInfo info = subMeshs[i];
                int len = info.triangleCnt * 3;
                //get closest array
                FixedArrayAllocator<int>.FixedArrayInfo arrayInfo = FixedArrayAllocator<int>.GetArray(len);

                //copy index
                System.Array.Copy(indices, info.indexBase, arrayInfo.array, 0, len);
                //clean last
                System.Array.Clear(arrayInfo.array, len, arrayInfo.fixedSize - len);
                //set triangle list
                renderMesh.SetTriangles(arrayInfo.array, i);
                //set submesh material
                mats[i] = info.mat;

                c += arrayInfo.fixedSize;
            }

            renderer.materials = mats;

            OnCommitEnd();
        }


        virtual protected void OnCommitEnd()
        {
        }


        void OnEnable()
        {
            if (!isInit)
                Init(maxPrimitiveCount);

            renderer.enabled = true;
        }

        void OnDisable()
        {
            renderer.enabled = false;
        }

        void OnDestroy()
        {
            renderer.materials = new Material[0];
            meshFilter.sharedMesh = null;
            if (Application.isPlaying)
                Destroy(renderMesh);
            else
                DestroyImmediate(renderMesh);

            OnDestroyImpl();
        }

        virtual protected void OnDestroyImpl()
        {
        }
    }





    [ExecuteInEditMode]
    public class SpriteSceneRenderer : SpriteBatchRenderer
    {
        public Camera2D clippingCamera;

        void Update()
        {
            if (clippingCamera != null)
            {
                CommitToRender(false, Vector3.zero, 0f);
            }
            else
            {
                CommitToRender(false, Vector3.zero, 0f);
            }
        }


        void OnDrawGizmos()
        {
            if (Application.isEditor && !Application.isPlaying)
                Update();
        }
    }


    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteMeshRenderer : SpriteBatchRenderer
    {
        [HideInInspector]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
                spriteRenderer.commitToSceneManager = false;
        }

        void Update()
        {
            if (spriteRenderer.isApply)
            {
                CommitToRender(false, Vector3.zero, 0f);
                spriteRenderer.isApply = false;
            }
        }


        void OnDrawGizmos()
        {
            if (Application.isEditor && !Application.isPlaying)
                Update();
        }



        override protected int getPrimitive(int priCount, bool isClipping, Vector3 cameraPos, float radius)
        {
            if (_primitives == null)
                Init(maxPrimitiveCount);

            int queueMaxLength = _primitives.Length;

            if ( spriteRenderer != null)
            {
                SpritePrimitiveGroup group = spriteRenderer.primitiveGroup;

                for (int pi = 0; pi < group.count.value; pi++)
                {
                    SpritePrimitive pri = group.primitives[pi];
                    if (pri.visible == false || pri.texId == 0)
                        continue;

                    _primitives[priCount] = pri;
                    priCount++;

                    if (priCount >= queueMaxLength)
                    {
                        Debug.Log("Commited SpritePrimitive's count is more than " + queueMaxLength + "!.\n" +
                            "if in editor, you should disable some layer.\n" +
                            "if in runtime, you should enable Camera2D clipping..\n");

                        return priCount;
                    }
                }
            }

            return priCount;
        }


        override protected void OnCommitEnd()
        {
            renderMesh.RecalculateBounds();
        }

        override protected void OnDestroyImpl()
        {
            if ( spriteRenderer != null )
                spriteRenderer.commitToSceneManager = true;
        }
    }
}