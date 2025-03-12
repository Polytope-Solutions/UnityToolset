#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputObjectInteractionHandler : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description;
        [SerializeField] private float raycastMaxDistance = 100;
        [SerializeField] private LayerMask raycastLayerMask;
        private Camera sourceCamera;
        private Vector2 screenPointerPosition;
        private Ray screenRay;
        private RaycastHit screenRayHit;
        private NewInputObjectInteractionReceiver activeReceiver;
        #region MANAGEMENT
        public string Description => this.description;
        public void Init() {
            this.sourceCamera = Camera.main;
        }
        public bool IsApplicable(InputAction.CallbackContext input) {
            this.activeReceiver = null;
            this.screenPointerPosition = Pointer.current.position.value;
            return IsOverRelevantObject();
        }
        private bool IsOverRelevantObject() {
            this.screenRay = this.sourceCamera.ScreenPointToRay(this.screenPointerPosition);
            if (Physics.Raycast(this.screenRay, out this.screenRayHit, this.raycastMaxDistance, this.raycastLayerMask.value)) {
                this.activeReceiver = this.screenRayHit.transform.GetFirstComponentInParentRecursively<NewInputObjectInteractionReceiver>();
                if (this.activeReceiver) {
                    #if DEBUG2
                    this.Log($"Selected receiver {this.activeReceiver.name}");
                    #endif
                    return true;
                }
            }
            #if DEBUG2
            this.Log($"None Selected");
            #endif
            return false;
        }
        #endregion
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"Over Object Started");
            #endif
            this.activeReceiver?.HandleStarted(input);
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"Over Object Performed");
            #endif
            this.activeReceiver?.HandlePerformed(input);
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"Over Object Ended");
            #endif
            this.activeReceiver?.HandleEnded(input);
            this.activeReceiver = null;
        }
        #endregion

    }
}
