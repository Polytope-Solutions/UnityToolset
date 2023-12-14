#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    public class UIToWorldInputReceiver : InputReceiver {
        [Header("Events")]
        [SerializeField] private InputActionReference pointerClickAction;
        [SerializeField] private InputActionReference pointerPositionAction;
        private bool isStartingInteraction;
        private bool isEndingInteraction;
        private bool isInteracting;

        private HashSet<UIToWorldInputHandler> currentHandlersHovering = new HashSet<UIToWorldInputHandler>();
        private List<UIToWorldInputHandler> activeHandlers = new List<UIToWorldInputHandler>();

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

            this.pointerPositionAction.action.performed += ClickActionPerformed;
            this.pointerPositionAction.action.Enable();
        }
        private void ClickActionCanceled(InputAction.CallbackContext context) {
            this.isEndingInteraction = true;

            this.pointerPositionAction.action.Disable();
            this.pointerPositionAction.action.performed -= ClickActionPerformed;
        }

        private void ClickActionPerformed(InputAction.CallbackContext context) {
            Vector2 screenPointerPosition = context.ReadValue<Vector2>();
            this.activeHandlers.ForEach((handler) => handler.OnInteractionPerformed(screenPointerPosition));
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        private void HandleInputValues() {
            if (this.isStartingInteraction) {
                bool isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
                if (isPointerOverUI && !this.IsSelfManaged && this.currentHandlersHovering.Count>0) {
                    this.activeHandlers = new List<UIToWorldInputHandler>(this.currentHandlersHovering);
                    this.activeHandlers.ForEach((handler) => handler.OnInteractionStarted());
                    #if DEBUG2
                    this.Log($"Starting interaction. Active Handlers: [{this.activeHandlers.Count}]. Block other interactors.");
                    #endif
                    this.isInteracting = true;
                    InputManager.Instance.InputReceiverSetActiveExclusive(this.inputReceiverKeyName, true);
                }
                this.isStartingInteraction = false;
            }
            if (this.isEndingInteraction) {
                this.isEndingInteraction = false;
                if (this.isInteracting && !this.IsSelfManaged && this.activeHandlers.Count > 0) {
                    this.activeHandlers.ForEach((handler) => handler.OnInteractionEnded());
                    this.activeHandlers.Clear();
                    #if DEBUG2
                    this.Log($"Ending interaction. Active Handlers: [{this.activeHandlers.Count}]. Unblock other interactors.");
                    #endif
                    this.isInteracting = false;
                    InputManager.Instance.InputReceiverSetActiveExclusive(this.inputReceiverKeyName, false);
                }
            }
        }

        public void HandlerHoverEnter(UIToWorldInputHandler handler) {
            this.currentHandlersHovering.Add(handler);
        }

        public void HandlerHoverExit(UIToWorldInputHandler handler) {
            this.currentHandlersHovering.Remove(handler);
        }
    }
}