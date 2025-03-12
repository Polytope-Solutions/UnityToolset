using UnityEngine;
using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public abstract class NewInputUIInteractionReceiver : MonoBehaviour, INewInputReceiver {
        #region HANDLERS
        public abstract void HandleStarted(InputAction.CallbackContext input);
        public abstract void HandlePerformed(InputAction.CallbackContext input);
        public abstract void HandleEnded(InputAction.CallbackContext input);
        #endregion
    }
}
