using System.Collections;
using UnityEngine;

using UnityEngine.InputSystem;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputLayerContinuousButton : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description;
        [SerializeField] private InterfaceReference<INewInputHandler> handler;
        private Coroutine continuousCallbacks;
        private InputAction.CallbackContext context;

        #region MANAGEMENT
        public string Description => this.description;
        public void Init() {
            this.handler.Value.Init();
        }
        public bool IsApplicable(InputAction.CallbackContext input) {
            return this.handler.Value.IsApplicable(input);
        }
        #endregion
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            this.handler.Value.HandleStarted(input);
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            this.context = input;
            this.continuousCallbacks = StartCoroutine(ContinuousCallback());
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            StopCoroutine(this.continuousCallbacks);
            this.continuousCallbacks = null;
            this.handler.Value.HandleEnded(input);
        }
        private IEnumerator ContinuousCallback() {
            while (true) {
                this.handler.Value.HandlePerformed(this.context);
                yield return null;
            }
        }
        #endregion
    }
}
