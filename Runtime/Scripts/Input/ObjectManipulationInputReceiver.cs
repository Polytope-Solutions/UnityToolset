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
    public class ObjectManipulationInputReceiver : InputReceiver {
        [Header("Events")]
        [SerializeField] private InputActionReference moveInput;
        [SerializeField] private LayerMask interactionRaycastLayerMask;
        [SerializeField] private float interactionRaycastMaxDistance = 1000f;
        [SerializeField] private LayerMask placementRaycastLayerMask;
        [SerializeField] private float placementRaycastMaxDistance = 1000f;

        private Vector2 screenPointerPosition;
        private Ray interactionRay, placementRay;
        private RaycastHit interactionHitInfo, placementHitInfo;

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
            this.screenPointerPosition = Pointer.current.position.ReadValue();
            this.placementRay = Camera.main.ScreenPointToRay(this.screenPointerPosition);
            if (Physics.Raycast(this.placementRay, out this.placementHitInfo, this.placementRaycastMaxDistance, this.placementRaycastLayerMask)) {
                return this.placementHitInfo;
            }
            return null;
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