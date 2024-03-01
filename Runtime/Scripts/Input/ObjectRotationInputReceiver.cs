#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public class ObjectRotationInputReceiver : InputReceiver {
        [Header("Events")]
        [SerializeField] private InputActionReference moveInput;
        [SerializeField] private float placementRaycastMaxDistance = 1000f;
        [SerializeField] private LayerMask placementRaycastLayerMask;

        private Vector2 screenPointerPosition;
        private Ray ray;
        private RaycastHit hitInfo;

        ///////////////////////////////////////////////////////////////////////
        #region INPUT_HANDLING
        protected override void EnableInputEvents() {
            this.moveInput.action.started += MoveActionStarted;
            this.moveInput.action.canceled += MoveActionCanceled;
            this.moveInput.action.Enable();
        }
        protected override void DisableInputEvents() {
            this.moveInput.action.Disable();
            this.moveInput.action.started -= MoveActionStarted;
            this.moveInput.action.canceled -= MoveActionCanceled;
        }
        private void MoveActionStarted(InputAction.CallbackContext context) {
            TriggerStartInteraction();
        }
        private void MoveActionCanceled(InputAction.CallbackContext context) {
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
        protected override object OnInteractionPerformed() {
            this.screenPointerPosition = Pointer.current.position.ReadValue();
            this.ray = Camera.main.ScreenPointToRay(this.screenPointerPosition);
            if (Physics.Raycast(this.ray, out this.hitInfo, this.placementRaycastMaxDistance, this.placementRaycastLayerMask)) {
                return this.hitInfo;
            }
            return null;
        }
        protected override void UpdateActiveHandlers() {
            this.activeHandlers.Clear();
            if (!this.IsPointerOverUI) {
                this.screenPointerPosition = Pointer.current.position.ReadValue();
                this.ray = Camera.main.ScreenPointToRay(this.screenPointerPosition);
                if (Physics.Raycast(this.ray, out this.hitInfo, this.placementRaycastMaxDistance)) {
                    foreach (ObjectRotationInputHandler handler in this.currentHandlers) {
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