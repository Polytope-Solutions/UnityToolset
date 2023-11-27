using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public class ExtendedTouchEmulatorActivator : MonoBehaviour {
        private ExtendedTouch extentdedTouch;
        private bool IsInteractorEnabled => this.extentdedTouch.enabled;
        private bool isStartingInteraction, isEndingInteraction;

        [SerializeField] private InputActionReference primaryContactAction;

        private void Start() {
            if (Touchscreen.current != null) {
                this.extentdedTouch = InputSystem.GetDevice<ExtendedTouch>();
                if (this.extentdedTouch == null)
                    this.extentdedTouch = InputSystem.AddDevice<ExtendedTouch>();
                InputSystem.EnableDevice(this.extentdedTouch);
                this.primaryContactAction.action.started += InteractionStarted;
                this.primaryContactAction.action.canceled += InteractionEnded;
                this.primaryContactAction.action.Enable();
            }
            InputSystem.onDeviceChange += (device, change) => {
                if (device is Touchscreen) {
                    this.extentdedTouch = InputSystem.GetDevice<ExtendedTouch>();
                    switch (change) {
                        case InputDeviceChange.Added:
                            if (this.extentdedTouch == null)
                                this.extentdedTouch = InputSystem.AddDevice<ExtendedTouch>();
                            InputSystem.EnableDevice(this.extentdedTouch);
                            this.primaryContactAction.action.started += InteractionStarted;
                            this.primaryContactAction.action.canceled += InteractionEnded;
                            this.primaryContactAction.action.Enable();
                            break;

                        case InputDeviceChange.Removed:
                            if (this.extentdedTouch != null) {
                                InputSystem.RemoveDevice(this.extentdedTouch);
                                this.primaryContactAction.action.started -= InteractionStarted;
                                this.primaryContactAction.action.canceled -= InteractionEnded;
                                this.primaryContactAction.action.Disable();
                            }
                            break;
                    }
                }
            };
        }

        private void Update() {
            if (this.extentdedTouch == null) return;

            if (this.isStartingInteraction){
                bool isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
                if (this.IsInteractorEnabled && isPointerOverUI) { 
                    InputSystem.DisableDevice(this.extentdedTouch);
                }
                this.isStartingInteraction = false;
            }
            if (this.isEndingInteraction) { 
                if (!this.IsInteractorEnabled) {
                    InputSystem.EnableDevice(this.extentdedTouch);
                }
                this.isEndingInteraction = false;
            }
        }
        protected void InteractionStarted(InputAction.CallbackContext context) {
            this.isStartingInteraction = true;
		}
        protected void InteractionEnded(InputAction.CallbackContext context) {
            this.isEndingInteraction = true;
        }
    }
}