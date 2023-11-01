using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Input {
    public class ThirdPersonCameraController : MonoBehaviour {
        [SerializeField] private Transform tCamera;
        [SerializeField] private Transform tTarget;
        [SerializeField] private Vector2 distanceRange;
        [SerializeField] private Vector2 verticalAngleRange;
        [SerializeField] private Vector2 horizontalAngleRange;
        private Camera cCamera;
        private float farCornerDistanceCache;

        public delegate void OnCameraViewChanged();
        public OnCameraViewChanged onCameraViewChanged;

        ///////////////////////////////////////////////////////////////////////
        private void OnEnable() {
            this.cCamera = this.tCamera?.GetComponent<Camera>();
            if (this.tCamera == null){
                this.cCamera = Camera.main;
                this.tCamera = this.cCamera.transform;
            }
            float frustumHeight = 2.0f * this.cCamera.farClipPlane * Mathf.Tan(this.cCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * this.cCamera.aspect;
            this.farCornerDistanceCache = Mathf.Sqrt(
                Mathf.Pow(this.cCamera.farClipPlane, 2) +
                Mathf.Pow(frustumHeight / 2, 2) +
                Mathf.Pow(frustumWidth / 2, 2)
            );
        }
        private void Update() {
            Vector3 lookDirection = this.tTarget.position - this.tCamera.position;
            float distance = lookDirection.magnitude;
            // Ensure camera within distance range
            distance = Mathf.Clamp(distance, this.distanceRange.x, this.distanceRange.y);
            // Ensure camera is in correct angular position
            Vector3 inPlaneNormal = Vector3.Cross(lookDirection, Vector3.up);
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(lookDirection, Vector3.up);
            float verticalAngle = Vector3.Angle(horizontalDirection, lookDirection);
            verticalAngle = Mathf.Clamp(verticalAngle, this.verticalAngleRange.x, this.verticalAngleRange.y);
            lookDirection = Quaternion.AngleAxis(-verticalAngle, inPlaneNormal) * horizontalDirection.normalized;
            // Update the position and rotation of the camera
            Vector3 currentPosition = this.tCamera.position;
            Quaternion currentRotation = this.tCamera.rotation;
            this.tCamera.position = this.tTarget.position - lookDirection * distance;
            this.tCamera.LookAt(this.tTarget);
            if (this.onCameraViewChanged != null
                && currentPosition != this.tCamera.position || currentRotation != this.tCamera.rotation)
                this.onCameraViewChanged.Invoke();
        }
        private void OnDrawGizmos() {
            if (this.tCamera == null || this.tTarget == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.tCamera.position, this.tTarget.position);
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
