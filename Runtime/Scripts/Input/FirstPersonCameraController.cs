using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
	[RequireComponent(typeof(Rigidbody))]
	public class FirstPersonCameraController : MonoBehaviour {
		[SerializeField] private InputActionReference cameraRotateLeftRight;
		[SerializeField] private InputActionReference cameraRotateUpDown;
        [SerializeField] private InputActionReference cameraMoveLeftRight;
        [SerializeField] private InputActionReference cameraMoveUpDown;
        [SerializeField] private InputActionReference cameraMoveForwardBackward;

		[SerializeField] private float rotateSpeed = 1f;
		[SerializeField] private float moveSpeed = 1f;

		[SerializeField] private Camera viewerCamera;
		private new Rigidbody rigidbody;

		private float rotateLeftRightValue;
        private float rotateUpDownValue;
        private float moveLeftRightValue;
        private float moveUpDownValue;
        private float moveForwardBackwardValue;

        private void OnEnable() {
			this.rigidbody = gameObject.GetComponent<Rigidbody>();

            this.cameraRotateLeftRight.action.Enable();
			this.cameraRotateUpDown.action.Enable();
			this.cameraMoveLeftRight.action.Enable();
            this.cameraMoveUpDown.action.Enable();
			this.cameraMoveForwardBackward.action.Enable();

            this.cameraRotateLeftRight.action.started += RotateLeftRightStarted;
            this.cameraRotateUpDown.action.started += RotateUpDownStarted;
			this.cameraMoveLeftRight.action.started += MoveLeftRightStarted;
            this.cameraMoveUpDown.action.started += MoveUpDownStarted;
            this.cameraMoveForwardBackward.action.started += MoveForwardBackwardStarted;
            this.cameraRotateLeftRight.action.canceled += RotateLeftRightEnded;
            this.cameraRotateUpDown.action.canceled += RotateUpDownEnded;
            this.cameraMoveLeftRight.action.canceled += MoveLeftRightEnded;
            this.cameraMoveUpDown.action.canceled += MoveUpDownEnded;
            this.cameraMoveForwardBackward.action.canceled += MoveForwardBackwardEnded;
        }
		private void OnDisable() {
            this.cameraRotateLeftRight.action.started -= RotateLeftRightStarted;
            this.cameraRotateUpDown.action.started -= RotateUpDownStarted;
            this.cameraMoveLeftRight.action.started += MoveLeftRightStarted;
            this.cameraMoveUpDown.action.started -= MoveUpDownStarted;
            this.cameraMoveForwardBackward.action.started -= MoveForwardBackwardStarted;
            this.cameraRotateLeftRight.action.canceled -= RotateLeftRightEnded;
            this.cameraRotateUpDown.action.canceled -= RotateUpDownEnded;
            this.cameraMoveLeftRight.action.canceled += MoveLeftRightEnded;
            this.cameraMoveUpDown.action.canceled -= MoveUpDownEnded;
            this.cameraMoveForwardBackward.action.canceled -= MoveForwardBackwardEnded;

            this.cameraRotateLeftRight.action.Disable();
			this.cameraRotateUpDown.action.Disable();
			this.cameraMoveUpDown.action.Disable();
			this.cameraMoveForwardBackward.action.Disable();
		}
		private void RotateLeftRightStarted(InputAction.CallbackContext context) { 
			this.rotateLeftRightValue = context.ReadValue<float>() * this.rotateSpeed;
		}
		private void RotateLeftRightEnded(InputAction.CallbackContext context) { 
			this.rotateLeftRightValue = 0f;
		}
		private void RotateUpDownStarted(InputAction.CallbackContext context) { 
			this.rotateUpDownValue = context.ReadValue<float>() * this.rotateSpeed;
        }
		private void RotateUpDownEnded(InputAction.CallbackContext context) { 
			this.rotateUpDownValue = 0f;
        }
		private void MoveLeftRightStarted(InputAction.CallbackContext context) { 
			this.moveLeftRightValue = context.ReadValue<float>() * this.moveSpeed;
		}
		private void MoveLeftRightEnded(InputAction.CallbackContext context) { 
			this.moveLeftRightValue = 0f;
		}
		private void MoveUpDownStarted(InputAction.CallbackContext context) { 
			this.moveUpDownValue = context.ReadValue<float>() * this.moveSpeed;
		}
		private void MoveUpDownEnded(InputAction.CallbackContext context) { 
			this.moveUpDownValue = 0f;
		}
		private void MoveForwardBackwardStarted(InputAction.CallbackContext context) {
            this.moveForwardBackwardValue = context.ReadValue<float>() * this.moveSpeed;
        }
		private void MoveForwardBackwardEnded(InputAction.CallbackContext context) {
            this.moveForwardBackwardValue = 0f;
		}

		void FixedUpdate() {
			this.viewerCamera.transform.RotateAround(
				this.viewerCamera.transform.position,
				Vector3.up, 
				this.rotateLeftRightValue * Time.fixedDeltaTime
			);
            this.viewerCamera.transform.RotateAround(
				this.viewerCamera.transform.position,
                this.viewerCamera.transform.right, 
				this.rotateUpDownValue * Time.fixedDeltaTime
			);
			Vector3 direction =
                Vector3.left * this.moveLeftRightValue +
                Vector3.up * this.moveUpDownValue +
                this.viewerCamera.transform.forward * this.moveForwardBackwardValue;
			this.rigidbody.MovePosition(
				transform.position + direction * Time.fixedDeltaTime
			);
        }
	}
}