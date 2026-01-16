#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputLayerFirstApplicable : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description;
        [SerializeField] private List<InterfaceReference<INewInputHandler>> handlers = new();
        private INewInputHandler currentHandler;

        #region MANAGEMENT
        public string Description => this.description;
        public void Init() {
            for (int i = 0; i < this.handlers.Count; i++)
                this.handlers[i].Value.Init();
        }
        public bool IsApplicable(InputAction.CallbackContext input) {
            this.currentHandler = null;
            for (int i = 0; i < this.handlers.Count; i++) {
                if (this.handlers[i].Value.IsApplicable(input)) {
                    this.currentHandler = this.handlers[i].Value;
                    #if DEBUG2
                    this.Log($"Selected {this.currentHandler.Description}");
                    #endif
                    return true;
                }
            }
            #if DEBUG2
            this.Log($"None Selected");
            #endif
            return false;
        }
        #endregion
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            this.currentHandler?.HandleStarted(input);
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            this.currentHandler?.HandlePerformed(input);
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            this.currentHandler?.HandleEnded(input);
            this.currentHandler = null;
        }
        #endregion
    }
}
