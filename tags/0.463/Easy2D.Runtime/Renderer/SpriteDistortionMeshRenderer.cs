using UnityEngine;
using System.Collections;




namespace EasyMotion2D
{
    //internal class MeshVertex
    //{
    //    public Vector3 orig;
    //    public Vector3 pos;
    //    public Vector2 uv;
    //    public Color color = Color.white;
    //}


    //[ExecuteInEditMode]
    //public class SpriteDistorionMeshRenderer : MonoBehaviour//, StateUpdater
    //{
    //    public SpriteRenderModeSetting renderMode = null;

    //    public SpriteAnchor anchor = SpriteAnchor.UpperLeft;

    //    [HideInInspector]
    //    public Sprite currentFrame = null;

    //    public bool visible
    //    {
    //        get { return _visible; }
    //        set { _visible = value;
    //            primitive.visible = _visible;
    //        //if (enabled)
    //        //    if (_visible)
    //        //        primitive.AddToLayer(gameObject.layer);
    //        //    else
    //        //        primitive.RemoveFromLayer(gameObject.layer);            
    //        }
    //    }


    //    public int cols = 1;
    //    public int rows = 1;
    //    public float cellWidth = 1;
    //    public float cellHeight = 1;


    //    public Vector2 hotSpot = Vector2.zero;

    //    public float z = 0f;
    //    public bool useTransformZ = true;
    //    public bool autoUpdate = true;

    //    [SerializeField]
    //    private bool _visible = true;



    //    private SpritePrimitive primitive = null;
    //    private MeshVertex[] vertices = new MeshVertex[] { };


    //    private GameObject ownerGameObject = null;
    //    private Transform ownerTransform = null;
    //    //

    //    private int currentFrameID = 0;
    //    private int lastFrameID = 0;


    //    private SpriteRenderMode lastRenderMode = SpriteRenderMode.None;
    //    private int lastLayer = 0;
    //    private SpriteAnchor lastAnchor;
    //    private Vector2 lastHotSpot = Vector2.zero;




    //    public void SetFrame(Sprite frame, bool isVaildSprite)
    //    {
    //        if (isVaildSprite && frame.instanceID != currentFrameID && lastFrameID != frame.instanceID)
    //        {
    //            currentFrame = frame;
    //            lastFrameID = currentFrameID = frame.instanceID;

    //            primitive.texture = currentFrame.image;
    //            primitive.texId = currentFrame.texID;

    //            initUV(currentFrame.image, currentFrame.texWidth, currentFrame.texHeight, currentFrame.imageRect);
    //            initVertice(currentFrame.imageRect);


    //            cellWidth = currentFrame.imageRect.width / cols;
    //            cellHeight = currentFrame.imageRect.height / rows;

    //            return;
    //        }


    //        if (isVaildSprite)
    //            lastFrameID = currentFrameID = frame.instanceID;
    //        else
    //        {
    //            currentFrame = null;
    //            lastFrameID = currentFrameID = 0;

    //            primitive.texture = null;
    //            primitive.texId = 0;
    //        }
    //    }



    //    void SetAnchor(SpriteAnchor am)
    //    {
    //        anchor = am;
    //        lastAnchor = anchor;
    //        lastHotSpot = hotSpot;

    //        if (currentFrame != null)
    //            initVertice(currentFrame.imageRect);
    //    }


    //    bool isInit = false;

    //    void init()
    //    {
    //        ownerGameObject = gameObject;
    //        ownerTransform = transform;

    //        lastLayer = ownerGameObject.layer;

    //        initCell();

    //        SetFrame(currentFrame, currentFrame != null);
    //        SetAnchor(anchor);
    //        primitive.AddToLayer(gameObject.layer);

    //        isInit = true;

    //        UpdateSprite();
    //    }


    //    public void initCell()
    //    {
    //        if (primitive != null)
    //            primitive.RemoveFromLayer(gameObject.layer);

    //        primitive = new SpritePrimitive(cols * rows);
    //        vertices = new MeshVertex[ (cols+1) * (rows+1) ];

    //        for (int i = 0; i < vertices.Length; i++)
    //        {
    //            MeshVertex vec = new MeshVertex();
    //            vertices[i] = vec;
    //        }

    //        primitive.ownerID = gameObject.GetInstanceID() ;
    //        primitive.SetColor( Color.white);
    //    }


    //    void initUV(Texture2D tex, float width, float height, Rect rc)
    //    {
    //        float w = 1f / width;
    //        float h = 1f / height;

    //        float xmin = rc.xMin * w;
    //        float ymin = 1f - rc.yMin * h;

    //        float xs = rc.width * w  / cols;
    //        float ys = rc.height * h / rows;

    //        int i = 0;

    //        float cy = ymin;
    //        float cx = xmin;

    //        for (int y = 0; y <= rows; y++)
    //        {
    //            for (int x = 0; x <= cols; x++)
    //            {
    //                vertices[i].uv.x = cx;
    //                vertices[i].uv.y = cy;

    //                cx += xs;
    //                i++;
    //            }
    //            cy -= ys;
    //            cx = xmin;
    //        }
    //    }



    //    Rect initVerticeRc = new Rect();
    //    void initVertice(Rect rc)
    //    {
    //        SpriteAnchorUtility.AlignRectToPoint(rc, anchor, hotSpot.x, hotSpot.y, ref initVerticeRc);

    //        float xs = initVerticeRc.width / cols;
    //        float ys = initVerticeRc.height / rows;

    //        float cy = initVerticeRc.yMax;
    //        float cx = initVerticeRc.xMin;

    //        int i = 0;
    //        for (int y = 0; y <= rows; y++)
    //        {
    //            for (int x = 0; x <= cols; x++)
    //            {
    //                vertices[i].pos.x = cx;
    //                vertices[i].pos.y = cy;

    //                vertices[i].orig.x = cx;
    //                vertices[i].orig.y = cy;

    //                cx += xs;
    //                i++;
    //            }
    //            cy -= ys;
    //            cx = initVerticeRc.xMin;
    //        }

    //        transformMesh( true, true, false);
    //    }

    //    void transformMesh( bool pos, bool color, bool uv )
    //    {
    //        if (pos)
    //        {
    //            float z = useTransformZ ? ownerTransform.position.z : this.z;
    //            primitive.z = z;
    //        }


    //        int syBase = 0;
    //        int eyBase = 0;

    //        Vector3 tranPos = ownerTransform.position;

    //        int i = 0;
    //        for (int y = 0; y < rows; y++)
    //        {
    //            syBase = y * (cols + 1);
    //            eyBase = (y + 1) * (cols + 1);
    //            for (int x = 0; x < cols; x++)
    //            {
    //                if (pos)
    //                {
    //                    primitive.position[i + 0] = vertices[syBase + x].pos + tranPos;      //LT
    //                    primitive.position[i + 1] = vertices[syBase + x + 1].pos + tranPos;  //RT
    //                    primitive.position[i + 2] = vertices[eyBase + x + 1].pos + tranPos;  //RB
    //                    primitive.position[i + 3] = vertices[eyBase + x].pos + tranPos;      //LB
    //                }

    //                if (uv)
    //                {
    //                    primitive.uv[i + 0] = vertices[syBase + x].uv;
    //                    primitive.uv[i + 1] = vertices[syBase + x + 1].uv;
    //                    primitive.uv[i + 2] = vertices[eyBase + x + 1].uv;
    //                    primitive.uv[i + 3] = vertices[eyBase + x].uv;
    //                }

    //                if (color)
    //                {
    //                    primitive.color[i + 0] = vertices[syBase + x].color;
    //                    primitive.color[i + 1] = vertices[syBase + x + 1].color;
    //                    primitive.color[i + 2] = vertices[eyBase + x + 1].color;
    //                    primitive.color[i + 3] = vertices[eyBase + x].color;
    //                }

    //                i+=4;
    //            }
    //        }
    //    }




    //    static Vector3 vecOne = Vector3.one;
    //    Vector3 lastTransformed = Vector3.zero;

    //    void updatePrimitives()
    //    {
    //        ////    //commit to render

    //        primitive.renderMode = renderMode.renderMode;

    //        Vector3 tmp = ownerTransform.TransformPoint(vecOne);
    //        //if (tmp != lastTransformed)
    //        {
    //            transformMesh( true, true, true);
    //            lastTransformed = tmp;
    //        }
    //    }





    //    public void UpdateSprite()
    //    {
    //        if (!visible)
    //            return;

    //        if (lastFrameID != currentFrameID)
    //            SetFrame(currentFrame, currentFrame != null);

    //        if (lastAnchor != anchor)
    //            SetAnchor(anchor);

    //        if (anchor == SpriteAnchor.HotSpot && lastHotSpot != hotSpot)
    //            SetAnchor(anchor);

    //        int tLayer = ownerGameObject.layer;
    //        if (lastLayer != tLayer)
    //        {
    //            primitive.RemoveFromLayer(lastLayer);
    //            primitive.AddToLayer(ownerGameObject.layer);
    //        }


    //        if (currentFrameID != 0)
    //            updatePrimitives();

    //        lastLayer = tLayer;
    //    }



    //    void OnDrawGizmos()
    //    {
    //        Gizmos.DrawIcon(transform.position, "");
    //    }


    //    void Reset()
    //    {
    //        Object[] objs = GameObject.FindObjectsOfTypeIncludingAssets(typeof(SpriteRenderModeSetting));
    //        if (objs != null && objs.Length > 0)
    //        {
    //            foreach (SpriteRenderModeSetting setting in objs)
    //            {
    //                if (setting.renderMode == SpriteRenderMode.AlphaBlend)
    //                {
    //                    renderMode = setting;
    //                    return;
    //                }
    //            }
    //            renderMode = objs[0] as SpriteRenderModeSetting;
    //        }
    //    }


    //    //if enable add sprite to render queue
    //    void OnEnable()
    //    {
    //        if (!isInit)
    //            init();

    //        //primitive.AddToLayer(gameObject.layer);
    //        visible = true;
    //    }

    //    //if enable remove sprite to render queue
    //    void OnDisable()
    //    {
    //        //primitive.RemoveFromLayer(gameObject.layer);
    //        visible = false;
    //    }


    //    public void UpdateState()
    //    {
    //        UpdateSprite();
    //    }

    //    public bool IsSelfUpdate()
    //    {
    //        return !autoUpdate;
    //    }

    //    public bool IsVisible(Vector2 pos, float radius)
    //    {
    //        return true;
    //    }

    //    public void Clear(Color col)
    //    {
    //        foreach (MeshVertex ver in vertices)
    //        {
    //            ver.pos = ver.orig;
    //            ver.color = col;
    //        }
    //    }


    //    public void SetColor( int cols, int rows, Color col)
    //    {
    //        if (cols <= this.cols && rows <= this.rows)
    //        {
    //            vertices[cols + rows * (this.cols + 1)].color = col;
    //        }
    //    }


    //    public Color GetColor(int cols, int rows)
    //    {
    //        if (cols <= this.cols && rows <= this.rows)
    //        {
    //            return vertices[cols + rows * (this.cols + 1)].color;
    //        }
    //        return Color.red;
    //    }

    //    public void SetUV(int cols, int rows, Vector2 uv)
    //    {
    //        if (cols <= this.cols && rows <= this.rows)
    //        {
    //            vertices[cols + rows * (this.cols + 1)].uv = uv;
    //        }
    //    }


    //    public Vector2 GetUV(int cols, int rows)
    //    {
    //        if (cols <= this.cols && rows <= this.rows)
    //        {
    //            return vertices[cols + rows * (this.cols + 1)].uv;
    //        }
    //        return Vector2.zero;
    //    }


    //    public void SetNode(int cols, int rows, Vector2 pos)
    //    {
    //        if (cols <= this.cols && rows <= this.rows)
    //        {
    //            vertices[cols + rows * (this.cols + 1)].pos = new Vector3(pos.x, pos.y, 0);
    //        }
    //    }


    //    public Vector2 GetOrigNode(int cols, int rows)
    //    {
    //        if (cols <= this.cols && rows <= this.rows)
    //        {
    //            return vertices[cols + rows * (this.cols + 1)].orig;
    //        }
    //        return Vector2.zero;
    //    }

    //    public Vector2 GetCenter()
    //    {
    //        Vector2 leftTop = vertices[0].orig;
    //        return new Vector2(leftTop.x + cols * 0.5f * cellWidth, leftTop.y - rows * 0.5f * cellWidth);
    //    }

    //    public Vector2 GetNode(int cols, int rows)
    //    {
    //        if (cols <= this.cols && rows <= this.rows)
    //        {
    //            return vertices[cols + rows * (this.cols + 1)].pos;
    //        }
    //        return Vector2.zero;
    //    }
    //}


}