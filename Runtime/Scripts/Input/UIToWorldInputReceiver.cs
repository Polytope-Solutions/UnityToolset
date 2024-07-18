#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public class UIToWorldInputReceiver : InputReceiver {
        [Header("Events")]
        [SerializeField] private InputActionReference pointerClickAction;
        [SerializeField] private float placementRaycastMaxDistance = 1000f;
        [SerializeField] private LayerMask placementRaycastLayerMask;

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
        protected override void OnInteractionStarted() {
            //TriggerPerformInteraction();
            if (!this.IsSelfManaged)
                InputManager.Instance.InputReceiverSetActiveExclusive(this.inputReceiverKeyName, true);
        }
        protected override void OnInteractionEnded() {
            if (!this.IsSelfManaged)
                InputManager.Instance.InputReceiverRestoreExclusive();
        }
        protected override object OnInteractionPerformed() {
            //if (this.IsPointerOverUI) return null;
            // Raycast to where it should be placed.
            this.screenPointerPosition = Pointer.current.position.ReadValue();
            this.ray = Camera.main.ScreenPointToRay(this.screenPointerPosition);
            if (Physics.Raycast(this.ray, out this.hitInfo, this.placementRaycastMaxDistance, this.placementRaycastLayerMask)) {
                return this.hitInfo;
            }
            return null;
        }
        protected override void UpdateActiveHandlers() {
            this.activeHandlers = new List<IInputHandler>(this.currentHandlers);
        }
    }
}