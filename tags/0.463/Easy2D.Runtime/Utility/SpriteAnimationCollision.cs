
using UnityEngine;
using System.Collections;


namespace EasyMotion2D
{
    /// <summary>
    /// Store bone base collider information. You do not need to use it directly.
    /// </summary>
    [System.Serializable]
    public class ColliderNode
    {
        /// <summary>
        /// Need attach a collider?
        /// </summary>
        public bool needAddCollider;

        
        /// <summary>
        /// Event listener must be set while the game is running.
        /// </summary>
        public bool eventListenerSet;


        /// <summary>
        /// Referenced animation component's name.
        /// </summary>
        public string nodeName;


        /// <summary>
        /// Referenced animation component's full pathname.
        /// </summary>
        public string nodePath;

        /// <summary>
        /// Pre-calculate path string hash.
        /// </summary>
        public int nodePathHash;

        /// <summary>
        /// Is collider should be a trigger?
        /// </summary>
        public bool isTrigger = false;

        /// <summary>
        /// Collider offset in depth axis.
        /// </summary>
        public float depthOffset = 0f;

        /// <summary>
        /// The thickness of collider.
        /// </summary>
        public float thickness = 20f;


        /// <summary>
        /// Collider owner gameobject.
        /// </summary>
        public GameObject colliderObject;
    }


    /// <summary>
    /// Type of CollisionComponent generate collider.
    /// </summary>
    public enum CollisionType
    {
        /// <summary>
        /// Nothing to do.
        /// </summary>
        None = 0,

        /// <summary>
        /// Create a collider as whole animation's bounding box.
        /// </summary>
        SpriteRendererAABB,

        /// <summary>
        /// Create colliders as selected bone's bounding box.
        /// </summary>
        AnimationComponentBB,
    }


    /// <summary>
    /// Create colliders by selected bone in inspector. And transform colliders in runtime when clip playing.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteAnimation))]
    public class CollisionComponent : MonoBehaviour
    {
        [SerializeField]
        private CollisionType _type = CollisionType.None;

        /// <summary>
        /// Need sendmessage collision event by MonoBehaviour.SendMessage? Else will send event to the delegate handler.
        /// </summary>
        public bool sendMessage = true;

        /// <summary>
        /// Colliders offset in depth axis.
        /// </summary>
        public float depthOffset = 0f;

        /// <summary>
        /// Colliders thickness.
        /// </summary>
        public float thickness = 20f;

        /// <summary>
        /// Collision type.
        /// </summary>
        public CollisionType type
        {
            set
            {
                if (_type == value)
                    return;

                ChangeType( _type, value);
            }

            get
            {
                return _type;
            }
        }


        private SpriteRenderer.ApplyHandler applyHandler = null;
        private SpriteAnimation.SpriteAnimatonDelegate applyMultiBoundingHandler = null;


        /// <summary>
        /// Change collision type. Internal used. Recommand use CollisionComponent.type to instead it.
        /// </summary>
        public void ChangeType( CollisionType from, CollisionType to)
        {
            if (from != to)
            {
                RemoveCollider(from);
            }

            AddCollider(to);

            _type = to;
        }




        private void AddCollider(CollisionType to)
        {
            if (to == CollisionType.SpriteRendererAABB)
            {
                if (this.collider == null)
                {
                    gameObject.AddComponent<BoxCollider>();
                }

                if (applyHandler == null)
                {
                    applyHandler = new SpriteRenderer.ApplyHandler(applyBounding);
                }
                spriteRenderer.applyHandler += applyHandler;
            }

            else if (to == CollisionType.AnimationComponentBB)
            {
                BuildCollider();

                if (applyMultiBoundingHandler == null)
                {
                    applyMultiBoundingHandler = new SpriteAnimation.SpriteAnimatonDelegate(applyBounding);
                }
                spriteAnimation.AddComponentUpdateHandler(SpriteAnimationComponentUpdateType.PostUpdateComponent, applyMultiBoundingHandler);
            }
        }




        private void RemoveCollider(CollisionType from)
        {
            if (from == CollisionType.SpriteRendererAABB)
            {
                if (this.collider != null && this.collider is BoxCollider)
                {
                    if (Application.isPlaying)
                        Object.DestroyObject(this.collider);
                    //else
                    //    Object.DestroyImmediate(this.collider);
                }

                if (applyHandler != null)
                {
                    spriteRenderer.applyHandler -= applyHandler;
                    applyHandler = null;
                }
            }

            else if (from == CollisionType.AnimationComponentBB)
            {
                RemoveAllCollider();

                if (applyMultiBoundingHandler != null)
                {
                    spriteAnimation.RemoveComponentUpdateHandler(SpriteAnimationComponentUpdateType.PostUpdateComponent, applyMultiBoundingHandler);
                    applyMultiBoundingHandler = null;
                }
            }
        }


        void applyBounding(SpriteRenderer renderer)
        {
            UpdateColliderAABB(renderer);          
        }

        private void UpdateColliderAABB(SpriteRenderer renderer)
        {
            Collider col = collider;

            if (col is BoxCollider)
            {
                BoxCollider bc = col as BoxCollider;


                Vector3 s = transform.localScale;

                Vector2 bbc = renderer.boundingAABB.center;
                Rect rc = renderer.boundingAABB.position;


                Vector3 center = Vector3.zero;
                Vector3 size = Vector3.one;

                switch (renderer.plane)
                {
                    case SpritePlane.PlaneXY:
                        {
                            Vector3 c = transform.InverseTransformPoint(bbc);
                            center = new Vector3(c.x, c.y, depthOffset);
                            size = new Vector3(rc.width / s.x, rc.height / s.y, thickness);
                        }
                        break;

                    case SpritePlane.PlaneZY:
                        {
                            Vector3 c = transform.InverseTransformPoint(new Vector3(0, bbc.y, bbc.x));
                            center = new Vector3(depthOffset, c.y, c.z);
                            size = new Vector3(thickness, rc.height / s.y, rc.width / s.z);
                        }
                        break;

                    case SpritePlane.PlaneXZ:
                        {
                            Vector3 c = transform.InverseTransformPoint(new Vector3(bbc.x, 0, bbc.y));
                            center = new Vector3(c.x, depthOffset, c.z);
                            size = new Vector3(rc.width / s.x, thickness, rc.height / s.z);
                        }
                        break;
                }


                bc.center = center;
                bc.size = size;
            }
        }



        void applyBounding(SpriteAnimation ani)
        {
            foreach (ColliderNode node in colliderNones)
            {
                if (node.colliderObject != null)
                {
                    updateCollisionTransformsForNode(node);
                }
            }
        }


        // Setup an event delegate so other components can listen for when collisions occur on the nodes
        //	of our animation. We pass back the name of the hit object so developers can track the
        //	hit object based on the name of the node.
        static private void dummyHandler(CollisionNodeInfo cni)
        {
        }

        /// <summary>
        /// Delegate to handler the collision event in sub colliders.
        /// </summary>
        /// <param name="cni">Collision information datas.</param>
        public delegate void CollisionEvent(CollisionNodeInfo cni);
        
        /// <summary>
        /// OnComponentMouseEnter is called when the mouse entered Collider.
        /// </summary>
        public CollisionEvent OnComponentMouseEnter = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentMouseOver is called every frame while the mouse is over Collider.
        /// </summary>
        public CollisionEvent OnComponentMouseOver = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentMouseExit is called when the mouse is not any longer over the Collider.
        /// </summary>
        public CollisionEvent OnComponentMouseExit = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentMouseDown is called when the user has pressed the mouse button while over the Collider.
        /// </summary>
        public CollisionEvent OnComponentMouseDown = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentMouseUp is called when the user has released the mouse button.
        /// </summary>
        public CollisionEvent OnComponentMouseUp = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentMouseUpAsButton is only called when the mouse is released over the same Collider as it was pressed. 
        /// </summary>
        public CollisionEvent OnComponentMouseUpAsButton = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentMouseDrag is called when the user has clicked on a Collider and is still holding down the mouse.
        /// </summary>
        public CollisionEvent OnComponentMouseDrag = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentTriggerEnter is called when the Collider other enters the trigger.
        /// </summary>
        public CollisionEvent OnComponentTriggerEnter = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentTriggerExit is called when the Collider other has stopped touching the trigger.
        /// </summary>
        public CollisionEvent OnComponentTriggerExit = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentTriggerStay is called once per frame for every Collider other that is touching the trigger.
        /// </summary>
        public CollisionEvent OnComponentTriggerStay = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
        /// </summary>
        public CollisionEvent OnComponentCollisionEnter = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider.
        /// </summary>
        public CollisionEvent OnComponentCollisionExit = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider.
        /// </summary>
        public CollisionEvent OnComponentCollisionStay = new CollisionEvent(dummyHandler);

        /// <summary>
        /// OnComponentControllerColliderHit is called when the controller hits a collider while performing a Move.
        /// </summary>
        public CollisionEvent OnComponentControllerColliderHit = new CollisionEvent(dummyHandler);







        //use a clip to know the bones of animation
        //EasyMotino2D animation not a skinned mesh animation, so the bones can be difference between clips.
        //user should keep the bone struct are the same in clips
        /// <summary>
        /// The clip use to create the collider.
        /// </summary>
        public SpriteAnimationClip basicBoneClip;

        //collider nodes
        [HideInInspector]
        public ColliderNode[] colliderNones;

        //component reference
        private SpriteRenderer spriteRenderer;
        private SpriteAnimation spriteAnimation;


        // Use this for initialization
        void OnEnable()
        {
            
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteAnimation = GetComponent<SpriteAnimation>();

            ChangeType(type, type);
        }



        void OnDestroy()
        {
            
            RemoveCollider(_type);
        }




        void Update()
        {
            if (!Application.isPlaying)
            {
                // Since this can be run in editor mode, we could come across a situation where the spriteRenderer has not
                //	yet been assigned. Make sure we can assign it here before continuing.
                if (spriteRenderer == null)
                    spriteRenderer = GetComponent<SpriteRenderer>();
                else
                {
                    if (GetComponent<SpriteRenderer>() == null)
                    {
                        spriteRenderer = null;
                        RemoveAllCollider();
                    }
                }

                if (spriteAnimation == null)
                {
                    spriteAnimation = GetComponent<SpriteAnimation>();
                    if (spriteAnimation != null)
                    {
                        BuildCollider();
                    }
                }
                else
                {
                    if (GetComponent<SpriteAnimation>() == null)
                    {
                        spriteAnimation = null;
                        RemoveAllCollider();
                    }
                }
            }
        }


        /// <summary>
        /// Internal function.
        /// </summary>
        public void RemoveAllCollider()
        {
            foreach (ColliderNode node in colliderNones)
            {
                if (node.colliderObject != null)
                {
                    GameObject destroyObj = node.colliderObject;
                    node.colliderObject = null;

                    // Remove the event listeners for this object
                    destroyObj.GetComponent<CollisionNodeToggler>().nodeCollisionHandler -= nodeCollisionHandler;

                    if (Application.isEditor)
                        Object.DestroyImmediate(destroyObj);
                    else
                        Object.Destroy(destroyObj);
                }
            }
        }


        /// <summary>
        /// Internal function.
        /// </summary>
        public void BuildCollider()
        {
            foreach (ColliderNode node in colliderNones)
            {
                if (node.needAddCollider && node.colliderObject == null )
                {
                    node.nodePathHash = EasyMotion2DUtility.GetHashCode( node.nodePath );

                    GameObject tmp = createColliderObject(node.nodeName, transform, node.nodePath);
                    node.colliderObject = tmp;
                    node.colliderObject.collider.isTrigger = node.isTrigger;
                }
            }
        }

        /// <summary>
        /// Internal function.
        /// </summary>
        /// <param name="node"></param>
        public void addColliderToNode(ColliderNode node)
        {
            GameObject tmp = createColliderObject(node.nodeName, transform, node.nodePath);
            node.colliderObject = tmp;

            if (Application.isPlaying)
                setNodeCollisionListeners(node);
        }




        GameObject createColliderObject(string newColName, Transform parentTransform, string nodePath )
        {
            // Create the GameObject itself
            GameObject newColObj = new GameObject(newColName);

            // Add the Box Collider and give it some initial properties
            newColObj.AddComponent<BoxCollider>();
            BoxCollider boxCollider = newColObj.GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(1.0f, 1.0f, 10.0f);
            boxCollider.isTrigger = true;

            // Add the event delegate component
            CollisionNodeToggler nodeToggler = newColObj.AddComponent<CollisionNodeToggler>();
            nodeToggler.componentPath = nodePath;

            newColObj.transform.parent = parentTransform;

            return newColObj;
        }

        void setNodeCollisionListeners(ColliderNode node)
        {
            GameObject colObj = node.colliderObject;
            colObj.GetComponent<CollisionNodeToggler>().nodeCollisionHandler += nodeCollisionHandler;

            node.eventListenerSet = true;
        }
        



        void nodeCollisionHandler(CollisionNodeToggler hitNode, Collider colliderObj, Collision collisionObj, ControllerColliderHit cchit, NodeCollisionEvent evt)
        {
            if (sendMessage)
            {
                // Inform any listening components that collision entry occured.
                switch (evt)
                {
                    case NodeCollisionEvent.OnMouseEnter:
                        SendMessage("OnComponentMouseEnter", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnMouseOver:
                        SendMessage("OnComponentMouseOver", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnMouseExit:
                        SendMessage("OnComponentMouseExit", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnMouseDown:
                        SendMessage("OnComponentMouseDown", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnMouseUp:
                        SendMessage("OnComponentMouseUp", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnMouseUpAsButton:
                        SendMessage("OnComponentMouseUpAsButton", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnMouseDrag:
                        SendMessage("OnComponentMouseDrag", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnTriggerEnter:
                        SendMessage("OnComponentTriggerEnter", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnTriggerExit:
                        SendMessage("OnComponentTriggerExit", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnTriggerStay:
                        SendMessage("OnComponentTriggerStay", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnCollisionEnter:
                        SendMessage("OnComponentCollisionEnter", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnCollisionExit:
                        SendMessage("OnComponentCollisionExit", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnCollisionStay:
                        SendMessage("OnComponentCollisionStay", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;

                    case NodeCollisionEvent.OnControllerColliderHit:
                        SendMessage("OnComponentControllerColliderHit", new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit), SendMessageOptions.DontRequireReceiver);
                        break;
                }
            }
            //else
            {
                switch (evt)
                {
                    case NodeCollisionEvent.OnMouseEnter:
                        OnComponentMouseEnter( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnMouseOver:
                        OnComponentMouseOver( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnMouseExit:
                        OnComponentMouseExit( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnMouseDown:
                        OnComponentMouseDown( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnMouseUp:
                        OnComponentMouseUp( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnMouseUpAsButton:
                        OnComponentMouseUpAsButton( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnMouseDrag:
                        OnComponentMouseDrag( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnTriggerEnter:
                        OnComponentTriggerEnter( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnTriggerExit:
                        OnComponentTriggerExit( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnTriggerStay:
                        OnComponentTriggerStay( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnCollisionEnter:
                        OnComponentCollisionEnter( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnCollisionExit:
                        OnComponentCollisionExit( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnCollisionStay:
                        OnComponentCollisionStay( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;

                    case NodeCollisionEvent.OnControllerColliderHit:
                        OnComponentControllerColliderHit( new CollisionNodeInfo(hitNode, colliderObj, collisionObj, cchit));
                        break;
                }
            }
        }

        /// <summary>
        /// Internal function.
        /// </summary>
        /// <param name="node"></param>
        public void updateCollisionTransformsForNode(ColliderNode node)
        {
            //SpriteTransform is transform data of animation node, include postion, rotation, scale, shear
            //SpriteTransform is in local space of SpriteRenderer component gameObject

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            SpriteTransform nodeTransform = spriteRenderer.GetSpriteTransformByFullPathHash(node.nodePathHash);
            if (nodeTransform == null)
                return;

            // Find the sprite object
            Sprite partSprite = null;
            if (nodeTransform.overrideSprite != null)
            {
                partSprite = nodeTransform.overrideSprite;
            }

            else if (nodeTransform.sprite != null)
            {
                partSprite = nodeTransform.sprite;
            }

            if (nodeTransform != null && partSprite)
            {
                //todo: 
                // 1. setting collider to correct center and size
                // You can get use Sprite and SpriteTransform to calculate the bounding box of a animation part. 
                // And use the bounding box data to set the center and size
                // 2. setting collider in correct plane in world space.
                // EasyMotion2D support sprite rendering to 3 plane: XY, ZY, XZ. Collider should be in correct plane
                // etc

                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                Vector3 scale = Vector3.one;
                Vector3 size = Vector3.one;
                Vector3 center = Vector3.zero;

                float localRotation = spriteRenderer.localRotation;
                Vector2 localScale = spriteRenderer.localScale;
                bool rr = localScale.x * localScale.y < 0f;
                float sr = rr  ? -1f : 1f;


                float pr = Mathf.Deg2Rad * (localRotation);
                float psin = Mathf.Sin(pr);
                float pcos = Mathf.Cos(pr);


                float psx = (nodeTransform.position.x) * localScale.x;
                float psy = (nodeTransform.position.y) * localScale.y;

                float tx = (psx * pcos - psy * psin);
                float ty = (psx * psin + psy * pcos);



                switch (spriteRenderer.plane)
                {
                    case SpritePlane.PlaneXY:
                        {
                            position = new Vector3( tx, ty, node.depthOffset);

                            rotation = Quaternion.Euler(0, 0, localRotation + nodeTransform.rotation * sr);

                            scale = new Vector3( (1.0f + nodeTransform.shear.y) * localScale.x, 
                                (1.0f + nodeTransform.shear.x) * localScale.y, 
                                1);

                            size = new Vector3((partSprite.imageRect.width * nodeTransform.scale.x),
                                (partSprite.imageRect.height * nodeTransform.scale.y),
                                node.thickness);

                            center = new Vector3((partSprite.imageRect.width * 0.5f - partSprite.pivot.x),
                                (partSprite.imageRect.height * 0.5f - partSprite.pivot.y),
                                0);

                            center.y = -center.y;
                        }
                        break;

                    case SpritePlane.PlaneZY:
                        {
                            position = new Vector3(-node.depthOffset, ty, tx);

                            rotation = Quaternion.Euler(-localRotation + nodeTransform.rotation * -sr, 0, 0);

                            scale = new Vector3(1, 
                                (1.0f + nodeTransform.shear.x) * localScale.y, 
                                (1.0f + nodeTransform.shear.y) * localScale.x);

                            size = new Vector3(node.thickness,
                                partSprite.imageRect.height * nodeTransform.scale.y,
                                partSprite.imageRect.width * nodeTransform.scale.x);

                            center = new Vector3(0,
                                (partSprite.imageRect.height * 0.5f - partSprite.pivot.y),
                                (partSprite.imageRect.width * 0.5f - partSprite.pivot.x)
                                );

                            center.y = -center.y;
                        }
                        break;

                    case SpritePlane.PlaneXZ:
                        {

                            position = new Vector3( tx, -node.depthOffset, ty);

                            rotation = Quaternion.Euler(0, (-localRotation - nodeTransform.rotation * sr) + (rr ? 180f : 0f), 0);
                            
                            scale = new Vector3( (1.0f + nodeTransform.shear.y) * localScale.y,
                                1,
                                (1.0f + nodeTransform.shear.x) * localScale.x);

                            size = new Vector3(partSprite.imageRect.width * nodeTransform.scale.x,
                                node.thickness,
                                partSprite.imageRect.height * nodeTransform.scale.y);

                            center = new Vector3((partSprite.imageRect.width * 0.5f - partSprite.pivot.x),
                                0,
                                (partSprite.imageRect.height * 0.5f - partSprite.pivot.y) );

                            center.z = -center.z;
                        }
                        break;
                }



                Transform t = node.colliderObject.transform;

                t.localPosition = position;
                t.localRotation = rotation;
                t.localScale = scale;

                BoxCollider tmpCollider = (node.colliderObject.collider as BoxCollider);


                tmpCollider.center = center;
                tmpCollider.size = size;



                if (Application.isPlaying)
                {
                    // Check the collision listener event
                    if (!node.eventListenerSet)
                    {
                        setNodeCollisionListeners(node);
                    }
                }
                else
                {
                    // If the game is not playing, but the collision listener is still set, disable it so we know to reenable
                    //	it when the game is playing.
                    if (node.eventListenerSet)
                        node.eventListenerSet = false;
                }
            }
        }
    }

}