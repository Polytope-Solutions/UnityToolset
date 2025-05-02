using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputManager : TManager<NewInputManager> {
        [System.Serializable]
        private class ActionPairing {
            [SerializeField] private InputActionReference action;
            [SerializeField] private InterfaceReference<INewInputHandler> rootHandler;
            private bool isApplicable;

            public void Enable() {
                if (this.rootHandler == null) return;
                this.action.action.started += OnStarted;
                this.action.action.performed += OnPerformed;
                this.action.action.canceled += OnEnded;
                this.action.action.Enable();
                this.rootHandler.Value.Init();
            }
            public void Disable() {
                if (this.rootHandler == null) return;
                this.action.action.Disable();
                this.action.action.started -= OnStarted;
                this.action.action.performed -= OnPerformed;
                this.action.action.canceled -= OnEnded;
            }
            private void OnStarted(InputAction.CallbackContext context) {
                if (this.rootHandler == null) return;
                this.isApplicable = this.rootHandler.Value.IsApplicable(context);
                if (this.isApplicable)
                    this.rootHandler.Value.HandleStarted(context);
            }
            private void OnPerformed(InputAction.CallbackContext context) {
                if (this.rootHandler == null || !this.isApplicable) return;
                this.rootHandler.Value.HandlePerformed(context);
            }
            private void OnEnded(InputAction.CallbackContext context) {
                if (this.rootHandler == null || !this.isApplicable) return;
                this.rootHandler.Value.HandleEnded(context);
            }
        }
        [SerializeField] private List<ActionPairing> actionPairings;
        [SerializeField] private float minDuration = .5f;
        [SerializeField] private bool useEnhancedTouch = true;
        public float MinDuration => this.minDuration;

        protected override void Awake() {
            base.Awake();
            if (this.useEnhancedTouch)
                EnhancedTouchSupport.Enable();
            for (int i = 0; i < this.actionPairings.Count; i++) {
                this.actionPairings[i].Enable();
            }
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            if (this.useEnhancedTouch)
                EnhancedTouchSupport.Disable();
            for (int i = 0; i < this.actionPairings.Count; i++) {
                this.actionPairings[i].Disable();
            }
        }

        private GraphicRaycaster[] raycasters;
        private PointerEventData pointerEventData;
        private bool isOverUIThisFrame = false;
        private int frameOfPointerUpdate = -1;
        public bool IsOverUI(out List<RaycastResult> rayCastUIResults) {
            rayCastUIResults = new();
            // Check if already dcalculated this frame and return cached result
            if (this.frameOfPointerUpdate == Time.frameCount)
                return this.isOverUIThisFrame;
            // Otherwise, calculate and cache the result
            this.frameOfPointerUpdate = Time.frameCount;
            this.pointerEventData = new PointerEventData(EventSystem.current);
            this.pointerEventData.position = Pointer.current.position.value;
            this.raycasters = GameObject.FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
            foreach (GraphicRaycaster raycaster in this.raycasters) {
                raycaster.Raycast(this.pointerEventData, rayCastUIResults);
                if (rayCastUIResults.Count > 0) {
                    this.isOverUIThisFrame = true;
                    return this.isOverUIThisFrame;
                }
            }
            this.isOverUIThisFrame = rayCastUIResults.Count > 0;
            return this.isOverUIThisFrame;
        }
    }
}
