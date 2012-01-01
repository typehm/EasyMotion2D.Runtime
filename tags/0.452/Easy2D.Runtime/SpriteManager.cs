using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace EasyMotion2D
{

    /// <summary>
    /// Sprite render mode
    /// </summary>
    public enum SpriteRenderMode
    {
        None = -1,
        AlphaKey,
        Additive,
        SoftAdditive,
        AlphaBlend,
        AlphaBlendPremultiply,
        UserRenderMode01,
        UserRenderMode02,
        UserRenderMode03, 
        UserRenderMode04,
        UserRenderMode05,
        UserRenderMode06,
        UserRenderMode07,
        UserRenderMode08,
        UserRenderMode09,
        UserRenderMode10,
        UserRenderMode11,
    }



    internal interface SpriteRenderable
    {
        void DoClipping(Vector3 pos, float radius);
    }





    [System.Serializable]
    public class IntHolder
    {
        public int value;

        public IntHolder(int v)
        {
            value = v;
        }
    }


    [System.Serializable]
    public class FloatHolder
    {
        public float value;

        public FloatHolder(int v)
        {
            value = v;
        }
    }


    internal class SpritePrimitiveGroup
    {
        public SpritePrimitive[] primitives;
        public SpriteRenderable renderable;

        public GameObject owner;
        public IntHolder count;
        public int layer;
        public bool visible;

        public bool isManaged = false;

        public int index = 0;

        public SpritePrimitiveGroup()
        {
        }

        public void AddToLayer(int layer)
        {
            RemoveFromLayer( this.layer );

            this.layer = layer;
            if (!isManaged)
            {
                SpriteManager.AddPrimitiveGroupToLayer(layer, this);
                isManaged = true;
            }
        }

        public void RemoveFromLayer(int layer)
        {
            if (isManaged)
            {
                SpriteManager.RemovePrimitiveGroupFromLayer(layer, this);
                isManaged = false;
            }
        }

        public bool IsManaged()
        {
            return isManaged;
        }
    }










    /// <summary>
    /// Internal class. You do not need to use this.
    /// </summary>
    [System.Serializable]
    internal class SpriteLayer
    {
        internal int visibleCount = 0;
        internal int id;
        internal List<SpritePrimitiveGroup> baseContainer = new List<SpritePrimitiveGroup>();

        public void SetId(int id)
        {
            this.id = id;
        }

        public void Reset(int size)
        {
            baseContainer.Capacity = size;
        }


        internal void Add( SpritePrimitiveGroup data)
        {
            baseContainer.Add(data);
            data.index = baseContainer.Count - 1;
        }

        internal void Remove( SpritePrimitiveGroup data)
        {
            int idx = data.index;
            int lastIdx = baseContainer.Count - 1;

            if (idx <= lastIdx)
            {
                SpritePrimitiveGroup swap = baseContainer[lastIdx];
                swap.index = idx;
                baseContainer[idx] = swap;
                baseContainer.RemoveAt(lastIdx);
            }
        }


        public SpritePrimitiveGroup this[int index]
        {
            get
            {
                return baseContainer[index];
            }
        }

        public int size
        {
            get
            {
                return baseContainer.Count;
            }
        }
    }

















    /// <summary>
    /// Internal class. You do not need to use this.
    /// </summary>
    internal class SpriteManager
    {
        private static bool isInstNull = true;
        private static SpriteManager _instance = null;

        private static SpriteManager instance
        {
            get
            {
                if (isInstNull)
                {
                    _instance = new SpriteManager();
                    isInstNull = false;
                }
                return _instance;
            }
        }



        private SpriteLayer[] layers = new SpriteLayer[32];
        private int[] layerIDs = new int[32];

        private SpriteManager()
        {
            ResetManager();
        }



        void ResetManager()
        {
            for (int i = 0; i < 32; i++)
            {
                layerIDs[i] = i;

                layers[i] = new SpriteLayer();
                layers[i].Reset(512);
                layers[i].SetId(i);
            }
        }

        int[] _GetLayersID()
        {
            return layerIDs;
        }



        SpriteLayer _GetLayer(int id)
        {
            return layers[id];
        }



        void _AddPrimitiveGroupToLayer(int id, SpritePrimitiveGroup group)
        {
            SpriteLayer layer = _GetLayer(id);
            layer.Add(group);
        }

        void _RemovePrimitiveGroupFromLayer(int id, SpritePrimitiveGroup primitive)
        {
            SpriteLayer layer = _GetLayer(id);
            layer.Remove(primitive);
        }





        internal static void AddPrimitiveGroupToLayer(int id, SpritePrimitiveGroup group)
        {
            instance._AddPrimitiveGroupToLayer(id, group);
        }

        internal static void RemovePrimitiveGroupFromLayer(int id, SpritePrimitiveGroup group)
        {
            instance._RemovePrimitiveGroupFromLayer(id, group);
        }


        internal static int[] GetLayersID()
        {
            return instance._GetLayersID();
        }

        public static SpriteLayer GetLayer(int id)
        {
            return instance._GetLayer(id);
        }


        internal static void Clear()
        {
            instance.ResetManager();
        }

    }
















    /// <summary>
    /// Internal class. You do not need to use this. 
    /// </summary>
    public class EasyMotion2DUtility
    {
        public delegate void SpriteLayerDelegate(int layer, int count);
        public delegate void PrimitiveGroupDelegate( object group, int index, GameObject owner );
        public delegate void PrimitiveDelegate( object[] primitives, int count, object parent);


        public static void EnumSpriteInSpriteManager( 
            SpriteLayerDelegate beginLayer, SpriteLayerDelegate endLayer,
            PrimitiveGroupDelegate beginGroup, PrimitiveGroupDelegate endGroup,
            PrimitiveDelegate primitive )
        {
            int[] ids = SpriteManager.GetLayersID();

            foreach (int id in ids)
            {               
                SpriteLayer layer = SpriteManager.GetLayer(id);

                if (beginLayer != null)
                    beginLayer(id, layer.size);


                for (int i = 0, e = layer.size; i < e; i++)
                {
                    SpritePrimitiveGroup group = layer[i];

                    if (beginGroup != null)
                        beginGroup( group, i, group.owner);


                    if (primitive != null)
                        primitive( group.primitives, group.count.value, group );


                    if (endGroup != null)
                        endGroup(group, i, group.owner);
                }

                if (endLayer != null)
                    endLayer(id, layer.size);
            }
        }


        public static int PickupObject(Ray ray)
        {
            int[] ids = SpriteManager.GetLayersID();

            foreach (int id in ids)
            {
                SpriteLayer layer = SpriteManager.GetLayer(id);

                for (int i = 0, e = layer.size; i < e; i++)
                {
                    SpritePrimitiveGroup group = layer[i];

                    for (int idx = 0; idx < group.count.value; idx++)
                    {
                        SpritePrimitive pri = group.primitives[idx];
                        if (pri.visible)
                        {
                            bool ret = IntersectTriangle(ray.origin, ray.direction, pri.position[0], pri.position[1], pri.position[2], 0, 0, 0)
                                || IntersectTriangle(ray.origin, ray.direction, pri.position[0], pri.position[2], pri.position[3], 0, 0, 0);

                            if (ret)
                                return pri.ownerID;
                        }
                    }
                }
            }

            return 0;
        }

        // Determine whether a ray intersect with a triangle
        // Parameters
        // orig: origin of the ray
        // dir: direction of the ray
        // v0, v1, v2: vertices of triangle
        // t(out): weight of the intersection for the ray
        // u(out), v(out): barycentric coordinate of intersection
         public static bool IntersectTriangle(Vector3 orig, Vector3 dir,
                                 Vector3 v0, Vector3 v1, Vector3 v2,
                                float t, float u, float v)
         {
             // E1
             Vector3 E1 = v1 - v0;
         
            // E2
             Vector3 E2 = v2 - v0;
         
             // P
             Vector3 P = Vector3.Cross( dir, E2);
         
             // determinant
             float det = Vector3.Dot( E1, P);
         
             // keep det > 0, modify T accordingly
             Vector3 T;
             if( det > 0 )
             {
                 T = orig - v0;
             }
             else
             {
                T = v0 - orig;
                 det = -det;
             }
         
             // If determinant is near zero, ray lies in plane of triangle
             if( det < 0.0001f )
                 return false;
         
             // Calculate u and make sure u <= 1
             u = Vector3.Dot( T, P);
             if( u < 0.0f || u > det )
                 return false;
         
             // Q
             Vector3 Q = Vector3.Cross( T, E1);
         
             // Calculate v and make sure u + v <= 1
             v = Vector3.Dot( dir, Q);
             if( v < 0.0f || u + v > det )
                 return false;
         
             // Calculate t, scale parameters, ray intersects triangle
             t = Vector3.Dot( E2, Q);
         
             float fInvDet = 1.0f / det;
             t *= fInvDet;
             u *= fInvDet;
             v *= fInvDet;
         
             return true;
         }
    }
}