using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
	[RequireComponent(typeof(Rigidbody))]
    public class ThirdPersonCameraController : MonoBehaviour {
        [Header("Events")]
        [SerializeField] private InputActionReference cameraRotateLeftRight;
        [SerializeField] private InputActionReference cameraRotateUpDown;
        [SerializeField] private InputActionReference cameraMoveLeftRight;
        [SerializeField] private InputActionReference cameraMoveForwardBackward;
        [SerializeField] private InputActionReference cameraMoveZoomInOut;

        [Header("General")]
        [SerializeField] private Transform tCamera;
        [SerializeField] private Transform tTarget;
        [SerializeField] private Vector2 distanceRange;
        [SerializeField] private Vector2 verticalAngleRange;
        //[SerializeField] private Vector2 horizontalAngleRange;

        [SerializeField] private float rotateSpeed = 25f;
        [SerializeField] private float moveSpeed = 2f;


        private new Rigidbody rigidbody;
        private Camera cCamera;
        private float farCornerDistanceCache;

        public delegate void OnCameraViewChanged();
        public OnCameraViewChanged onCameraViewChanged = null;

        private float rotateLeftRightValue;
        private float rotateUpDownValue;
        private float moveLeftRightValue;
        private float moveForwardBackwardValue;
        private float moveZoomInOutValue;

        ///////////////////////////////////////////////////////////////////////
        private void OnEnable() {
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

            this.cameraRotateLeftRight.action.performed += RotateLeftRightPerformed;
            this.cameraRotateUpDown.action.performed += RotateUpDownPerformed;
            this.cameraMoveLeftRight.action.performed += MoveLeftRightPerformed;
            this.cameraMoveForwardBackward.action.performed += MoveForwardBackwardPerformed;
            this.cameraMoveZoomInOut.action.performed += MoveZoomInOutPerformed;
            this.cameraRotateLeftRight.action.canceled += RotateLeftRightEnded;
            this.cameraRotateUpDown.action.canceled += RotateUpDownEnded;
            this.cameraMoveLeftRight.action.canceled += MoveLeftRightEnded;
            this.cameraMoveForwardBackward.action.canceled += MoveForwardBackwardEnded;
            this.cameraMoveZoomInOut.action.canceled += MoveZoomInOutEnded;

            this.cameraRotateLeftRight.action.Enable();
            this.cameraRotateUpDown.action.Enable();
            this.cameraMoveLeftRight.action.Enable();
            this.cameraMoveForwardBackward.action.Enable();
            this.cameraMoveZoomInOut.action.Enable();
        }
        private void OnDisable() { 
            this.cameraRotateLeftRight.action.Disable();
            this.cameraRotateUpDown.action.Disable();
            this.cameraMoveLeftRight.action.Disable();
            this.cameraMoveForwardBackward.action.Disable();
            this.cameraMoveZoomInOut.action.Disable();

            this.cameraRotateLeftRight.action.performed -= RotateLeftRightPerformed;
            this.cameraRotateUpDown.action.performed -= RotateUpDownPerformed;
            this.cameraMoveLeftRight.action.performed -= MoveLeftRightPerformed;
            this.cameraMoveForwardBackward.action.performed -= MoveForwardBackwardPerformed;
            this.cameraMoveZoomInOut.action.performed -= MoveZoomInOutPerformed;
            this.cameraRotateLeftRight.action.canceled -= RotateLeftRightEnded;
            this.cameraRotateUpDown.action.canceled -= RotateUpDownEnded;
            this.cameraMoveLeftRight.action.canceled -= MoveLeftRightEnded;
            this.cameraMoveForwardBackward.action.canceled -= MoveForwardBackwardEnded;
            this.cameraMoveZoomInOut.action.canceled -= MoveZoomInOutEnded;
        }
        private void FixedUpdate() {
            Vector3 currentPosition = this.tCamera.position;
            Quaternion currentRotation = this.tCamera.rotation;
            HandleInputs();
            ConstrainCameraToTraget();
            if (currentPosition != this.tCamera.position || currentRotation != this.tCamera.rotation)
                this.onCameraViewChanged?.Invoke();
        }
        private void OnDrawGizmos() {
            if (this.tCamera == null || this.tTarget == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.tCamera.position, this.tTarget.position);
        }
        ///////////////////////////////////////////////////////////////////////
        private void HandleInputs() { 
			this.tCamera.RotateAround(
				this.tTarget.position,
				Vector3.up, 
				this.rotateLeftRightValue * Time.fixedDeltaTime
			);
            this.tCamera.transform.RotateAround(
                this.tTarget.position,
                this.tCamera.right,
                this.rotateUpDownValue * Time.fixedDeltaTime
            );
            Vector3 direction =
                -this.tCamera.right * this.moveLeftRightValue +
                this.tCamera.forward.XZ().ToXZ() * this.moveForwardBackwardValue +
                this.tCamera.forward * this.moveZoomInOutValue;
            this.rigidbody.MovePosition(
                transform.position + direction * Time.fixedDeltaTime
            );
        }
        private void ConstrainCameraToTraget() {
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
            this.tCamera.position = this.tTarget.position - lookDirection * distance;
            this.tCamera.LookAt(this.tTarget);
        }
        ///////////////////////////////////////////////////////////////////////
        private void MoveLeftRightPerformed(InputAction.CallbackContext context) {
            this.moveLeftRightValue = context.ReadValue<float>() * this.moveSpeed;
        }
        private void MoveLeftRightEnded(InputAction.CallbackContext context) {
            this.moveLeftRightValue = 0f;
        }
        private void MoveForwardBackwardPerformed(InputAction.CallbackContext context) {
            this.moveForwardBackwardValue = context.ReadValue<float>() * this.moveSpeed;
        }
        private void MoveForwardBackwardEnded(InputAction.CallbackContext context){
            this.moveForwardBackwardValue = 0f;
        }
        private void MoveZoomInOutPerformed(InputAction.CallbackContext context) {
            this.moveZoomInOutValue = context.ReadValue<float>() * this.moveSpeed;
        }
        private void MoveZoomInOutEnded(InputAction.CallbackContext context) {
            this.moveZoomInOutValue = 0f;
        }
        private void RotateLeftRightPerformed(InputAction.CallbackContext context)
        {
            this.rotateLeftRightValue = context.ReadValue<float>() * this.rotateSpeed;
        }
        private void RotateLeftRightEnded(InputAction.CallbackContext context) {
            this.rotateLeftRightValue = 0f;
        }
        private void RotateUpDownPerformed(InputAction.CallbackContext context) {
            this.rotateUpDownValue = context.ReadValue<float>() * this.rotateSpeed;
        }
        private void RotateUpDownEnded(InputAction.CallbackContext context) {
            this.rotateUpDownValue = 0f;
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
