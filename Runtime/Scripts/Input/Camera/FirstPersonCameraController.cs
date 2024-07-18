using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
	public class FirstPersonCameraController : CameraInputProvider {
		[Header("Events")]
		[SerializeField] private InputActionReference cameraRotateLeftRight;
		[SerializeField] private InputActionReference cameraRotateUpDown;
		[SerializeField] private InputActionReference cameraMoveLeftRight;
		[SerializeField] private InputActionReference cameraMoveForwardBackward;
		[SerializeField] private InputActionReference cameraMoveUpDown;

		[Header("Movement")]
		[SerializeField] private float rotateSpeed = 25f;
		[SerializeField] private float moveSpeed = 2f;

        protected virtual Vector3 UpDirection => Vector3.up;

        private float rotateLeftRightValue;
		private float rotateUpDownValue;
		private float moveLeftRightValue;
		private float moveForwardBackwardValue;
		private float moveUpDownValue;

        ///////////////////////////////////////////////////////////////////////
        #region INPUT_HANDLING
        protected override void EnableInputEvents() {
            this.cameraRotateLeftRight.action.performed += RotateLeftRightPerformed;
            this.cameraRotateUpDown.action.performed += RotateUpDownPerformed;
            this.cameraMoveLeftRight.action.performed += MoveLeftRightPerformed;
            this.cameraMoveUpDown.action.performed += MoveUpDownPerformed;
            this.cameraMoveForwardBackward.action.performed += MoveForwardBackwardPerformed;

            this.cameraRotateLeftRight.action.canceled += RotateLeftRightEnded;
            this.cameraRotateUpDown.action.canceled += RotateUpDownEnded;
            this.cameraMoveLeftRight.action.canceled += MoveLeftRightEnded;
            this.cameraMoveUpDown.action.canceled += MoveUpDownEnded;
            this.cameraMoveForwardBackward.action.canceled += MoveForwardBackwardEnded;

            this.cameraRotateLeftRight.action.Enable();
            this.cameraRotateUpDown.action.Enable();
            this.cameraMoveLeftRight.action.Enable();
            this.cameraMoveUpDown.action.Enable();
            this.cameraMoveForwardBackward.action.Enable();
        }
        protected override void DisableInputEvents() {
            this.cameraRotateLeftRight.action.Disable();
            this.cameraRotateUpDown.action.Disable();
            this.cameraMoveLeftRight.action.Disable();
            this.cameraMoveUpDown.action.Disable();
            this.cameraMoveForwardBackward.action.Disable();

            this.cameraRotateLeftRight.action.performed -= RotateLeftRightPerformed;
            this.cameraRotateUpDown.action.performed -= RotateUpDownPerformed;
            this.cameraMoveLeftRight.action.performed -= MoveLeftRightPerformed;
            this.cameraMoveUpDown.action.performed -= MoveUpDownPerformed;
            this.cameraMoveForwardBackward.action.performed -= MoveForwardBackwardPerformed;

            this.cameraRotateLeftRight.action.canceled -= RotateLeftRightEnded;
            this.cameraRotateUpDown.action.canceled -= RotateUpDownEnded;
            this.cameraMoveLeftRight.action.canceled -= MoveLeftRightEnded;
            this.cameraMoveUpDown.action.canceled -= MoveUpDownEnded;
            this.cameraMoveForwardBackward.action.canceled -= MoveForwardBackwardEnded;

			this.rotateLeftRightValue = 0f;
            this.rotateUpDownValue = 0f;
            this.moveLeftRightValue = 0f;
            this.moveUpDownValue = 0f;
            this.moveForwardBackwardValue = 0f;
		}
        private void RotateLeftRightPerformed(InputAction.CallbackContext context) {
            if (!this.IsInputEnabled) return;
			this.rotateLeftRightValue = context.ReadValue<float>() * this.rotateSpeed;
			TriggerPerformInteraction();
		}
		private void RotateLeftRightEnded(InputAction.CallbackContext context) { 
			this.rotateLeftRightValue = 0f;
            TriggerPerformInteraction();
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
		private void MoveLeftRightPerformed(InputAction.CallbackContext context) { 
            if (!this.IsInputEnabled) return;
			this.moveLeftRightValue = context.ReadValue<float>() * this.moveSpeed;
            TriggerPerformInteraction();
        }
		private void MoveLeftRightEnded(InputAction.CallbackContext context) { 
			this.moveLeftRightValue = 0f;
            TriggerPerformInteraction();
        }
		private void MoveUpDownPerformed(InputAction.CallbackContext context) { 
            if (!this.IsInputEnabled) return;
			this.moveUpDownValue = context.ReadValue<float>() * this.moveSpeed;
            TriggerPerformInteraction();
        }
		private void MoveUpDownEnded(InputAction.CallbackContext context) { 
			this.moveUpDownValue = 0f;
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
        #endregion
        ///////////////////////////////////////////////////////////////////////
        Vector3 direction, objectPosition;
        float upDownAngle, leftRightAngle;
        protected override object OnInteractionPerformed() {
            upDownAngle =
                this.rotateUpDownValue * Time.fixedDeltaTime;
            leftRightAngle =
                this.rotateLeftRightValue * Time.fixedDeltaTime;

            direction =
				- this.CameraProxyRight * this.moveLeftRightValue +
                this.UpDirection * this.moveUpDownValue +
				this.CameraProxyForward * this.moveForwardBackwardValue;
            objectPosition =
                this.ObjectProxyPosition + direction * Time.fixedDeltaTime;
            ModifyRig(this.UpDirection, upDownAngle, leftRightAngle, Vector3.zero, objectPosition);
			return null;
		}
	}
}