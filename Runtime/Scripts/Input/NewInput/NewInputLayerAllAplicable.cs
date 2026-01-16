#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputLayerAllAplicable : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description;
        [SerializeField] private List<InterfaceReference<INewInputHandler>> handlers = new();
        private List<INewInputHandler> currentHandlers = new();

        #region MANAGEMENT
        public string Description => this.description;
        public void Init() {
            for (int i = 0; i < this.handlers.Count; i++)
                this.handlers[i].Value.Init();
        }
        public bool IsApplicable(InputAction.CallbackContext input) {
            this.currentHandlers.Clear();
            for (int i = 0; i < this.handlers.Count; i++) {
                if (this.handlers[i].Value.IsApplicable(input)) {
                    this.currentHandlers.Add(this.handlers[i].Value);
                    #if DEBUG2
                    this.Log($"Added {this.currentHandler.Description}");
                    #endif
                }
            }
            if (this.currentHandlers.Count > 0)
                return true;
            #if DEBUG2
            this.Log($"None Selected");
            #endif
            return false;
        }
        #endregion
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            foreach (INewInputHandler handler in this.currentHandlers)
                handler?.HandleStarted(input);
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            foreach (INewInputHandler handler in this.currentHandlers)
                handler?.HandlePerformed(input);
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            foreach (INewInputHandler handler in this.currentHandlers)
                handler?.HandleEnded(input);
            this.currentHandlers.Clear();
        }
        #endregion
    }
}