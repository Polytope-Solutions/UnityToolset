#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    public class UIToWorldInputHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] private UnityEvent onInteractionStarted;
        [SerializeField] private UnityEvent<RaycastHit> onInteractionPerformed;
        [SerializeField] private UnityEvent onInteractionEnded;
        [SerializeField]private UIToWorldInputReceiver inputReceiver;

        protected void Start() {
            if (!this.inputReceiver) {
                this.inputReceiver = GameObject.FindObjectOfType<UIToWorldInputReceiver>();
                if (!this.inputReceiver)
                    this.LogWarning("No UIToWorldInputReceiver found!");
            }
        }
        protected void OnDestroy() {
            this.inputReceiver?.UnregisterInputHandler(this);
        }

        ///////////////////////////////////////////////////////////////////////
        // Inform the input receiver that this handler is being hovered over
        public void OnPointerEnter(PointerEventData eventData) {
            #if DEBUG2
            this.Log(Pointer Enter");
            #endif
            this.inputReceiver?.RegisterInputHandler(this);
        }
        // Inform the input receiver that this handler is not being hovered over anymore
        public void OnPointerExit(PointerEventData eventData) {
            #if DEBUG2
            this.Log(Pointer Exit");
            #endif
            this.inputReceiver?.UnregisterInputHandler(this);
        }

        ///////////////////////////////////////////////////////////////////////
        // Methods to be called by the input receiver,
        // if at the moment of press, this handler is being hovered over
        public virtual void OnInteractionStarted() { 
            #if DEBUG2
            this.Log($"Interaction started");
            #endif
            this.onInteractionStarted?.Invoke();
        }
        public virtual void OnInteractionPerformed(RaycastHit hitInfo) { 
            #if DEBUG2
            this.Log($"Ray hit: {hitinfo.transform.gameObject.name}");
            #endif
            this.onInteractionPerformed?.Invoke(hitInfo);
        }
        public virtual void OnInteractionEnded() {
            #if DEBUG2
            this.Log($"Interaction ended");
            #endif
            this.onInteractionEnded?.Invoke();
        }
    }
}