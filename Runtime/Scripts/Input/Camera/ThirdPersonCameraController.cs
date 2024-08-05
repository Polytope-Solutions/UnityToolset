using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    public class ThirdPersonCameraController : CameraInputProvider {

        [Header("Events")]
        [SerializeField] private InputActionReference cameraRotateLeftRight;
        [SerializeField] private InputActionReference cameraRotateUpDown;
        [SerializeField] private InputActionReference cameraMoveLeftRight;
        [SerializeField] private InputActionReference cameraMoveForwardBackward;
        [SerializeField] private InputActionReference cameraMoveZoomInOut;

        [Header("Movement")]
        [SerializeField] private Vector2 verticalAngleRangeMin = new Vector2(15, 45);
        [SerializeField] private Vector2 verticalAngleRangeMax = new Vector2(85, 85);
        private Vector2 VerticalAngleRange => new Vector2(
            Mathf.Lerp(this.verticalAngleRangeMin.x, this.verticalAngleRangeMin.y, this.TargetProximity),
            Mathf.Lerp(this.verticalAngleRangeMax.x, this.verticalAngleRangeMax.y, this.TargetProximity));
        //[SerializeField] private Vector2 horizontalAngleRange;

        [SerializeField] private float rotateSpeed = 50f;
        [SerializeField] private Vector2 moveSpeedRange = new Vector2(100f, 100f);
        [SerializeField] private float zoomSpeed = 25f;
        private float moveSpeed => Mathf.Lerp(this.moveSpeedRange.x, this.moveSpeedRange.y, this.TargetProximity);

        protected virtual Vector3 UpDirection => Vector3.up;

        private float rotateLeftRightValue;
        private float rotateUpDownValue;
        private float moveLeftRightValue;
        private float moveForwardBackwardValue;
        private float moveZoomInOutValue;

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
        Vector3 localForward, directionHorizontal, targetDirection, directionInRig, objectPosition, cameraMoveDelta;
        float angleUpDown, angleLeftRight;
        protected override object OnInteractionPerformed() {
            localForward = Vector3.Cross(this.UpDirection, this.CameraProxyRight);
            directionHorizontal =
                -this.CameraProxyRight * this.moveLeftRightValue +
                -localForward.normalized * this.moveForwardBackwardValue;
            targetDirection = this.CameraProxyPosition - this.TargetProxyPosition;
            directionInRig =
                targetDirection.normalized * this.moveZoomInOutValue;
            objectPosition =
                this.ObjectProxyPosition + directionHorizontal * Time.fixedDeltaTime;
            angleUpDown =
                this.rotateUpDownValue * Time.fixedDeltaTime;
            angleLeftRight =
                this.rotateLeftRightValue * Time.fixedDeltaTime;
            cameraMoveDelta = directionInRig * Time.fixedDeltaTime;
            ModifyRig(this.UpDirection, angleUpDown, angleLeftRight, cameraMoveDelta, objectPosition);
            ConstrainCameraToTarget(this.VerticalAngleRange);

            return null;
        }
    }
}
