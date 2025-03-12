#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputUIInteractionHandler : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description = "IsPointerOverUI";

        private NewInputUIInteractionReceiver activeReceiver;

        #region MANAGEMENT
        public string Description => this.description;
        public void Init() {}
        public bool IsApplicable(InputAction.CallbackContext input) {
            this.activeReceiver = null;
            if (NewInputManager.Instance.IsOverUI(out List<RaycastResult> raycastResults)) { 
                IsOverExtendedUI(raycastResults);
                return true;
            }
            return false;
        }
        private bool IsOverExtendedUI(List<RaycastResult> raycastResults) {
            for (int i = 0; i < raycastResults.Count; i++) {
                this.activeReceiver = raycastResults[i].gameObject.GetFirstComponentInParentRecursively<NewInputUIInteractionReceiver>();
                if (this.activeReceiver) {
                    #if DEBUG2
                    this.Log($"UI interactionReceiver selected {this.activeReceiver.name}");
                    #endif
                    return true;
                }
            }
            #if DEBUG2
            this.Log($"No UI interactionReceivers found");
            #endif
            return false;
        }
        #endregion
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"Over UI Started");
            #endif
            this.activeReceiver?.HandleStarted(input);
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"Over UI Performed");
            #endif
            this.activeReceiver?.HandlePerformed(input);
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"Over UI Ended");
            #endif
            this.activeReceiver?.HandleEnded(input);
        }
        #endregion

    }
}
