using UnityEngine;
using System.Collections;
using System.Collections.Generic;





namespace EasyMotion2D
{
    [System.Serializable]
    public class BinderItem
    {
        public GameObject bindObject;
        public string componentName;
        public Vector2 offset;
        public float rotation;
    }


    /// <summary>
    /// Bind GameObjects with a sub sprite in SpriteRenderer.
    /// </summary>
    public class GameObjectBinder : MonoBehaviour
    {
        private SpriteAnimation.SpriteAnimatonDelegate moveDelegate = null;
        private SpriteRenderer spriteRenderer;

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

                    pos.z = item.bindObject.transform.position.z;
                    item.bindObject.transform.position = transform.TransformPoint(pos);
                    item.bindObject.transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + tmp.rotation + item.rotation);
                }
            }
        }
    }
}