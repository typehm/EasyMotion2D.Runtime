using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EasyMotion2D
{
    internal class TextureGUIDManager
    {
        public static bool _instNull = true;
        public static TextureGUIDManager _inst = new TextureGUIDManager();
        public static TextureGUIDManager instance
        {
            get
            {
                return _inst;
            }
        }

        private Hashtable texs = new Hashtable();
        private uint guid = 1;

        private TextureGUIDManager()
        {
        }

        public uint RegisterTexture(Texture2D tex)
        {
            if (tex == null) return 0;

            int texID = tex.GetInstanceID();
            uint id = GetTextureID(texID);
            if (id == 0)
            {
                id = guid++;
                texs.Add(texID, id);

                if ( id > (1 << 9) )
                    Debug.LogError("Error, Sprite used Texture2D counts greater than 1024. You need Combine some texture to keep it less than 1024");
            }
            return id;
        }

        public uint GetTextureID(int texID)
        {
            if (texs.ContainsKey(texID))
                return (uint)texs[texID];
            return 0;
        }
    }




    /// <summary>
    /// Sprite is the render primitive in Easy2D.
    /// </summary>
    public class Sprite : ScriptableObject
    {
        /// <summary>
        /// Atlas the sprite used.
        /// </summary>
        public SpriteAtlasAsset atlas = null;

        /// <summary>
        /// Image the sprite used.
        /// </summary>
        public Texture2D image = null;

        /// <summary>
        /// Sprite image block in image.
        /// </summary>
        public Rect imageRect;

        /// <summary>
        /// Sprite's parent group.
        /// Usually use in edit time.
        /// Will return null at runtime.
        /// </summary>
        public SpriteGroup parent
        {
            get
            {
                return Application.isEditor && Application.isPlaying ?
                    null :
                    SpriteGroup.GetByID(parentID);
            }

            set
            {
                if (value != null)
                {
                    parentName = value.name;
                    parentID = value.GetInstanceID();
                }
                else
                {
                    parentName = string.Empty;
                    parentID = 0;
                }
            }
        }

        [HideInInspector]
        [SerializeField]
        internal string parentName = string.Empty;
        internal int parentID;

        //public string parent;

        /// <summary>
        /// The image texture path of sprite linked.
        /// </summary>
        public string linkedTexturePath = "";
        /// <summary>
        /// Sprite image block in image.
        /// </summary>
        public Rect linkedImageRect;

        public Vector2 origTextureSize;

        /// <summary>
        /// The image pivot of sprite.
        /// </summary>
        public Vector2 pivot = Vector2.zero;

        /// <summary>
        /// The image position of sprite.
        /// </summary>
        public Vector2 position = Vector2.zero;

        /// <summary>
        /// The image rotation of sprite.
        /// </summary>
        public float rotation = 0f;

        /// <summary>
        /// The image scale of sprite.
        /// </summary>
        public Vector2 scale = Vector2.one;

        /// <summary>
        /// The name of sprite.
        /// </summary>
        //[HideInInspector]
        public string spriteName;


        /// <summary>
        /// Internal texID of sprite's texture. Not a native ('hardware') handle to a texture.
        /// </summary>
        [HideInInspector]
        public uint texID;


        /// <summary>
        /// postions of sprite
        /// </summary>
        [HideInInspector]
        public Vector3[] vertices = new Vector3[4];


        /// <summary>
        /// UVs of sprite
        /// </summary>
        [HideInInspector]
        public Vector2[] uvs = new Vector2[4];

        /// <summary>
        /// Not used.
        /// </summary>
        public string fullPath = "";

        public bool useOrigSize = false;

        void OnEnable()
        {
            
            Add(nameSpritePairs, name, this);
            Init();
        }

        void OnDisable()
        {
            Remove(nameSpritePairs, name, this);
        }

        /// <summary>
        /// Initialize the sprite. You only need to call it if you create a sprite at runtime with ScriptableObject.CreateInstance.
        /// </summary>
        public void Init()
        {
            texID = TextureGUIDManager.instance.RegisterTexture(image);

            InitVertices();

            if (image)
                InitUVs();
        }

        internal void InitVertices()
        {
            Vector2 lt = new Vector2(-pivot.x * scale.x, pivot.y * scale.y);
            Vector2 rt = lt + new Vector2(imageRect.width * scale.x, 0);
            Vector2 lb = lt + new Vector2(0, -imageRect.height * scale.y);
            Vector2 rb = lt + new Vector2(imageRect.width * scale.x, - imageRect.height * scale.y);

            Vector3 leftTop = Quaternion.Euler(0, 0, rotation) * lt;
            Vector3 rightTop = Quaternion.Euler(0, 0, rotation) * rt;
            Vector3 leftBottom = Quaternion.Euler(0, 0, rotation) * lb;
            Vector3 rightBottom = Quaternion.Euler(0, 0, rotation) * rb;

            Vector3 pos = position;
            vertices[0] = pos + leftTop;
            vertices[1] = pos + rightTop;
            vertices[2] = pos + rightBottom;
            vertices[3] = pos + leftBottom;
        }

        internal void InitUVs()
        {
            uvs[0] = ConvertPixelToUV(0, 0 );
            uvs[1] = ConvertPixelToUV(imageRect.width, 0);
            uvs[2] = ConvertPixelToUV(imageRect.width, imageRect.height);
            uvs[3] = ConvertPixelToUV(0, imageRect.height);
        }

        /// <summary>
        /// Convert a position in imageRect to image UV.
        /// </summary>
        public Vector2 ConvertPixelToUV(float x, float y)
        {
            if (useOrigSize)
            {
                return new Vector2(
                    (imageRect.xMin + x) / origTextureSize.x,
                    1f - (imageRect.yMin + y) / origTextureSize.y);
            }

            float w = 1f / image.width;
            float h = 1f / image.height;
            float xmin = (imageRect.xMin + x) * w;
            float ymin = 1f - (imageRect.yMin + y) * h;

            return new Vector2(xmin, ymin);
        }



        /// <summary>
        /// Clone a Sprite ScriptableObject.
        /// </summary>
        public static Sprite Duplicate(Sprite source)
        {
            Sprite ret = ScriptableObject.CreateInstance<Sprite>();
          
            ret.name = source.name;
            ret.spriteName = source.spriteName;

            ret.image = source.image;
            ret.imageRect = source.imageRect;

            ret.pivot = source.pivot;
            ret.position = source.position;
            ret.rotation = source.rotation;
            ret.scale = source.scale;

            ret.parent = source.parent;

            ret.Init();
            return ret;
        }




        private static Dictionary<string, List<System.WeakReference>> nameSpritePairs = new Dictionary<string, List<System.WeakReference>>();
        /// <summary>
        /// Find a sprite by it name.
        /// </summary>
        /// <param name="name">Sprite's name.</param>
        /// <returns>If not found, return null.</returns>
        public static Sprite[] GetSprite(string name)
        {
            List<System.WeakReference> wps = new List<System.WeakReference>();

            nameSpritePairs.TryGetValue(name, out wps);

            List<Sprite> ret = new List<Sprite>();

            if (wps != null)
            {
                foreach (System.WeakReference wp in wps)
                {
                    if (wp.IsAlive)
                        ret.Add(wp.Target as Sprite);
                }
            }

            return ret.ToArray();
        }


        static void Add(Dictionary<string, List<System.WeakReference>> dict, string key, Sprite spr)
        {
            List<System.WeakReference> l = null;
            if (dict.ContainsKey(key))
            {
                l = dict[key];
            }
            else
            {
                l = new List<System.WeakReference>();
                dict.Add(key, l);
            }

            l.Add( new System.WeakReference(spr) );
        }

        
        static void Remove(Dictionary<string, List<System.WeakReference>> dict, string key, Sprite spr)
        {
            List<System.WeakReference> l = null;
            if (dict.ContainsKey(key))
            {
                l = dict[key];

                List<System.WeakReference> removeList = new List<System.WeakReference>();
                foreach (System.WeakReference wp in l)
                {
                    if (wp.Target == spr || wp.IsAlive == false)
                        removeList.Add(wp);
                }

                foreach (System.WeakReference wp in removeList)
                    l.Remove(wp);
            }
        }


        static int sortByHeight(Sprite lhs, Sprite rhs)
        {
            int la = (int)(lhs.imageRect.height);
            int ra = (int)(rhs.imageRect.height);
            return la - ra;
        }



        /// <summary>
        /// Pack sprites into a texture. Usually use to building atlas in runtime.
        /// The sprite's image texture should be readable.
        /// </summary>
        /// <param name="sprites">Array of sprites you want pack.</param>
        /// <param name="packSize">The target texture size.</param>
        /// <param name="mipmap">Not used.</param>
        /// <returns>Return true successed, false means sprites can not layout in the size of texture.</returns>
        public static bool PackSprites( Sprite[] sprites, int packSize, bool mipmap )
        {
            List<Sprite> sprs = new List<Sprite>(sprites);
            sprs.Sort(sortByHeight);
            sprs.Reverse();


            List<Rect> rects = new List<Rect>();
            foreach (Sprite item in sprs)
                rects.Add(item.imageRect);


            Rect retRc = new Rect();
            Rect[] ret = RectSpliter.Spliter(rects.ToArray(), packSize, 1, ref retRc, packSize, packSize, false, true, 4);


            if (ret != null)
            {
                if (ret.Length > 0)
                {

                    Texture2D tex = new Texture2D(packSize, packSize, TextureFormat.ARGB32, mipmap);
                    Color32 c = new Color32(0,0,0,0);
                    Color32[] _buf = tex.GetPixels32();
                    for (int ci = 0; ci < _buf.Length; ci++)
                        _buf[ci] = c;
                    tex.SetPixels32(_buf);


                    int i = 0;
                    foreach (Rect rc in ret)
                    {
                        Sprite spr = sprs[i];

                        Rect src = new Rect(
                            spr.imageRect.x,
                            spr.image.height - spr.imageRect.y - spr.imageRect.height,
                            spr.imageRect.width, spr.imageRect.height);

                        Rect dst = new Rect(
                            rc.x,
                            tex.height - rc.y - spr.imageRect.height,
                            spr.imageRect.width, spr.imageRect.height
                            );

                        Color[] buf = spr.image.GetPixels((int)src.x, (int)src.y, (int)src.width, (int)src.height);
                        tex.SetPixels((int)dst.x, (int)dst.y, (int)dst.width, (int)dst.height, buf);


                        spr.image = tex;
                        spr.atlas = null;
                        spr.imageRect = new Rect(rc.x, rc.y, spr.imageRect.width, spr.imageRect.height);

                        spr.Init();

                        i++;
                    }

                    tex.Apply();

                    return true;
                }
            }

            return false;
        }
    }












    /// <summary>
    /// Sprite's owner.
    /// </summary>
    public class SpriteGroup : ScriptableObject
    {
        /// <summary>
        /// All sprites in group
        /// </summary>
        public Sprite[] frames = new Sprite[]{};

        ///// <summary>
        ///// Group's parent asset.
        ///// </summary>
        //public SpriteAsset parent;


        /// <summary>
        /// Group's parent asset.
        /// Usually use in edit time.
        /// Will return null at runtime.
        /// </summary>
        public SpriteAsset parent
        {
            get
            {
                return Application.isEditor && Application.isPlaying ?
                    null :
                    SpriteAsset.GetByID(parentID);
            }

            set
            {
                if (value != null)
                {
                    parentName = value.name;
                    parentID = value.GetInstanceID();
                }
                else
                {
                    parentName = string.Empty;
                    parentID = 0;
                }
            }
        }

        [HideInInspector]
        [SerializeField]
        internal string parentName = string.Empty;
        internal int parentID;

        /// <summary>
        /// Internal data. You do not to need use this.
        /// </summary>
        [HideInInspector]
        public bool isExpand = true;


        /// <summary>
        /// The name of group.
        /// </summary>
        [HideInInspector]
        public string groupName = "";



        void OnEnable()
        {
            
            foreach (Sprite spr in frames)
            {
                spr.parentName = name;
                spr.parentID = this.GetInstanceID();
            }
        }


        internal static SpriteGroup GetByID(int id)
        {
            Object[] groups = Resources.FindObjectsOfTypeAll(typeof(SpriteGroup));

            if (groups.Length > 0)
            {
                foreach (SpriteGroup group in groups)
                {
                    if (group.GetInstanceID() == id)
                        return group;
                }
            }

            return null;
        }
    }













    /// <summary>
    /// SpriteGroup's owner.
    /// </summary>
    public class SpriteAsset : ScriptableObject
    {
        /// <summary>
        /// Groups in the asset.
        /// </summary>
        public SpriteGroup[] spriteGroups = new SpriteGroup[] { };


        /// <summary>
        /// reserved.
        /// </summary>
        [HideInInspector]
        public bool isExpand = true;


        public bool showInViewer = true;

        void OnEnable()
        {
            
            foreach (SpriteGroup group in spriteGroups)
            {
                group.parentName = name;
                group.parentID = this.GetInstanceID();
            }
        }

        internal static SpriteAsset GetByID(int id)
        {
            Object[] assets = Resources.FindObjectsOfTypeAll(typeof(SpriteAsset));

            if (assets.Length > 0)
            {
                foreach (SpriteAsset asset in assets)
                {
                    if (asset.GetInstanceID() == id)
                        return asset;
                }
            }

            return null;
        }
    }



    /// <summary>
    /// An atlas with a image of sprites image packed
    /// </summary>
    public class SpriteAtlasAsset : ScriptableObject
    {
        /// <summary>
        /// All Sprites link with the atlas.
        /// </summary>
        public Sprite[] sprites = new Sprite[]{};

        /// <summary>
        /// The image of atlas.
        /// </summary>
        public Texture2D texture = null;

        
    }
}