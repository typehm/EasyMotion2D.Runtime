using UnityEngine;
using System.Collections;
using System.Collections.Generic;





namespace EasyMotion2D
{
    /// <summary>
    /// Render mode setting. 
    /// </summary>
    public class SpriteRenderModeSetting : ScriptableObject
    {
        /// <summary>
        /// RenderMode type. Should be unique in project.
        /// </summary>
        public SpriteRenderMode renderMode = SpriteRenderMode.None;

        /// <summary>
        /// Shader used by RenderMode
        /// </summary>
        public Shader shader = null;

        /// <summary>
        /// Is shader need to sort by depth? Use for semi-transparent shader.
        /// </summary>
        public bool sortByDepth = false;

        [HideInInspector]
        [SerializeField]
        public List<Material> materials = new List<Material>();


        void OnEnable()
        {
            Clear();
            SpriteMaterialManager.RegisterRenderMode(this);
        }

        void OnDisable()
        {
            Clear();
        }

        void Clear()
        {
            foreach (Material mat in materials)
            {
                if (mat != null && mat)
                {
                    if (Application.isPlaying)
                        Object.Destroy(mat);
                    else
                        Object.DestroyImmediate(mat);
                }
            }
            materials.Clear();
        }
    }



    /// <summary>
    /// This is used to decide how the camera rendering result to draw to screen.
    /// </summary>
    public enum Camera2DStretchMode
    {
        None,
        StretchFit,
        AspectStretchFit,
    }



    /// <summary>
    /// A Camera2D is a camera through which the player views the world.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Camera2D : MonoBehaviour
    {
        /// <summary>
        /// The width of the target screen window in pixels.
        /// </summary>
        public float screenWidth = 800;

        /// <summary>
        /// The height of the target screen window in pixels.
        /// </summary>
        public float screenHeight = 600;

        /// <summary>
        /// Camera's half-size in orthographic mode
        /// </summary>
        public float orthographicSize
        {
            get
            {
                return _orthographicSize;
            }

            set
            {
                _orthographicSize = value;

                float aspect = (float)screenWidth / (float)screenHeight;

                screenHeight = 2f * _orthographicSize;
                screenWidth = screenHeight * aspect;
            }
        }

        [SerializeField]
        private float _orthographicSize = 300;

        /// <summary>
        /// This is used to select gameObjects in scene who will be render.
        /// </summary>
        public LayerMask cullingMask;

        /// <summary>
        /// Auto adjust screenwidth with Screen's aspect.
        /// </summary>
        public bool autoAdjustAspect = false;

        /// <summary>
        /// This is used to clipping sprite if not in the bounding sphere of Camera2D.
        /// </summary>
        public bool clipping = true;

        /// <summary>
        /// This is used to decide how the camera rendering result to draw to screen.
        /// </summary>
        public Camera2DStretchMode stretchMode = Camera2DStretchMode.StretchFit;

        /// <summary>
        /// Clear background color.
        /// </summary>
        public Color backgroundColor = Color.black;

        public bool autoClearScreen = false;

        public int maxPrimitiveCount = 2048;

        public float adjustScreenWidth = 0f;


        [HideInInspector]
        [SerializeField]
        private SpriteBatchRenderer spriteMeshRenderer;



        void Reset()
        {
            uint i = 0xffff0000;
            cullingMask.value = (int)i;

            if (spriteMeshRenderer == null)
            {
                GameObject obj = new GameObject(name + ".SpriteBatchRenderer");
                obj.AddComponent<SpriteBatchRenderer>();
                spriteMeshRenderer = obj.GetComponent<SpriteBatchRenderer>();
                spriteMeshRenderer.gameObject.hideFlags = HideFlags.NotEditable;
                //spriteMeshRenderer.parent = this;
            }
        }



        void OnDestroy()
        {
            if (spriteMeshRenderer != null && spriteMeshRenderer)
            {
                if (Application.isPlaying)
                    Object.Destroy(spriteMeshRenderer.gameObject);
                else
                    Object.DestroyImmediate(spriteMeshRenderer.gameObject);
            }

            SpriteManager.Clear();
        }


        private bool isOpenGL = false;
        void Awake()
        {
        }


        bool needClear = false;
        bool isClearNow = false;

        void Update()
        {
            if ( autoClearScreen )
                clearScreen();
        }

        private void clearScreen()
        {
            if ( needClear && !isClearNow )
            {
                if (Screen.width <= 0f || Screen.height <= 0f || Application.isEditor)
                    return;

                isClearNow = true;
                Rect tmpRc = camera.pixelRect;
                CameraClearFlags flag = camera.clearFlags;
                Color tmp = camera.backgroundColor;

                camera.pixelRect = new Rect(0f, 0f, Screen.width, Screen.height);
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = backgroundColor;
                
                camera.Render();

                camera.cullingMask = cullingMask;
                camera.clearFlags = flag;
                camera.pixelRect = tmpRc;
                camera.backgroundColor = tmp;

                isClearNow = false;
            }
        }


        void resetCamera()
        {
            orthographicSize = screenHeight * 0.5f;

            camera.orthographic = true;
            camera.orthographicSize = screenHeight * 0.5f;
            camera.aspect = (float)screenWidth / (float)screenHeight;
            camera.cullingMask = cullingMask;


            if ( spriteMeshRenderer != null )
                spriteMeshRenderer.cullingMask = cullingMask;


            int mask = 1;
            int i = 0;
            while (i < 32)
            {
                if ((cullingMask.value & (mask << i)) != 0)
                {
                    if (spriteMeshRenderer != null)
                        spriteMeshRenderer.gameObject.layer = i;
                }

                i++;
            }



            float hw = screenWidth * 0.5f;
            float hh = screenHeight * 0.5f;


            if (autoAdjustAspect)
            {
                adjustScreenWidth = screenHeight * ((float)Screen.width / Screen.height);
                hw = adjustScreenWidth * 0.5f;
                stretchMode = Camera2DStretchMode.StretchFit;
            }
            else
                adjustScreenWidth = screenWidth;


            if (isOpenGL)
                camera.projectionMatrix = Matrix4x4.Ortho(-hw, hw, -hh, hh, 0.0f, 1024f);
            else
            {
                //if ( stretchMode == Camera2DStretchMode.None )
                    camera.projectionMatrix = Matrix4x4.Ortho(-hw + 0.5f, hw + 0.5f, -hh - 0.5f, hh - 0.5f, -0.01f, 1024f);
                //else
                    //camera.projectionMatrix = Matrix4x4.Ortho(-hw - 0.5f, hw - 0.5f, -hh + 0.5f, hh + 0.5f, 0.0f, 1024f);
            }



            if (Screen.width <= 0f || Screen.height <= 0f)
                return;


            if (stretchMode == Camera2DStretchMode.None)
            {
                camera.pixelRect = new Rect((Screen.width - screenWidth) * 0.5f, (Screen.height - screenHeight) * 0.5f, screenWidth, screenHeight);
                needClear = true;
            }

            if (stretchMode == Camera2DStretchMode.StretchFit)
            {
                camera.pixelRect = new Rect(0f, 0f, Screen.width, Screen.height);
            }

            if (stretchMode == Camera2DStretchMode.AspectStretchFit)
            {
                float cameraAspect = (float)screenWidth / (float)screenHeight;
                float screenAspect = (float)Screen.width / (float)Screen.height;

                if (screenAspect >= cameraAspect)
                {
                    float h = Screen.height;
                    float w = Screen.height * cameraAspect;
                    camera.pixelRect = new Rect( (Screen.width - w) * 0.5f, 0f, w, h);
                }
                else
                {
                    float w = Screen.width;
                    float h = w * ((float)screenHeight / (float)screenWidth);
                    camera.pixelRect = new Rect( 0, (Screen.height - h) * 0.5f, w, h);
                }
                needClear = true;
            }
        }

        public void Render()
        {
            float radius = new Vector2( screenWidth * 0.5f, screenHeight * 0.5f).magnitude;
            spriteMeshRenderer.CommitToRender(clipping, transform.position, radius);
        }


        void OnPreRender()
        {
            //clearScreen();

            if (!isClearNow)
            {
                resetCamera();

                Render();
            }
        }

        void OnDrawGizmos()
        {
            if (Application.isEditor || !Application.isPlaying)
            {
                Render();
            }
        }

        void OnDrawGizmosSelected()
        {
            float radius = new Vector2(screenWidth, screenHeight).magnitude * 0.5f;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        void OnEnable()
        {
            if (autoAdjustAspect)
                adjustScreenWidth = screenHeight * ((float)Screen.width / Screen.height);
            else
                adjustScreenWidth = screenWidth;



            if (spriteMeshRenderer != null)
            {
                spriteMeshRenderer.gameObject.active = true;
                spriteMeshRenderer.enabled = true;
            }

            isOpenGL = SystemInfo.graphicsDeviceName.ToUpper().IndexOf("OPENGL") >= 0;
            resetCamera();
        }

        void OnDisable()
        {
            if (spriteMeshRenderer != null)
                spriteMeshRenderer.enabled = false;
        }


        void SetMaxPrimitiveCount()
        {
            spriteMeshRenderer.maxPrimitiveCount = maxPrimitiveCount;
        }
    }



}