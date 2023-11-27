using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    [RequireComponent(typeof(Rigidbody))]
    public abstract class CameraController : InputReceiver {
        // NB! If overriding OnEnable call the base version in derived classes

        [Header("General")]
        [SerializeField] protected Transform tCamera;

        protected new Rigidbody rigidbody;
        protected Camera cCamera;

        private float epsilon = 0.0001f;
        private Coroutine movementMonitor;
        private float farCornerDistanceCache;
        public event Action onCameraViewChanged = null;

        protected override void OnEnable() { 
            base.OnEnable();
            ObjectSetup();
        }
        protected void ObjectSetup() {
            this.rigidbody = gameObject.GetComponent<Rigidbody>();
            this.cCamera = this.tCamera?.GetComponent<Camera>();
            if (this.tCamera == null){
                this.cCamera = Camera.main;
                this.tCamera = this.cCamera?.transform;
            }

            float frustumHeight = 2.0f * this.cCamera.farClipPlane * Mathf.Tan(this.cCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * this.cCamera.aspect;
            this.farCornerDistanceCache = Mathf.Sqrt(
                Mathf.Pow(this.cCamera.farClipPlane, 2) +
                Mathf.Pow(frustumHeight / 2, 2) +
                Mathf.Pow(frustumWidth / 2, 2)
            );

            if (this.movementMonitor == null)
                this.movementMonitor = StartCoroutine(MovementMonitor());
        }
        private IEnumerator MovementMonitor() {
            Vector3 lastPosition, currentPosition;
            Quaternion lastRotation, currentRotation;
            lastPosition = this.tCamera.position;
            lastRotation = this.tCamera.rotation;
            while (true) {
                currentPosition = this.tCamera.position;
                currentRotation = this.tCamera.rotation;

                if (this.onCameraViewChanged != null
                    && ((currentPosition - lastPosition).sqrMagnitude > this.epsilon
                        || (currentRotation.ToVector4() - lastRotation.ToVector4()).sqrMagnitude > this.epsilon))
                    this.onCameraViewChanged?.Invoke();
                lastPosition = currentPosition;
                lastRotation = currentRotation;
                yield return new WaitForEndOfFrame();
            }
        }
        ///////////////////////////////////////////////////////////////////////
        public List<Vector3> FieldOfViewBoundaries(Plane plane) { 
            List<Vector3> boundaryPoints = new List<Vector3>();
            foreach (Vector3 corner in 
                new List<Vector3> { 
                    Vector3.zero, 
                    new Vector3(0, 1, 0), 
                    new Vector3(1, 1, 0), 
                    new Vector3(1, 0, 0) 
                }) {
                Ray temp = this.cCamera.ViewportPointToRay(corner);
                if (plane.Raycast(temp, out float distance)){
                    distance = Mathf.Clamp(distance, 0, this.farCornerDistanceCache);
                    boundaryPoints.Add(temp.origin + temp.direction * distance);
                } else {
                    // Ray does not intersect with plane - snap the corner to the relevant plane
                    boundaryPoints.Add(
                        plane.ClosestPointOnPlane(temp.origin + temp.direction * this.farCornerDistanceCache)
                    );
                }
            }
            return boundaryPoints;
        }
    }
}