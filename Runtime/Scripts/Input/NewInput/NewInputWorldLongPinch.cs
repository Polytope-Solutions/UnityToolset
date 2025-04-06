#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputWorldPinch : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description = "WorldLongPinch";
        [SerializeField] private bool normalizeInScreenSize = true;
        [SerializeField] private bool invertDirection = false;
        [SerializeField] private bool resetOnRelease = true;
        [SerializeField] private UnityEvent<float> onPinch;
        [SerializeField] private UnityEvent onStarted, onEnded;
        private float startDelta;
        private bool isStarted;

        #region MANAGEMENT
        public string Description => this.description;
        public void Init() { }
        public bool IsApplicable(InputAction.CallbackContext input) {
            return true;
        }
        #endregion
        
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongPinch Started");
            #endif
            this.isStarted = false;
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongPinch Performed");
            #endif
            Vector2 screenPosition1 = Vector2.zero, screenPosition2 = Vector2.zero;
            if (!this.isStarted && Touch.activeTouches.Count > 1) {
                // reset start delta
                screenPosition1 = Touch.activeTouches[0].screenPosition;
                screenPosition2 = Touch.activeTouches[1].screenPosition;
                this.startDelta = (screenPosition2 - screenPosition1).magnitude;
                StartPinch();
            }
            else if (this.isStarted && Touch.activeTouches.Count <= 1)
                ReleasePinch();
            else if (this.isStarted && Touch.activeTouches.Count > 1) {
                screenPosition1 = Touch.activeTouches[0].screenPosition;
                screenPosition2 = Touch.activeTouches[1].screenPosition;
            }
            if (!this.isStarted) {
                return;
            }

            float currentDelta = (screenPosition2 - screenPosition1).magnitude;
            currentDelta = currentDelta - this.startDelta;
            if (this.normalizeInScreenSize) {
                float limit = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
                currentDelta /= limit;
            }
            if (this.invertDirection)
                currentDelta = -currentDelta;
            #if DEBUG2
            this.Log($"Pinch {currentDelta}");
            #endif
            this.onPinch?.Invoke(currentDelta);
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongPinch Ended");
            #endif
            ReleasePinch();
        }

        private void StartPinch() {
            this.isStarted = true;
            this.onStarted?.Invoke();
            #if DEBUG2
            this.Log("Pinch Started");
            #endif
        }
        private void ReleasePinch() {
            this.isStarted = false;
            if (this.resetOnRelease) 
                this.onPinch?.Invoke(0);
            this.onEnded?.Invoke();
            #if DEBUG2
            this.Log("Pinch Ended");
            #endif
        }
        #endregion
    }
}
