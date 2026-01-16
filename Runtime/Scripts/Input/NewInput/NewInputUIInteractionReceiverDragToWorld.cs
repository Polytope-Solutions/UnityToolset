using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.InputSystem;

using PolytopeSolutions.Toolset.GlobalTools;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputUIInteractionReceiverDragToWorld : NewInputUIInteractionReceiver {
        [SerializeField] protected GameObject goObjectToSpawn;
        [SerializeField] protected LayerMask validPlacementLayerMask, invalidPlacementLayerMask;
        [SerializeField, Layer] protected int tempLayer;
        [SerializeField] protected float maxDistance = 1000f;
        [SerializeField] protected UnityEvent<GameObject> onValidObjectPlaced;
        [SerializeField] protected float smoothResetTime = 0.2f;
        protected Vector3? lastValidPoint;
        protected Dictionary<Collider,int> initialLayers = new();
        protected GameObject goSpawnedObject;
        private int RaycastLayerMask => (this.validPlacementLayerMask.value | this.invalidPlacementLayerMask.value);

        protected virtual void Awake() {
            this.sourceCamera = Camera.main;
        }
        #region HANDLERS
        private bool isObjectValid;
        public override void HandleStarted(InputAction.CallbackContext input) {}
        public override void HandlePerformed(InputAction.CallbackContext input) {
            if (!this.goSpawnedObject && this.IsObjectReady) {
                InitObject();
            }
            if (!this.goSpawnedObject) return;
            TryPlaceObject();
        }
        public override void HandleEnded(InputAction.CallbackContext input) {
            if (!this.goSpawnedObject) return;
            if (!this.isObjectValid) {
                if (!this.lastValidPoint.HasValue) {
                    Destroy(this.goSpawnedObject);
                    this.goSpawnedObject = null;
                    return;
                }
                StartCoroutine(MoveAndReset());
                return;
            }

            ResetValidObject();
        }
        #endregion
        #region INTERNAL
        private void InitObject() {
            this.goSpawnedObject = Instantiate(this.goObjectToSpawn);
            Collider[] colliders = this.goSpawnedObject.GetComponentsInChildren<Collider>();
            this.initialLayers.Clear();
            foreach (Collider collider in colliders) {
                this.initialLayers.Add(collider, collider.gameObject.layer);
                collider.gameObject.layer = this.tempLayer;
            }
            this.isObjectValid = false;
            this.lastValidPoint = null;
        }
        private Camera sourceCamera;
        private Vector2 screenPointerPosition;
        private Ray screenRay;
        private RaycastHit screenRayHit;
        private void TryPlaceObject() {
            this.screenPointerPosition = Pointer.current.position.value;
            this.screenRay = this.sourceCamera.ScreenPointToRay(this.screenPointerPosition);
            float invalidDistance = this.maxDistance;
            if (Physics.Raycast(this.screenRay, out this.screenRayHit, this.maxDistance, this.RaycastLayerMask)) {
                if (this.screenRayHit.transform.gameObject.IsInLayerMask(this.validPlacementLayerMask)) {
                    // valid layer and distance
                    if (!this.isObjectValid)
                        HandleValid();
                    this.lastValidPoint = this.screenRayHit.point;
                    PlaceObject(this.screenRayHit.point, this.screenRayHit.normal);
                    return;
                }
                invalidDistance = this.screenRayHit.distance;
            }
            if (this.isObjectValid)
                HandleInvalid();
            PlaceObject(this.screenRay.origin + invalidDistance * this.screenRay.direction, Vector3.up);
        }
        private void PlaceObject(Vector3 place, Vector3 normal) {
            this.goSpawnedObject.transform.position = place;
            this.goSpawnedObject.transform.up = normal;
        }
        private IEnumerator MoveAndReset() {
            Vector3 velocity = Vector3.zero;
            while ((this.goSpawnedObject.transform.position - this.lastValidPoint.Value).sqrMagnitude > 0.0004f) {
                this.goSpawnedObject.transform.position = Vector3.SmoothDamp(
                    this.goSpawnedObject.transform.position, this.lastValidPoint.Value, ref velocity, this.smoothResetTime);
                yield return null;
            }
            this.goSpawnedObject.transform.position = this.lastValidPoint.Value;
            ResetValidObject();
        }
        private void ResetValidObject() {
            foreach (KeyValuePair<Collider, int> colliderHolder in this.initialLayers) {
                colliderHolder.Key.gameObject.layer = colliderHolder.Value;
            }
            this.onValidObjectPlaced?.Invoke(this.goSpawnedObject);
            this.goSpawnedObject = null;
        }
        #endregion
        #region OVERRIDABLE
        protected virtual bool IsObjectReady { get; } = true;
        protected virtual void HandleValid() {
            this.isObjectValid = true;
        }
        protected virtual void HandleInvalid() {
            this.isObjectValid = false;
        }
        #endregion
    }
}
