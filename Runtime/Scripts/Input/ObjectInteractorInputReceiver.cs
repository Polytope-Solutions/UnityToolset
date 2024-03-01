#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public class ObjectInteractorInputReceiver : InputReceiver {
        [Header("Events")]
        [SerializeField] private InputActionReference pointerClickAction;
        [SerializeField] private float raycastMaxDistance = 1000f;

        private Vector2 screenPointerPosition;
        private Ray ray;
        private RaycastHit hitInfo;

        ///////////////////////////////////////////////////////////////////////
        #region INPUT_HANDLING
        protected override void EnableInputEvents() {
            this.pointerClickAction.action.started += ClickActionStarted;
            this.pointerClickAction.action.canceled += ClickActionCanceled;

            this.pointerClickAction.action.Enable();
        }

        protected override void DisableInputEvents() {
            this.pointerClickAction.action.Disable();

            this.pointerClickAction.action.started -= ClickActionStarted;
            this.pointerClickAction.action.canceled -= ClickActionCanceled;
        }

        private void ClickActionStarted(InputAction.CallbackContext context) {
            TriggerStartInteraction();
        }
        private void ClickActionCanceled(InputAction.CallbackContext context) {
            TriggerEndInteraction();
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        protected override void OnInteractionStart() {
            TriggerPerformInteraction();
            if (!this.IsSelfManaged)
                InputManager.Instance.InputReceiverSetActiveExclusive(this.inputReceiverKeyName, true);
        }
        protected override void OnInteractionEnded() {
            if (!this.IsSelfManaged)
                InputManager.Instance.InputReceiverRestoreExclusive();
        }
        protected override void UpdateActiveHandlers() {
            this.activeHandlers.Clear();
            if (!this.IsPointerOverUI) {
                this.screenPointerPosition = Pointer.current.position.ReadValue();
                this.ray = Camera.main.ScreenPointToRay(this.screenPointerPosition);
                if (Physics.Raycast(this.ray, out this.hitInfo, this.raycastMaxDistance)) {
                    foreach (ObjectInteractorInputHandler handler in this.currentHandlers) {
                        if (handler.DidRayHitHandler(this.hitInfo)) {
                            this.activeHandlers.Add(handler);
                            break;
                        }
                    }
                }
            }
        }
    }
}
