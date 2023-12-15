#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.Events;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    public class InteractableObjectInputHandler : MonoBehaviour {
        [SerializeField] protected List<Collider> colliders;
        [SerializeField] private UnityEvent onInteractionStarted;
        [SerializeField] private UnityEvent onInteractionEnded;
        [SerializeField] private ObjectInteractorInputReceiver inputReceiver;

        ///////////////////////////////////////////////////////////////////////
        // Inform the input receiver that this handler is being hovered over
        protected virtual void Start() {
            #if DEBUG2
            this.Log("Subscribe to events");
            #endif
            if (!this.inputReceiver)
                this.inputReceiver = GameObject.FindObjectOfType<ObjectInteractorInputReceiver>();
            if (!this.inputReceiver)
                this.LogWarning("No ObjectInteractorInputReceiver found!");
            if (this.colliders == null || this.colliders.Count == 0)
                this.LogWarning("No Colliders were given!");
            if (this.inputReceiver && this.colliders != null)
                this.inputReceiver.RegisterInputHandler(this);
        }
        // Inform the input receiver that this handler is not being hovered over anymore
        protected virtual void OnDestory() {
            #if DEBUG2
            this.Log("Unubscribe from events");
            #endif
            if (this.inputReceiver && this.colliders != null)
                this.inputReceiver.UnregisterInputHandler(this);
        }
        ///////////////////////////////////////////////////////////////////////
        public bool IsRayValid(RaycastHit hitInfo) {
            bool isValid = this.colliders.Contains(hitInfo.collider);
            #if DEBUG2
            this.Log($"Clicked on an interactable object: {isValid}");
            #endif
            return isValid;
        }
        public virtual void OnInteractionStarted() {
            #if DEBUG2
            this.Log($"Interaction started");
            #endif
            this.onInteractionStarted?.Invoke();
        }
        public virtual void OnInteractionEnded() {
            #if DEBUG2
            this.Log($"Interaction ended");
            #endif
            this.onInteractionEnded?.Invoke();
        }
    }
}
