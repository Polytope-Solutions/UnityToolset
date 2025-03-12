using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public interface INewInputHandler {
        public string Description { get; }
        public void Init();
        public bool IsApplicable(InputAction.CallbackContext input);
        public void HandleStarted(InputAction.CallbackContext input);
        public void HandlePerformed(InputAction.CallbackContext input);
        public void HandleEnded(InputAction.CallbackContext input);
    }
}
