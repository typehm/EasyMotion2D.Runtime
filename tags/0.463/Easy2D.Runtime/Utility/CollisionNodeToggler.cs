using UnityEngine;
using System.Collections;

namespace EasyMotion2D
{
    internal enum NodeCollisionEvent
    {
        OnMouseEnter = 0,
        OnMouseOver,
        OnMouseExit,
        OnMouseDown,
        OnMouseUp,
        OnMouseUpAsButton,
        OnMouseDrag,
        OnTriggerEnter,
        OnTriggerExit,
        OnTriggerStay,
        OnCollisionEnter,
        OnCollisionExit,
        OnCollisionStay,
        OnControllerColliderHit,
    }

    /// <summary>
    /// Sub colliders collision event data.
    /// </summary>
    public struct CollisionNodeInfo
    {
        /// <summary>
        /// The collisioned sub collider wrapper.
        /// </summary>
        public CollisionNodeToggler hitNode;

        /// <summary>
        /// If a trigger event, here is other Collider, else is null.
        /// </summary>
        public Collider collider;

        /// <summary>
        /// If a collision event, here is other Collision, else is null.
        /// </summary>
        public Collision collision;

        /// <summary>
        /// If a ControllerColliderHit event, here is ControllerColliderHit, else is null.
        /// </summary>
        public ControllerColliderHit controllerColliderHit;

        public CollisionNodeInfo(CollisionNodeToggler hitNode, Collider colliderObj, Collision collisionObj, ControllerColliderHit cchit)
        {
            this.hitNode = hitNode;
            collider = colliderObj;
            collision = collisionObj;
            controllerColliderHit = cchit;
        }
    }

    /// <summary>
    /// CollisionComponent hepler components. Dispatch collider collision event to CollisionComponent.
    /// Create by CollisionComponent automatically. You should never modify any class members in runtime.
    /// </summary>
    public class CollisionNodeToggler : MonoBehaviour
    {
        internal delegate void collisionNodeEvent(CollisionNodeToggler hitNode, Collider colliderObj, Collision collisionObj, ControllerColliderHit cchit, NodeCollisionEvent evt);
        internal collisionNodeEvent nodeCollisionHandler;

        /// <summary>
        /// The path of bone.
        /// </summary>
        public string componentPath;

        void OnTriggerEnter(Collider colObj)
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, colObj, null, null, NodeCollisionEvent.OnTriggerEnter);
        }

        void OnTriggerStay(Collider colObj)
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, colObj, null, null, NodeCollisionEvent.OnTriggerStay);
        }

        void OnTriggerExit(Collider colObj)
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, colObj, null, null, NodeCollisionEvent.OnTriggerExit);
        }


        void OnMouseEnter()
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, null, null, NodeCollisionEvent.OnMouseEnter);
        }

        void OnMouseOver()
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, null, null, NodeCollisionEvent.OnMouseOver);
        }

        void OnMouseExit()
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, null, null, NodeCollisionEvent.OnMouseExit);
        }


        void OnMouseDown()
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, null, null, NodeCollisionEvent.OnMouseDown);
        }

        void OnMouseUp()
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, null, null, NodeCollisionEvent.OnMouseUp);
        }

        void OnMouseUpAsButton()
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, null, null, NodeCollisionEvent.OnMouseUpAsButton);
        }

        void OnMouseDrag()
        {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, null, null, NodeCollisionEvent.OnMouseDrag);
        }


        void OnCollisionEnter(Collision collision) {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, collision, null, NodeCollisionEvent.OnCollisionEnter);
        }

        void OnCollisionExit(Collision collision) {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, collision, null, NodeCollisionEvent.OnCollisionExit);
        }

        void OnCollisionStay(Collision collision) {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, collision, null, NodeCollisionEvent.OnCollisionStay);
        }

        void OnControllerColliderHit(ControllerColliderHit hit) {
            if (nodeCollisionHandler != null)
                nodeCollisionHandler(this, null, null, hit, NodeCollisionEvent.OnControllerColliderHit);
        }
    }
}