using UnityEngine;
using System.Collections;
using System.Collections.Generic;





namespace EasyMotion2D
{
    /// <summary>
    /// Bind item information class
    /// </summary>
    [System.Serializable]
    public class BinderItem
    {
        /// <summary>
        /// The gameobject you want to bind.
        /// </summary>
        public GameObject bindObject;

        /// <summary>
        /// The bone name/path you want bind to.
        /// </summary>
        public string componentName;

        /// <summary>
        /// Offset in bind target bone local space.
        /// </summary>
        public Vector2 offset;

        /// <summary>
        /// Rotation in bind target bone local space.
        /// </summary>
        public float rotation;
    }


    /// <summary>
    /// Bind GameObjects with a sub sprite in SpriteRenderer.
    /// </summary>
    public class GameObjectBinder : MonoBehaviour
    {
        private SpriteAnimation.SpriteAnimatonDelegate moveDelegate = null;
        private SpriteRenderer spriteRenderer;

        /// <summary>
        /// All the gameobject bind in this components.
        /// </summary>
        public BinderItem[] bindObjects = new BinderItem[] { };

        void OnEnable()
        {
            
            if (moveDelegate == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                moveDelegate = new SpriteAnimation.SpriteAnimatonDelegate(binderHandler);
                GetComponent<SpriteAnimation>().AddComponentUpdateHandler(SpriteAnimationComponentUpdateType.PostUpdateComponent, moveDelegate);
            }
        }


        void OnDestroy()
        {
            
            if (moveDelegate != null)
            {
                GetComponent<SpriteAnimation>().RemoveComponentUpdateHandler(SpriteAnimationComponentUpdateType.PostUpdateComponent, moveDelegate);
            }
        }


        void binderHandler(SpriteAnimation ani)
        {
            foreach (BinderItem item in bindObjects)
            {
                SpriteTransform tmp = spriteRenderer.GetSpriteTransform(item.componentName);
                if (tmp != null)
                {
                    Vector3 offset = item.offset;
                    Vector3 position = tmp.position;
                    Vector3 pos = position + (Quaternion.Euler(0, 0, tmp.rotation) * offset);

                    pos = transform.TransformPoint(pos);
                    pos.z = item.bindObject.transform.position.z;
                    item.bindObject.transform.position = pos;
                    item.bindObject.transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + tmp.rotation + item.rotation);
                }
            }
        }
    }
}