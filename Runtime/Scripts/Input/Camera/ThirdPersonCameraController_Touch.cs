#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    public class ThirdPersonCameraController_Touch : CameraInputProvider {
        [Header("Events")]
        [SerializeField] private InputActionReference touchAny;

        [Header("Movement")]
        [SerializeField] private Vector2 verticalAngleRange = new Vector2(15, 85);
        [SerializeField] private float maxRayDistance = 1000f;
        [SerializeField] private float discardTime = 1 / 60f;
        protected virtual Vector3 UpDirection => Vector3.up;

        private Vector2 primaryTouchPreviousPosition, primaryTouchCurrentPosition;
        private Vector2 secondaryTouchPreviousPosition, secondaryTouchCurrentPosition;

        private Vector3 primaryTouchPreviousGamePosition, primaryTouchCurrentGamePosition;
        private Ray primaryTouchRay;
        private Vector3 secondaryTouchPreviousGamePosition, secondaryTouchCurrentGamePosition;
        private Vector3 differencePrevious, differenceCurrent;
        private Ray secondaryTouchRay;

        private Plane interactionPlane = new Plane(Vector3.up, Vector3.zero);
        protected virtual bool EvaluateRay(Ray ray, out float distance, bool discardMax = false) {
            if (!interactionPlane.Raycast(ray, out distance))
                return false;
            if (discardMax && distance > this.maxRayDistance)
                return false;
            return true;
        }

        ///////////////////////////////////////////////////////////////////////
        #region INPUT_HANDLING
        protected override void EnableInputEvents() {
            this.touchAny.action.started += TouchStarted;
            this.touchAny.action.canceled += TouchCanceled;

            this.touchAny.action.Enable();
            EnhancedTouchSupport.Enable();
        }

        protected override void DisableInputEvents() {
            this.touchAny.action.Disable();
            this.touchAny.action.started -= TouchStarted;
            this.touchAny.action.canceled -= TouchCanceled;
        }

        private void TouchStarted(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            if (CheckTouches()) {
                if (!this.IsPointerOverUI) { 
                    startTime = Time.time;
                    TriggerStartInteraction();
                    //TriggerPerformInteraction();
                }
            }
        }
        private void TouchCanceled(InputAction.CallbackContext context) {
            if (!CheckTouches()) {
                TriggerEndInteraction();
            }
        }

        #endregion
        ///////////////////////////////////////////////////////////////////////
        Quaternion objectRotation;
        float scale;
        float distance;
        bool resetPrimaryTouch = false;
        bool resetSecondaryTouch = false;
        int previousTouchCount = -1, currentTouchCount;
        int primaryTouchId = -1, secondaryTouchId = -1;
        float startTime = -1;

        UnityEngine.InputSystem.EnhancedTouch.Touch[] currentTouches;

        protected override object OnInteractionPerformed() {
            if (!this.IsInputEnabled) {
                return null;
            }
            objectRotation = Quaternion.identity;
            scale = 1f;

            if (!CheckTouches()) { 
                TriggerEndInteraction();
            } else if (currentTouchCount > 0 && currentTouches[0].inProgress) {
                // Check primary
                if (this.resetPrimaryTouch && Time.time - startTime > this.discardTime)
                    ResetPrimaryTouch();
                if (!this.resetPrimaryTouch) {
                    if (UpdatePimaryTouch()) {
                        // Check secondary
                        if (currentTouchCount > 1 && currentTouches[1].inProgress) {
                            if (this.resetSecondaryTouch)
                                ResetSecondaryTouch();
                            else 
                                UpdateSecondaryTouch();
                        }
                    }
                }
            }

            ModifyRigDirect(this.UpDirection, this.primaryTouchPreviousGamePosition, this.primaryTouchCurrentGamePosition, objectRotation, scale);
            ConstrainCameraToTarget(this.verticalAngleRange);
            return null;
        }

        #region TOUCH_HANDLING
        private bool CheckTouches() {
            currentTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.ToArray();
            currentTouchCount = currentTouches.Length;
            #if DEBUG2
            this.Log($"Active touches: {currentTouches.Length}");
            #endif
            if (previousTouchCount != currentTouchCount) {
                this.resetPrimaryTouch = true;
                this.primaryTouchId = -1;
                this.resetSecondaryTouch = true;
                this.secondaryTouchId = -1;
                previousTouchCount = currentTouchCount;
            }
            return (currentTouchCount != 0);
        }
        private void ResetPrimaryTouch() {
            this.primaryTouchPreviousGamePosition = Vector3.zero;
            this.primaryTouchCurrentGamePosition = Vector3.zero;
            this.primaryTouchCurrentPosition = currentTouches[0].screenPosition;
            this.primaryTouchPreviousPosition = this.primaryTouchCurrentPosition;
            this.resetPrimaryTouch = false;
            this.primaryTouchId = currentTouches[0].touchId;
        }
        private bool UpdatePimaryTouch() {
            this.primaryTouchPreviousPosition = this.primaryTouchCurrentPosition;
            //Shoot ray from from current touch position
            primaryTouchRay = this.Camera.ScreenPointToRay(this.primaryTouchPreviousPosition.ToXY());
            if (!EvaluateRay(primaryTouchRay, out distance, true))
                return false;
            primaryTouchPreviousGamePosition = primaryTouchRay.GetPoint(distance);
            //primaryTouchCurrentOffset = this.primaryTouchRay.origin - this.primaryTouchCurrentGamePosition;

            //Shoot ray from from current touch position
            primaryTouchCurrentPosition = currentTouches[0].screenPosition;
            primaryTouchRay = this.Camera.ScreenPointToRay(this.primaryTouchCurrentPosition.ToXY());
            if (!EvaluateRay(primaryTouchRay, out distance))
                return false;
            primaryTouchCurrentGamePosition = primaryTouchRay.GetPoint(distance);
            //primaryTouchCurrentOffset = this.primaryTouchRay.origin - this.primaryTouchCurrentGamePosition;

            //If moved - adjust object's position.
            //objectMoveDelta = this.primaryTouchPreviousGamePosition - this.primaryTouchCurrentGamePosition;

            return true;
        }
        private void ResetSecondaryTouch() {
            this.secondaryTouchCurrentPosition = currentTouches[1].screenPosition;
            this.secondaryTouchPreviousPosition = this.secondaryTouchCurrentPosition;
            this.secondaryTouchId = currentTouches[1].touchId;
            this.resetSecondaryTouch = false;
        }
        private void UpdateSecondaryTouch() {
            secondaryTouchPreviousPosition = this.secondaryTouchCurrentPosition;
            //Shoot ray from from current touch position
            secondaryTouchRay = this.Camera.ScreenPointToRay(secondaryTouchPreviousPosition.ToXY());
            if (!EvaluateRay(secondaryTouchRay, out distance, true))
                return;
            secondaryTouchPreviousGamePosition = secondaryTouchRay.GetPoint(distance);
            differencePrevious = secondaryTouchPreviousGamePosition - primaryTouchPreviousGamePosition;

            secondaryTouchCurrentPosition = currentTouches[1].screenPosition;
            //Shoot ray from from current touch position
            secondaryTouchRay = this.Camera.ScreenPointToRay(secondaryTouchCurrentPosition.ToXY());
            if (!EvaluateRay(secondaryTouchRay, out distance))
                return;
            secondaryTouchCurrentGamePosition = secondaryTouchRay.GetPoint(distance);
            differenceCurrent = secondaryTouchCurrentGamePosition - primaryTouchCurrentGamePosition;

            // 
            float currentMagnitude = differenceCurrent.magnitude;
            float previousMagnitude = differencePrevious.magnitude;
            if (currentMagnitude != 0 && previousMagnitude != 0) {
                objectRotation = Quaternion.FromToRotation(differencePrevious, differenceCurrent);
                scale = previousMagnitude / currentMagnitude;
            }
        }
        #endregion

    }
}