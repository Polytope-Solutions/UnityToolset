using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public class ExtendedTouchEmulatorActivator : MonoBehaviour {
        private void Start() {
            if (Touchscreen.current != null) {
                ExtendedTouch extentdedTouch = InputSystem.GetDevice<ExtendedTouch>();
                if (extentdedTouch == null)
                    extentdedTouch = InputSystem.AddDevice<ExtendedTouch>();
                InputSystem.EnableDevice(extentdedTouch);
            }
            InputSystem.onDeviceChange += (device, change) => {
                if (device is Touchscreen) {
                    ExtendedTouch extentdedTouch = InputSystem.GetDevice<ExtendedTouch>();
                    switch (change) {
                        case InputDeviceChange.Added:
                            if (extentdedTouch == null)
                                extentdedTouch = InputSystem.AddDevice<ExtendedTouch>();
                            InputSystem.EnableDevice(extentdedTouch);
                            break;

                        case InputDeviceChange.Removed:
                            if (extentdedTouch != null)
                                InputSystem.RemoveDevice(extentdedTouch);
                            break;
                    }
                }
            };
        }
    }
}