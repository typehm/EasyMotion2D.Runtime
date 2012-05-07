using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyMotion2D
{
    /// <summary>
    /// Bitmap font asset
    /// </summary>
    [Serializable]
    public class BitmapFont : ScriptableObject
    {
        /// <summary>
        /// Holds information on how the <see cref="BitmapFont">bitmap font</see> was generated.
        /// </summary>
        public FontInfo info = new FontInfo();

        /// <summary>
        /// Holds information common to all characters
        /// </summary>
        public FontCommon common = new FontCommon();

        /// <summary>
        /// Holds all textures the <see cref="BitmapFont">bitmap font</see> used.
        /// </summary>
        public FontPage[] pages = new FontPage[]{};

        /// <summary>
        /// Holds all included character in the <see cref="BitmapFont">bitmap font</see>.
        /// </summary>
        public FontChar[] chars = new FontChar[]{};

        /// <summary>
        /// Holds all kerning information is used to adjust the distance between certain characters
        /// </summary>
        public FontKerning[] kernings = new FontKerning[]{};


        private int[] _chars = null;

        void OnEnable()
        {            
            foreach (FontPage page in pages)
                page.texid = TextureGUIDManager.instance.RegisterTexture(page.texture);

            InitFindTable();
        }

        void InitFindTable()
        {
            if (_chars == null || _chars.Length == 0)
            {
                _chars = new int[chars.Length];
                for (int i = 0, e = chars.Length; i < e; i++)
                {
                    FontChar ch = chars[i];
                    _chars[i] = chars[i].id;
                }
            }
        }


        /// <summary>
        /// Get a char's information in the <see cref="BitmapFont">bitmap font</see>.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public FontChar GetChar(char c)
        {
            if (_chars == null || _chars.Length == 0)
            {
                InitFindTable();
                if (_chars == null || _chars.Length == 0)
                    return null;
            }

            int i = Array.BinarySearch<int>(_chars, (int)c);
            //int i = BinarySearch(_chars, c);
            return (i < 0) ? null : chars[i];        
        }


        //instead Array.BinarySearch
        static int BinarySearch(int[] array, int des)
        {
            int low = 0;
            int high = array.Length - 1;

            while (low <= high)
            {
                int middle = (high + low) >> 1;
                int mv = array[middle];
                if (des == mv)
                {
                    return middle;
                }
                else if (des < mv)
                {
                    high = middle - 1;
                }
                else
                {
                    low = middle + 1;
                }
            }

            return -1;
        }


        /// <summary>
        /// Get the kerning information is used to adjust the distance between certain characters
        /// </summary>
        public int GetKerning(char l, char r)
        {
            if (l == 0)
                return 0;

            for (int i = 0, e = kernings.Length; i < e; i++)
            {
                if ( kernings[i].first == l && kernings[i].second == r)
                    return kernings[i].amount;
            }

            return 0;
        }
    }



    /// <summary>
    /// FontInfo holds information on how the <see cref="BitmapFont">bitmap font</see> was generated.
    /// </summary>
    [Serializable]
    public class FontInfo
    {
        /// <summary>
        /// The name of the true type font.
        /// </summary>
        public string face;

        /// <summary>
        /// The size of the true type font.
        /// </summary>
        public int size;

        /// <summary>
        /// The font is bold.
        /// </summary>
        public bool bold;

        /// <summary>
        /// The font is italic.
        /// </summary>
        public bool italic;

        /// <summary>
        /// The name of the OEM charset used (when not unicode).
        /// </summary>
        public string charSet;

        /// <summary>
        /// Is the unicode charset.
        /// </summary>
        public bool unicode;

        /// <summary>
        /// The font height stretch in percentage. 100% means no stretch.
        /// </summary>
        public int stretchHeight;

        /// <summary>
        /// Is smoothing was turned on.
        /// </summary>
        public bool smooth;

        /// <summary>
        /// The supersampling level used. 1 means no supersampling was used.
        /// </summary>
        public int superSampling;

        /// <summary>
        /// The padding for each character (up, right, down, left).
        /// </summary>
        public Rect padding;


        /// <summary>
        /// The spacing for each character (horizontal, vertical).
        /// </summary>
        public Vector2 spacing;

        /// <summary>
        /// The outline thickness for the characters.
        /// </summary>
        public int outLine;
    }


    /// <summary>
    /// FontCommon holds information common to all characters
    /// </summary>
    [Serializable]
    public class FontCommon
    {
        /// <summary>
        /// This is the distance in pixels between each line of text.
        /// </summary>
        public int lineHeight;

        /// <summary>
        /// The number of pixels from the absolute top of the line to the base of the characters.
        /// </summary>
        public int baseHeight;

        /// <summary>
        /// The width of the texture, normally used to scale the x pos of the character image.
        /// </summary>
        public int scaleW;

        /// <summary>
        /// The height of the texture, normally used to scale the y pos of the character image.
        /// </summary>
        public int scaleH;

        /// <summary>
        /// The number of texture pages included in the font.
        /// </summary>
        public int pages;

        /// <summary>
        /// Set to true if the monochrome characters have been packed into each of the texture channels. In this case alphaChnl describes what is stored in each channel.
        /// </summary>
        public bool packed;

        /// <summary>
        /// Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
        /// </summary>
        public int alphaChannel;

        /// <summary>
        /// Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
        /// </summary>
        public int redChannel;

        /// <summary>
        /// Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
        /// </summary>
        public int greenChannel;

        /// <summary>
        /// Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
        /// </summary>
        public int blueChannel;
    }


    /// <summary>
    /// FontPage gives the name of a texture.
    /// </summary>
    [Serializable]
    public class FontPage
    {
        /// <summary>
        /// The page id.
        /// </summary>
        public int id;

        /// <summary>
        /// The texture's reference.
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// Internal texture id. You do not need to use this.
        /// </summary>
        [HideInInspector]
        public uint texid;
    }



    /// <summary>
    /// FontChar describes on character in the <see cref="BitmapFont">bitmap font</see>.
    /// </summary>
    [Serializable]
    public class FontChar
    {
        /// <summary>
        /// The character id.
        /// </summary>
        public int id;

        /// <summary>
        /// The left position of the character image in the texture.
        /// </summary>
        public int x;

        /// <summary>
        /// The top position of the character image in the texture.
        /// </summary>
        public int y;

        /// <summary>
        /// The width of the character image in the texture.
        /// </summary>
        public int width;

        /// <summary>
        /// The height of the character image in the texture.
        /// </summary>
        public int height;

        /// <summary>
        /// How much the current position should be offset when copying the image from the texture to the screen.
        /// </summary>
        public int xOffset;

        /// <summary>
        /// How much the current position should be offset when copying the image from the texture to the screen.
        /// </summary>
        public int yOffset;

        /// <summary>
        /// How much the current position should be advanced after drawing the character.
        /// </summary>
        public int xAdvance;

        /// <summary>
        /// The texture page where the character image is found.
        /// </summary>
        public int page;

        /// <summary>
        /// The texture channel where the character image is found (1 = blue, 2 = green, 4 = red, 8 = alpha, 15 = all channels).
        /// </summary>
        public int channel;

        /// <summary>
        /// Sprite of char
        /// </summary>
        public Sprite sprite;
    }

    internal class FontCharComparer : IComparer<FontChar>
    {
        public int Compare(FontChar lhs, FontChar rhs)
        {
            return lhs.id - rhs.id;
        }

        public static FontCharComparer comparer = new FontCharComparer();
    }


    /// <summary>
    /// FontKerning is used to adjust the distance between certain characters
    /// </summary>
    [Serializable]
    public class FontKerning
    {
        /// <summary>
        /// The first character id.
        /// </summary>
        public int first;

        /// <summary>
        /// The second character id.
        /// </summary>
        public int second;


        /// <summary>
        /// How much the x position should be adjusted when drawing the second character immediately following the first.
        /// </summary>
        public int amount;
    }













    /// <summary>
    /// A text string displayed use <see cref="BitmapFont">bitmap font</see>.
    /// </summary>
    [ExecuteInEditMode]
    public class BitmapFontTextRenderer : SpriteRenderer
    {
        /// <summary>
        /// The text to display.
        /// </summary>
        public string text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
                updateText(_text);
            }
        }

        [SerializeField]
        private string _text;

        /// <summary>
        /// The font used for the text.
        /// </summary>
        public BitmapFont font;


        /// <summary>
        /// How lines of text are aligned (Left, Right, Center)
        /// </summary>
        public TextAlignment textAlignment = TextAlignment.Left;

        /// <summary>
        /// The character's left top color.
        /// </summary>
        public Color colorLT
        {
            get
            {
                return __colorLT;
            }

            set
            {
                _colorLT = __colorLT = value;
            }
        }


        /// <summary>
        /// The character's right top color.
        /// </summary>
        public Color colorRT
        {
            get
            {
                return __colorRT;
            }

            set
            {
                _colorLT = __colorRT = value;
            }
        }

        /// <summary>
        /// The character's left bottom color.
        /// </summary>
        public Color colorLB
        {
            get
            {
                return __colorLB;
            }

            set
            {
                _colorLT = __colorLB = value;
            }
        }

        /// <summary>
        /// The character's right bottom color.
        /// </summary>
        public Color colorRB
        {
            get
            {
                return __colorRB;
            }

            set
            {
                _colorLT = __colorRB = value;
            }
        }

        [SerializeField]
        protected Color _colorLT = Color.white;
        [SerializeField]
        protected Color _colorRT = Color.white;
        [SerializeField]
        protected Color _colorLB = Color.white;
        [SerializeField]
        protected Color _colorRB = Color.white;


        /// <summary>
        /// Display text in a area
        /// </summary>
        public bool textArea = false;

        /// <summary>
        /// Display text area size.
        /// </summary>
        public Vector2 textAreaSize = new Vector2(100, 50);

        new void OnEnable()
        {
            
            base.OnEnable();
            text = text;
        }

        List<SpriteTransform> textSpriteTransforms = new List<SpriteTransform>();

        void updateText(string text)
        {
            if (font == null || !enabled || renderMode == null || !gameObject.active)
                return;

            float offsetX = 0f;
            float offsetY = 0f;

            char lastChar = (char)0;

            base.Clear();

            if (text.Length == 0)
                return;

            __colorLT = _colorLT;
            __colorRT = _colorRT;
            __colorLB = _colorLB;
            __colorRB = _colorRB;


            base.Resize( text.Length * 2);

            textSpriteTransforms.Clear();

 
            int e = text.Length;
            for (int i = 0;  i < e; i++)
            {
                char c = text[i];

                if (textArea && (c == '\n') || (c == '\r'))
                {
                    alignText(offsetX);

                    offsetX = 0;
                    offsetY -= font.common.lineHeight;
                }


                if (textArea && offsetY < -textAreaSize.y)
                {
                    break;
                }


                FontChar fontChar = font.GetChar(c);
                if (fontChar == null)
                {
                    continue;
                }


                int k = font.GetKerning(lastChar, c);


                if (textArea && (( (offsetX + fontChar.xOffset + k) >= textAreaSize.x) ) )
                {
                    alignText(offsetX);

                    offsetX = 0;
                    offsetY -= font.common.lineHeight;
                }

                int idx = AttachSprite(fontChar.sprite);
                SpriteTransform tran = GetSpriteTransform(idx);
                
                textSpriteTransforms.Add(tran);

                tran.position = new Vector2(offsetX + fontChar.xOffset + k, offsetY - fontChar.yOffset);
                tran.layer = (e - i) & 0xf;


                offsetX += fontChar.xAdvance;
                lastChar = c;
            }

            alignText(offsetX);

            if (updateMode == SpriteRendererUpdateMode.None)
            {
                Apply();
            }
        }

        void alignText(float xOffset)
        {
            if (!textArea)
                return;

            float offset = 0f;

            if (textAlignment == TextAlignment.Center)
            {
                offset = ((textAreaSize.x - xOffset) * 0.5f);
            }

            if (textAlignment == TextAlignment.Right)
            {
                offset = (textAreaSize.x - xOffset);
            }


            foreach (SpriteTransform sprTran in textSpriteTransforms)
                sprTran.position.x += offset;


            textSpriteTransforms.Clear();
        }

        static Vector3[] vecBuf = new Vector3[4];

        void OnDrawGizmosSelected()
        {
            if (textArea)
            {
                vecBuf[0] = transform.TransformPoint(new Vector3( 0 , 0, 0));
                vecBuf[1] = transform.TransformPoint(new Vector3( textAreaSize.x, 0, 0));
                vecBuf[2] = transform.TransformPoint(new Vector3( textAreaSize.x, -textAreaSize.y, 0));
                vecBuf[3] = transform.TransformPoint(new Vector3( 0, -textAreaSize.y, 0));

                drawBoundingBox(vecBuf, Color.blue );
            }
        }
    }


























    /// <summary>
    /// Bind a GameObject to a Camera's local space
    /// </summary>
    [ExecuteInEditMode]
    public class GUIElement : MonoBehaviour
    {
        /// <summary>
        /// A Camera2D gameObject you want bind to
        /// </summary>
        public Camera2D viewCamera;

        /// <summary>
        /// The position in camera's local space
        /// </summary>        
        public Vector2 position;

        /// <summary>
        /// Where to update gameObject's position
        /// </summary>
        public SpriteRendererUpdateMode updateMode;

        void Apply()
        {
            gameObject.transform.position = viewCamera.transform.TransformPoint(position);
        }


        void Update()
        {
            if (updateMode == SpriteRendererUpdateMode.Update)
                Apply();
        }

        void LateUpdate()
        {
            if (updateMode == SpriteRendererUpdateMode.LateUpdate)
                Apply();
        }

        void FixedUpdate()
        {
            if (updateMode == SpriteRendererUpdateMode.FixedUpdate)
                Apply();
        }
    }
}
