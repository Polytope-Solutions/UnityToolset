#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputWorldLongTouchSimplePinch : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description = "WorldLongTouchSimplePinch";
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
                StartInteraction();
            }
            else if (this.isStarted && Touch.activeTouches.Count <= 1)
                EndInteraction();
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
            EndInteraction();
        }

        private void StartInteraction() {
            this.isStarted = true;
            this.onStarted?.Invoke();
            #if DEBUG2
            this.Log("WorldLongPinch Pinch Started");
            #endif
        }
        private void EndInteraction() {
            this.isStarted = false;
            if (this.resetOnRelease)
                this.onPinch?.Invoke(0);
            this.onEnded?.Invoke();
            #if DEBUG2
            this.Log("WorldLongPinch Pinch Ended");
            #endif
        }
        #endregion
    }
}
