using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    public class ThirdPersonCameraController : CameraController {
        [SerializeField] protected Transform tTarget;

        [Header("Events")]
        [SerializeField] private InputActionReference cameraRotateLeftRight;
        [SerializeField] private InputActionReference cameraRotateUpDown;
        [SerializeField] private InputActionReference cameraMoveLeftRight;
        [SerializeField] private InputActionReference cameraMoveForwardBackward;
        [SerializeField] private InputActionReference cameraMoveZoomInOut;

        [Header("Movement")]
        [SerializeField] private Vector2 distanceRange;
        [SerializeField] private Vector2 verticalAngleRange;
        //[SerializeField] private Vector2 horizontalAngleRange;

        [SerializeField] private float rotateSpeed = 25f;
        [SerializeField] private float moveSpeed = 2f;

        protected virtual Vector3 UpDirection => Vector3.up;

        private float rotateLeftRightValue;
        private float rotateUpDownValue;
        private float moveLeftRightValue;
        private float moveForwardBackwardValue;
        private float moveZoomInOutValue;

        ///////////////////////////////////////////////////////////////////////
        #region UNITY_FUNCTIONS
        protected virtual void FixedUpdate() {
            HandleInputValues();
            ConstrainCameraToTraget();
        }
        protected virtual void OnDrawGizmos() {
            if (this.tCamera == null || this.tTarget == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.tCamera.position, this.tTarget.position);
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        #region INPUT_HANDLING
        protected override void EnableInputEvents() {
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
        protected override void DisableInputEvents() {
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

            this.rotateLeftRightValue = 0f;
            this.rotateUpDownValue = 0f;
            this.moveLeftRightValue = 0f;
            this.moveForwardBackwardValue = 0f;
            this.moveZoomInOutValue = 0f;
        }
        private void MoveLeftRightPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.moveLeftRightValue = context.ReadValue<float>() * this.moveSpeed;
        }
        private void MoveLeftRightEnded(InputAction.CallbackContext context) {
            this.moveLeftRightValue = 0f;
        }
        private void MoveForwardBackwardPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.moveForwardBackwardValue = context.ReadValue<float>() * this.moveSpeed;
        }
        private void MoveForwardBackwardEnded(InputAction.CallbackContext context){
            this.moveForwardBackwardValue = 0f;
        }
        private void MoveZoomInOutPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.moveZoomInOutValue = context.ReadValue<float>() * this.moveSpeed;
        }
        private void MoveZoomInOutEnded(InputAction.CallbackContext context) {
            this.moveZoomInOutValue = 0f;
        }
        private void RotateLeftRightPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.rotateLeftRightValue = context.ReadValue<float>() * this.rotateSpeed;
        }
        private void RotateLeftRightEnded(InputAction.CallbackContext context) {
            this.rotateLeftRightValue = 0f;
        }
        private void RotateUpDownPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.rotateUpDownValue = context.ReadValue<float>() * this.rotateSpeed;
        }
        private void RotateUpDownEnded(InputAction.CallbackContext context) {
            this.rotateUpDownValue = 0f;
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        private void HandleInputValues() {
            transform.up = this.UpDirection;
			this.tCamera.RotateAround(
				this.tTarget.position,
				this.UpDirection, 
				this.rotateLeftRightValue * Time.fixedDeltaTime
			);
            this.tCamera.transform.RotateAround(
                this.tTarget.position,
                this.tCamera.right,
                this.rotateUpDownValue * Time.fixedDeltaTime
            );
            Vector3 directionHorizontal =
                -this.tCamera.right * this.moveLeftRightValue +
                this.tCamera.forward.XZ().ToXZ().normalized * this.moveForwardBackwardValue;
            Vector3 directionInRig =
                this.tCamera.forward * this.moveZoomInOutValue;
            this.rigidbody.MovePosition(
                transform.position + directionHorizontal * Time.fixedDeltaTime
            );
            this.tCamera.position += directionInRig * Time.fixedDeltaTime;
        }
        private void ConstrainCameraToTraget() {
            Vector3 lookDirection = this.tTarget.position - this.tCamera.position;
            float distance = lookDirection.magnitude;
            // Ensure camera within distance range
            distance = Mathf.Clamp(distance, this.distanceRange.x, this.distanceRange.y);
            // Ensure camera is in correct angular position
            Vector3 inPlaneNormal = Vector3.Cross(lookDirection, this.UpDirection);
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(lookDirection, this.UpDirection);
            float verticalAngle = Vector3.Angle(horizontalDirection, lookDirection);
            verticalAngle = Mathf.Clamp(verticalAngle, this.verticalAngleRange.x, this.verticalAngleRange.y);
            lookDirection = Quaternion.AngleAxis(-verticalAngle, inPlaneNormal) * horizontalDirection.normalized;
            // Update the position and rotation of the camera
            this.tCamera.position = this.tTarget.position - lookDirection * distance;
            this.tCamera.LookAt(this.tTarget);
        }
    }
}
