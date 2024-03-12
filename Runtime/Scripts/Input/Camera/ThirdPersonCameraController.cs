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

        [SerializeField] private float rotateSpeed = 50f;
        [SerializeField] private Vector2 moveSpeedRange = new Vector2(100f, 100f);
        [SerializeField] private float zoomSpeed = 25f;
        private float moveSpeed => Mathf.Lerp(this.moveSpeedRange.x, this.moveSpeedRange.y, this.Proximity);

        protected virtual Vector3 UpDirection => Vector3.up;
        private Vector3 cameraLocalUp = Vector3.up;

        public float Proximity {
            get { 
                float distance = Vector3.Distance(this.tCamera.position, this.tTarget.position);
                return Mathf.InverseLerp(this.distanceRange.x, this.distanceRange.y, distance);
            }
        }

        private float rotateLeftRightValue;
        private float rotateUpDownValue;
        private float moveLeftRightValue;
        private float moveForwardBackwardValue;
        private float moveZoomInOutValue;

        private Transform tTargetProxy;
        private Vector3 targetMovementVelocity, targetRotateVeloccity;

        ///////////////////////////////////////////////////////////////////////
        #region UNITY_FUNCTIONS
        protected virtual void FixedUpdate() {
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
            TriggerPerformInteraction();
        }
        private void MoveLeftRightEnded(InputAction.CallbackContext context) {
            this.moveLeftRightValue = 0f;
            TriggerPerformInteraction();
        }
        private void MoveForwardBackwardPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.moveForwardBackwardValue = context.ReadValue<float>() * this.moveSpeed;
            TriggerPerformInteraction();
        }
        private void MoveForwardBackwardEnded(InputAction.CallbackContext context) {
            this.moveForwardBackwardValue = 0f;
            TriggerPerformInteraction();
        }
        private void MoveZoomInOutPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.moveZoomInOutValue = context.ReadValue<float>() * this.zoomSpeed;
            TriggerPerformInteraction();
        }
        private void MoveZoomInOutEnded(InputAction.CallbackContext context) {
            this.moveZoomInOutValue = 0f;
            TriggerPerformInteraction();
        }
        private void RotateLeftRightPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.rotateLeftRightValue = context.ReadValue<float>() * this.rotateSpeed;
            TriggerPerformInteraction();
        }
        private void RotateLeftRightEnded(InputAction.CallbackContext context) {
            this.rotateLeftRightValue = 0f;
        }
        private void RotateUpDownPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
            this.rotateUpDownValue = context.ReadValue<float>() * this.rotateSpeed;
            TriggerPerformInteraction();
        }
        private void RotateUpDownEnded(InputAction.CallbackContext context) {
            this.rotateUpDownValue = 0f;
            TriggerPerformInteraction();
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        protected override void ObjectSetup() {
            base.ObjectSetup();
            if (this.useProxies) {
                if (!this.tTargetProxy) { 
                    this.tTargetProxy = this.tObjectProxy.gameObject.TryFindOrAddByName("TargetProxy").transform;
                    this.tTargetProxy.position = this.tTarget.position;
                    this.tTargetProxy.rotation = this.tTarget.rotation;
                    this.tTargetProxy.localScale = this.tTarget.localScale;
                }
            }
            else {
                this.tTargetProxy = this.tTarget;
            }
        }
        protected override object OnInteractionPerformed() {
            this.tObjectProxy.up = this.UpDirection;
            Vector3 localForward = Vector3.Cross(this.UpDirection, this.tCameraProxy.right);
            Vector3 directionHorizontal =
                -this.tCameraProxy.right * this.moveLeftRightValue +
                -localForward.normalized * this.moveForwardBackwardValue;
            Vector3 targetDirection = this.tCameraProxy.position - this.tTargetProxy.position;
            Vector3 directionInRig =
                targetDirection.normalized * this.moveZoomInOutValue;
            this.objectRigidbody.MovePosition(
                this.tObjectProxy.position + directionHorizontal * Time.fixedDeltaTime
            );
            this.tCameraProxy.RotateAround(
                this.tTargetProxy.position,
                this.tCameraProxy.right,
                this.rotateUpDownValue * Time.fixedDeltaTime
            );
            this.tCameraProxy.RotateAround(
                this.tTargetProxy.position,
                this.UpDirection,
                this.rotateLeftRightValue * Time.fixedDeltaTime
            );
            this.tCameraProxy.position += directionInRig * Time.fixedDeltaTime;
            return null;
        }
        private void ConstrainCameraToTraget() {
            Vector3 lookDirection = this.tTargetProxy.position - this.tCameraProxy.position;
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
            this.tCameraProxy.position = this.tTargetProxy.position - lookDirection * distance;
            cameraLocalUp = Vector3.Cross(lookDirection, this.tCameraProxy.right);
            this.tCameraProxy.LookAt(this.tTargetProxy, cameraLocalUp);
        }
        protected override void ApplyProxies() {
            base.ApplyProxies();
            if (this.useProxies) {
                this.tTarget.position = Vector3.SmoothDamp(this.tTarget.position, this.tTargetProxy.position, ref this.targetMovementVelocity, this.smoothTime);
                this.tTarget.rotation = Quaternion.Euler(
                    Mathf.SmoothDampAngle(this.tTarget.rotation.eulerAngles.x, this.tTargetProxy.rotation.eulerAngles.x, ref this.targetRotateVeloccity.x, this.smoothTime),
                    Mathf.SmoothDampAngle(this.tTarget.rotation.eulerAngles.y, this.tTargetProxy.rotation.eulerAngles.y, ref this.targetRotateVeloccity.y, this.smoothTime),
                    Mathf.SmoothDampAngle(this.tTarget.rotation.eulerAngles.z, this.tTargetProxy.rotation.eulerAngles.z, ref this.targetRotateVeloccity.z, this.smoothTime)
                );
                //this.tTarget.localRotation = Quaternion.RotateTowards(this.tTarget.localRotation, this.tTargetProxy.localRotation, this.maxDegreesDelta*Time.deltaTime);
            }
        }
    }        
}
