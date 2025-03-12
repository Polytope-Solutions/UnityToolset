using UnityEngine;
using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public interface INewInputReceiver {
        public void HandleStarted(InputAction.CallbackContext input);
        public void HandlePerformed(InputAction.CallbackContext input);
        public void HandleEnded(InputAction.CallbackContext input);
    }
}
