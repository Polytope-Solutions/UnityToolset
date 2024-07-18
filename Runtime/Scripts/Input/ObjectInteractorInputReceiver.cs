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
        [SerializeField] private LayerMask interactionRaycastLayerMask;
        [SerializeField] private float interactionRaycastMaxDistance = 1000f;

        private Vector2 screenPointerPosition;
        private Ray interactionRay;
        private RaycastHit interactionHitInfo;

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
        protected override void OnInteractionStarted() {
            TriggerPerformInteraction();
            if (!this.IsSelfManaged)
                InputManager.Instance.InputReceiverSetActiveExclusive(this.inputReceiverKeyName, true);
        }
        protected override void OnInteractionEnded() {
            if (!this.IsSelfManaged)
                InputManager.Instance.InputReceiverRestoreExclusive();
        }
        protected override RaycastHit? CurrentInteractionRay() {
            if (!this.IsPointerOverUI) {
                this.screenPointerPosition = Pointer.current.position.ReadValue();
                this.interactionRay = Camera.main.ScreenPointToRay(this.screenPointerPosition);
                if (Physics.Raycast(this.interactionRay, out this.interactionHitInfo, this.interactionRaycastMaxDistance, this.interactionRaycastLayerMask)) {
                    return this.interactionHitInfo;
                }
            }
            return null;
        }
    }
}
