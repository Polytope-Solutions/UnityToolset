#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputShortWorldInteractionHandlerRay : NewInputShortInteractionHandler {
        [SerializeField] protected LayerMask validPlacementLayerMask, invalidPlacementLayerMask;
        [SerializeField] private float maxDistance = 1000f;
        [SerializeField] protected UnityEvent<Vector3> onValidSelect, onInvalidSelect;
        private int RaycastLayerMask => ((int)this.validPlacementLayerMask.value | (int)this.invalidPlacementLayerMask.value);

        #region MANAGEMENT
        public override void Init() {
            this.sourceCamera = Camera.main;
        }
        public override bool IsApplicable(InputAction.CallbackContext input) {
            this.screenPointerPosition = Pointer.current.position.value;
            EvaluateRay(this.screenPointerPosition, out _, out _, out _, out bool hit);
            return hit;
        }
        #endregion
        #region HANDLERS
        public override void HandleEnded(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log("Ray Short World Interaction");
            #endif
            this.screenPointerPosition = Pointer.current.position.value;
            EvaluateRay(this.screenPointerPosition, out Vector3 point, out _, out bool isValid, out bool hit);
            if (isValid)
                this.onValidSelect?.Invoke(point);
            else if (hit)
                this.onInvalidSelect?.Invoke(point);
        }
        #endregion

        private Camera sourceCamera;
        private Vector2 screenPointerPosition;
        private Ray screenRay;
        private RaycastHit screenRayHit;
        private void EvaluateRay(Vector2 screenPointerPosition, out Vector3 point, out Vector3 normal, out bool isValid, out bool hit) {
            this.screenRay = this.sourceCamera.ScreenPointToRay(screenPointerPosition);
            float invalidDistance = this.maxDistance;
            hit = false;
            if (Physics.Raycast(this.screenRay, out this.screenRayHit, this.maxDistance, this.RaycastLayerMask)) {
                hit = true;
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
    }
}
