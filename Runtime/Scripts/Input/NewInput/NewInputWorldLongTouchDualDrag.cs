#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputWorldLongTouchDualDrag : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description = "WorldLongTouchDual";
        [SerializeField] private bool resetOnRelease = true;
        [SerializeField] private UnityEvent onStarted, onEnded;

        [SerializeField] private UnityEvent onPrimaryReset, onSecondaryReset;
        [SerializeField] private UnityEvent<Vector3, Vector3> onPrimaryInteract;
        [SerializeField] private UnityEvent<Vector3, Vector3, Vector3, Vector3> onDualInteract;
        
        private Camera rayCamera;
        private Touch[] currentTouches;
        private int currentTouchCount, previousTouchCount = -1;

        private TouchInfo primary, secondary;

        private class TouchInfo {
            private int touchIndex;
            //private int touchID = -1;
            private bool isResetPending = true;
            private UnityEvent onReset;
            private Plane interactionPlane;
            private Vector2 previousScreenPosition, currentScreenPosition;
            private Vector3 currentPreviousWorldPosition, currentCurrentWorldPosition;
            private Ray ray;

            public bool IsInProgress(Touch[] currentTouches) => currentTouches[this.touchIndex].inProgress;
            //public bool HasTouchIDChanged(Touch[] currentTouches) => this.touchID != currentTouches[this.touchIndex].touchId;
            public Vector3 CurrentPreviousWorldPosition => this.currentPreviousWorldPosition;
            public Vector3 CurrentCurrentWorldPosition => this.currentCurrentWorldPosition;

            public TouchInfo(int touchIndex, UnityEvent onReset) {
                this.touchIndex = touchIndex;
                this.onReset = onReset;
                Reset();
            }
            public void Reset() {
                this.isResetPending = true;
                //this.touchID = -1;
                this.onReset?.Invoke();
            }
            public bool Update(Touch[] currentTouches, Camera camera, Func<Vector2, Plane> evaluateInteractionPlane) {
                if (this.isResetPending) {
                    Init(currentTouches, evaluateInteractionPlane);
                }
                return EvaluateCurrentWorldPositions(currentTouches, camera);
            }

            private void Init(Touch[] currentTouches, Func<Vector2, Plane> evaluateInteractionPlane) {
                this.isResetPending = false;
                //this.touchID = currentTouches[this.touchIndex].touchId;
                this.previousScreenPosition = currentTouches[this.touchIndex].screenPosition;
                this.currentScreenPosition = currentTouches[this.touchIndex].screenPosition;
                this.currentPreviousWorldPosition = Vector3.zero;
                this.currentCurrentWorldPosition = Vector3.zero;
                this.interactionPlane = evaluateInteractionPlane.Invoke(this.currentScreenPosition);
            }
            private bool EvaluateCurrentWorldPositions(Touch[] currentTouches, Camera camera) {
                this.previousScreenPosition = this.currentScreenPosition;
                float distance;
                // Evaluate where previous position results in the world in the camera space
                this.ray = camera.ScreenPointToRay(this.previousScreenPosition.ToXY());
                if (!EvaluateRay(this.ray, out distance))
                    return false;
                this.currentPreviousWorldPosition = this.ray.GetPoint(distance);
                // Evaluate where previous position results in the world in the camera space
                this.currentScreenPosition = currentTouches[this.touchIndex].screenPosition;
                this.ray = camera.ScreenPointToRay(this.currentScreenPosition.ToXY());
                if (!EvaluateRay(this.ray, out distance))
                    return false;
                this.currentCurrentWorldPosition = this.ray.GetPoint(distance);
                return true;
            }
            protected virtual bool EvaluateRay(Ray ray, out float distance, float maxDistance=1000) {
                if (!this.interactionPlane.Raycast(ray, out distance))
                    return false;
                if (distance > maxDistance)
                    return false;
                return true;
            }
        }

        #region MANAGEMENT
        public string Description => this.description;
        public void Init() { 
            this.rayCamera = Camera.main;
            this.primary = new TouchInfo(0, this.onPrimaryReset);
            this.secondary = new TouchInfo(1, this.onSecondaryReset);
        }
        public bool IsApplicable(InputAction.CallbackContext input) {
            return true;
        }
        #endregion
        
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongTouchDual Started");
            #endif
            StartInteraction();
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongTouchDual Performed");
            #endif
            if (!CheckTouches())
                ;// EndInteraction();
            else {
                if (this.currentTouchCount > 0 && this.primary.IsInProgress(this.currentTouches)) {
                    if (this.primary.Update(this.currentTouches, this.rayCamera, EvaluateInteractionPlane)) {
                        // Primary touch is in progress and updated
                        this.onPrimaryInteract?.Invoke(
                            this.primary.CurrentPreviousWorldPosition, this.primary.CurrentCurrentWorldPosition
                        );
                        if (this.currentTouchCount > 1 && this.secondary.IsInProgress(this.currentTouches)){
                            if (this.secondary.Update(this.currentTouches, this.rayCamera, EvaluateInteractionPlane)) {
                                // Both touches are in progress and updated
                                this.onDualInteract?.Invoke(
                                    this.primary.CurrentPreviousWorldPosition, this.primary.CurrentCurrentWorldPosition,
                                    this.secondary.CurrentPreviousWorldPosition, this.secondary.CurrentCurrentWorldPosition
                                );
                            }
                        }
                    }
                }
            }
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongTouchDual Ended");
            #endif
            EndInteraction();
        }

        private bool CheckTouches() {
            this.currentTouches = Touch.activeTouches.ToArray();
            this.currentTouchCount = this.currentTouches.Length;
            #if DEBUG2
            this.Log($"Active touches: {this.currentTouchCount}");
            #endif
            if (this.previousTouchCount != this.currentTouchCount) {
                this.primary.Reset();
                this.secondary.Reset();
                this.previousTouchCount = this.currentTouchCount;
            }
            return (this.currentTouchCount != 0);
        }
        protected virtual Plane EvaluateInteractionPlane(Vector2 screenPosition) 
            => new Plane(Vector3.up, Vector3.zero);
        private void StartInteraction() {
            this.previousTouchCount = -1;
            this.onStarted?.Invoke();
            #if DEBUG2
            this.Log("WorldLongTouchDual Interaction Started");
            #endif
        }
        private void EndInteraction() {
            this.onEnded?.Invoke();
            #if DEBUG2
            this.Log("WorldLongTouchDual Interaction Ended");
            #endif
        }
        #endregion
    }
}
