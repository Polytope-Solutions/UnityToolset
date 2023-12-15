#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.InputSystem;
using System;

namespace PolytopeSolutions.Toolset.Input {
    public class ObjectInteractorInputReceiver : InputReceiver {
        [Header("Events")]
        [SerializeField] private InputActionReference pointerClickAction;
        [SerializeField] private float raycastMaxDistance = 1000f;

        private bool IsPointerOverUI => InputManager.Instance.IsPointerOverUI;
        private bool isStartingInteraction;
        private bool isEndingInteraction;

        private Vector2 screenPointerPosition;
        private Ray ray;
        private RaycastHit hitInfo;

        private List<InteractableObjectInputHandler> currentHandlers = new List<InteractableObjectInputHandler>();
        private InteractableObjectInputHandler activeHandler;

        ///////////////////////////////////////////////////////////////////////
        #region UNITY_FUNCTIONS
        private void Update() {
            HandleInputValues();
        }
        #endregion
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
            this.isStartingInteraction = true;
        }
        private void ClickActionCanceled(InputAction.CallbackContext context) {
            this.isEndingInteraction = true;
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        private void HandleInputValues() {
            // Handle Start
            if (this.isStartingInteraction) {
                if (!this.IsPointerOverUI) {
                    this.screenPointerPosition = Pointer.current.position.ReadValue();
                    this.ray = Camera.main.ScreenPointToRay(this.screenPointerPosition);
                    if (Physics.Raycast(this.ray, out this.hitInfo, this.raycastMaxDistance)) {
                        foreach (InteractableObjectInputHandler handler in this.currentHandlers) {
                            if (handler.IsRayValid(this.hitInfo)) { 
                                // Found relevant handler
                                if (!this.IsSelfManaged)
                                    InputManager.Instance.InputReceiverSetActiveExclusive(this.inputReceiverKeyName, true);
                                this.activeHandler = handler;
                                this.activeHandler.OnInteractionStarted();
                                break;
                            }
                        }
                    }
                }
                this.isStartingInteraction = false;
            }
            // Handle End
            if (this.isEndingInteraction) {
                this.isEndingInteraction = false;
                if (this.activeHandler) {
                    this.activeHandler.OnInteractionEnded();
                    this.activeHandler = null;
                    if (!this.IsSelfManaged)
                        InputManager.Instance.InputReceiverRestoreExclusive();
                }
            }
        }

        public void RegisterInputHandler(InteractableObjectInputHandler handler) {
            if (!this.currentHandlers.Contains(handler))
                this.currentHandlers.Add(handler);
        }
        public void UnregisterInputHandler(InteractableObjectInputHandler handler) {
            if (this.currentHandlers.Contains(handler))
                this.currentHandlers.Remove(handler);
        }
    }
}
