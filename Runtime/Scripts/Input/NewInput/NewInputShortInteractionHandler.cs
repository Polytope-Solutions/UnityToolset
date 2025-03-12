using UnityEngine;

using UnityEngine.InputSystem;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public abstract class NewInputShortInteractionHandler : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description;
        #region MANAGEMENT
        public string Description => this.description;
        public abstract void Init();
        public abstract bool IsApplicable(InputAction.CallbackContext input);
        #endregion
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {}
        public void HandlePerformed(InputAction.CallbackContext input) {}
        public abstract void HandleEnded(InputAction.CallbackContext input);
        #endregion
    }
}