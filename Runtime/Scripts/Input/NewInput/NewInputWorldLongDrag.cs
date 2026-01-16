#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputWorldLongDrag : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description = "WorldLongDrag";
        [SerializeField] private bool normalizeInScreenSize = true;
        [SerializeField] private bool invertHorizontal, invertVertical;
        [SerializeField] private bool resetOnRelease = true;
        [SerializeField] private UnityEvent<float> onDragHorizontal, onDragVertical;
        [SerializeField] private UnityEvent onStarted, onEnded;
        private Vector2 startPosition;
        private bool isStarted = false;

        #region MANAGEMENT
        public string Description => this.description;
        public void Init() {}
        public bool IsApplicable(InputAction.CallbackContext input) {
            return true;
        }
        #endregion
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongDrag Started");
            #endif
            this.isStarted = false;
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongDrag Performed");
            #endif
            Vector2 screenPosition = Pointer.current.position.value;
            if (!this.isStarted) {
                this.startPosition = screenPosition;
                this.onStarted?.Invoke();
                this.isStarted = true;
            }
            float horizontal = screenPosition.x - this.startPosition.x;
            float vertical = screenPosition.y - this.startPosition.y;
            if (this.normalizeInScreenSize) {
                float limit = Mathf.Min(Screen.width - Mathf.Abs(this.startPosition.x), Mathf.Abs(this.startPosition.x));
                horizontal = Mathf.Sign(horizontal) * Mathf.InverseLerp(
                    0, limit,
                    Mathf.Abs(horizontal)
                );
                limit = Mathf.Min(Screen.height - Mathf.Abs(this.startPosition.y), Mathf.Abs(this.startPosition.y));
                vertical = Mathf.Sign(vertical) * Mathf.InverseLerp(
                    0, limit,
                    Mathf.Abs(vertical)
                );
            }
            if (this.invertHorizontal) horizontal = -horizontal;
            if (this.invertVertical) vertical = -vertical;
            this.onDragHorizontal?.Invoke(horizontal);
            this.onDragVertical?.Invoke(vertical);
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            #if DEBUG2
            this.Log($"WorldLongDrag Ended");
            #endif
            if (this.resetOnRelease) {
                this.onDragHorizontal?.Invoke(0);
                this.onDragVertical?.Invoke(0);
            }

            this.onEnded?.Invoke();
        }
        #endregion
    }
}
