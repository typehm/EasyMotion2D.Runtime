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

            uint id = GetTextureID(tex);
            if (id == 0)
            {
                id = guid++;
                texs.Add(tex, id);

                if ( id > (1 << 7) )
                    Debug.LogError("Error, Sprite used Texture2D counts greater than 127. You need Combine some texture to keep it less than 127");
            }
            return id;
        }

        public uint GetTextureID(Texture2D tex)
        {
            if (tex == null) return 0;
            if (texs.ContainsKey(tex))
                return (uint)texs[tex];
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
        /// </summary>
        public SpriteGroup parent = null;

        /// <summary>
        /// The image texture path of sprite linked.
        /// </summary>
        public string linkedTexturePath = "";
        /// <summary>
        /// Sprite image block in image.
        /// </summary>
        public Rect linkedImageRect;

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

        void OnEnable()
        {
            Init();
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
            float w = 1f / image.width;
            float h = 1f / image.height;
            float xmin = (imageRect.xMin + x) * w;
            float ymin = 1f - (imageRect.yMin + y) * h;

            return new Vector2(xmin, ymin);
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

        /// <summary>
        /// Group's parent asset.
        /// </summary>
        public SpriteAsset parent;


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