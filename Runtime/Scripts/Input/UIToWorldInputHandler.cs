#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

namespace PolytopeSolutions.Toolset.Input {
    public abstract class UIToWorldInputHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField]private UIToWorldInputReceiver inputReceiver;
        
        protected void Start() {
            if (!this.inputReceiver) {
                this.inputReceiver = GameObject.FindObjectOfType<UIToWorldInputReceiver>();
                if (!this.inputReceiver)
                    Debug.LogWarning("UIToWorldInputHandler [" + gameObject.name + "]: No UIToWorldInputReceiver found!");
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Inform the input receiver that this handler is being hovered over
        public void OnPointerEnter(PointerEventData eventData) {
            #if DEBUG2
            Debug.Log("UIToWorldInputHandler [" + gameObject.name + "]: Pointer Enter");
            #endif
            this.inputReceiver?.HandlerHoverEnter(this);
        }
        // Inform the input receiver that this handler is not being hovered over anymore
        public void OnPointerExit(PointerEventData eventData) {
            #if DEBUG2
            Debug.Log("UIToWorldInputHandler [" + gameObject.name + "]: Pointer Exit");
            #endif
            this.inputReceiver?.HandlerHoverExit(this);
        }

        ///////////////////////////////////////////////////////////////////////
        // Methods to be called by the input receiver,
        // if at the moment of press, this handler is being hovered over
        public abstract void OnInteractionStarted();
        public abstract void OnInteractionPerformed(Vector2 screenMousePos);
        public abstract void OnInteractionEnded();
    }
}