using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using PolytopeSolutions.Toolset.GlobalTools;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputObjectInteractionReceiverTouchManipulation : NewInputObjectInteractionReceiver {
        [SerializeField] protected LayerMask validPlacementLayerMask, invalidPlacementLayerMask;
        [SerializeField, Layer] protected int tempLayer;
        [SerializeField] private float maxDistance = 1000f;
        [SerializeField] private bool fixUpDirection = true;
        [SerializeField] protected float smoothResetTime = 0.2f;
        protected Vector3? lastValidPoint;
        protected virtual Vector3 UpDirection => Vector3.up;
        private int RaycastLayerMask => (this.validPlacementLayerMask.value | this.invalidPlacementLayerMask.value);

        protected Dictionary<Collider, int> initialLayers = new();
        private bool isObjectValid;
        private bool isPrimaryStarted, isSecondaryStarted;
        private Vector3 startPrimaryOffset, startSecondaryOffset;
        private Quaternion startRotation;
        protected virtual void Awake() {
            this.sourceCamera = Camera.main;
            Collider[] colliders = GetComponentsInChildren<Collider>();
            this.initialLayers.Clear();
            foreach (Collider collider in colliders)
                this.initialLayers.Add(collider, collider.gameObject.layer);
        }
        #region HANDLERS
        public override void HandleStarted(InputAction.CallbackContext input) {
            EnhancedTouchSupport.Enable();
            this.isPrimaryStarted = false;
            this.isSecondaryStarted = false;
            this.isObjectValid = false;
            foreach (Collider collider in this.initialLayers.Keys)
                collider.gameObject.layer = this.tempLayer;
        }
        public override void HandlePerformed(InputAction.CallbackContext input) {
            Vector2 primaryPosition, secondaryPosition;
            Vector3 startPrimaryPoint, currentPrimaryPoint, startSecondaryPoint, currentSecondaryPoint;
            bool isValidPlacement;
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count == 0) {
                primaryPosition = Pointer.current.position.value;
            }
            else if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0) {
                primaryPosition = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
            }
            else {
                // Something went wrong
                this.isPrimaryStarted = false;
                this.isSecondaryStarted = false;
                return;
            }
            if (!this.isPrimaryStarted) {
                EvaluateRay(primaryPosition, out startPrimaryPoint, out _, out _);
                this.startPrimaryOffset = transform.position - startPrimaryPoint;
                this.isPrimaryStarted = true;
            }
            EvaluateRay(primaryPosition, out currentPrimaryPoint, out _, out isValidPlacement);
            ApplyMove(currentPrimaryPoint);
            if (!this.isObjectValid && isValidPlacement)
                HandleValid();
            else if (this.isObjectValid && !isValidPlacement)
                HandleInvalid();
            this.isObjectValid = isValidPlacement;
            if (this.isObjectValid)
                this.lastValidPoint = transform.position;

            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 1) {
                secondaryPosition = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition;
            }
            else {
                // no longer has 2 touches
                this.isSecondaryStarted = false;
                return;
            }
            if (!this.isSecondaryStarted) {
                EvaluateRay(secondaryPosition, out startSecondaryPoint, out _, out _);
                this.startSecondaryOffset = startSecondaryPoint - currentPrimaryPoint;
                if (this.fixUpDirection)
                    this.startSecondaryOffset = this.startSecondaryOffset.DoubleCross(this.UpDirection);
                this.startRotation = transform.rotation;
                this.isSecondaryStarted = true;
            }
            EvaluateRay(secondaryPosition, out currentSecondaryPoint, out _, out _);
            ApplyRotation(currentPrimaryPoint, currentSecondaryPoint);
        }

        public override void HandleEnded(InputAction.CallbackContext input) {
            EnhancedTouchSupport.Disable();
            if (!this.isObjectValid) {
                if (!this.lastValidPoint.HasValue) {
                    // Should not happen, just in case
                    return;
                }
                StartCoroutine(MoveAndReset());
                return;
            }

            ResetValidObject();
        }
        #endregion

        private Camera sourceCamera;
        private Ray screenRay;
        private RaycastHit screenRayHit;
        private void EvaluateRay(Vector2 screenPointerPosition, out Vector3 point, out Vector3 normal, out bool isValid) {
            this.screenRay = this.sourceCamera.ScreenPointToRay(screenPointerPosition);
            float invalidDistance = this.maxDistance;
            if (Physics.Raycast(this.screenRay, out this.screenRayHit, this.maxDistance, this.RaycastLayerMask)) {
                if (this.screenRayHit.transform.gameObject.IsInLayerMask(this.validPlacementLayerMask)) {
                    // valid layer and distance
                    point = this.screenRayHit.point;
                    normal = this.screenRayHit.normal;
                    isValid = true;
                    return;
                }
                invalidDistance = this.screenRayHit.distance;
            }
            point = this.screenRay.origin + invalidDistance * this.screenRay.direction;
            normal = Vector3.up;
            isValid = false;
        }
        private void ApplyMove(Vector3 currentPrimaryPoint) {
            transform.position = currentPrimaryPoint + this.startPrimaryOffset;
        }
        private void ApplyRotation(Vector3 currentPrimaryPoint, Vector3 currentSecondaryPoint) {
            Vector3 currentSecondaryOffset = currentSecondaryPoint - currentPrimaryPoint;
            if (this.fixUpDirection)
                currentSecondaryOffset = currentSecondaryOffset.DoubleCross(this.UpDirection);
            transform.rotation
                = Quaternion.FromToRotation(this.startSecondaryOffset, currentSecondaryOffset) * this.startRotation;
        }
        private IEnumerator MoveAndReset() {
            Vector3 velocity = Vector3.zero;
            while ((transform.position - this.lastValidPoint.Value).sqrMagnitude > 0.001f) {
                transform.position = Vector3.SmoothDamp(
                    transform.position, this.lastValidPoint.Value, ref velocity, this.smoothResetTime);
                yield return null;
            }
            transform.position = this.lastValidPoint.Value;
            ResetValidObject();
        }
        private void ResetValidObject() {
            foreach (KeyValuePair<Collider, int> colliderHolder in this.initialLayers) {
                colliderHolder.Key.gameObject.layer = colliderHolder.Value;
            }
        }
        #region OVERRIDABLE
        protected virtual void HandleValid() {
            this.isObjectValid = true;
        }
        protected virtual void HandleInvalid() {
            this.isObjectValid = false;
        }
        #endregion
    }
}
