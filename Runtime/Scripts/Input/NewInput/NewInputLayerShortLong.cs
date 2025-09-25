using UnityEngine;

using UnityEngine.InputSystem;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Input {
    public class NewInputLayerShortLong : MonoBehaviour, INewInputHandler {
        [SerializeField] private string description;
        [SerializeField] private InterfaceReference<INewInputHandler, NewInputShortInteractionHandler> shortHandler;
        [SerializeField] private InterfaceReference<INewInputHandler> longHandler;
        [SerializeField] private bool ignoreLongTillMinTime = true;
        private float startTime;
        private bool hasLongStarted = false;
        private bool hasShortInterface = false, hasLongInterface = false;

        #region MANAGEMENT
        public string Description => this.description;
        public void Init() {
            if (this.shortHandler != null && this.shortHandler.DirectValue != null) {
                this.hasShortInterface = true;
                this.shortHandler.Value.Init();
            }
            if (this.longHandler != null && this.longHandler.DirectValue != null) {
                this.hasLongInterface = true;
                this.longHandler.Value.Init();
            }
        }
        public bool IsApplicable(InputAction.CallbackContext input) {
            bool isApplicable = this.hasShortInterface && this.shortHandler.Value.IsApplicable(input);
            isApplicable |= this.hasLongInterface && this.longHandler.Value.IsApplicable(input);
            return isApplicable;
        }
        #endregion
        #region HANDLERS
        public void HandleStarted(InputAction.CallbackContext input) {
            this.startTime = Time.realtimeSinceStartup;
            this.hasLongStarted = false;
        }
        public void HandlePerformed(InputAction.CallbackContext input) {
            if (this.hasLongInterface) {
                if (!this.ignoreLongTillMinTime || Time.realtimeSinceStartup - this.startTime >= NewInputManager.Instance.MinDuration) {
                    if (!this.hasLongStarted) {
                        this.hasLongStarted = true;
                        this.longHandler.Value.HandleStarted(input);
                    }
                    else
                        this.longHandler.Value.HandlePerformed(input);
                }
            }
        }
        public void HandleEnded(InputAction.CallbackContext input) {
            if (Time.realtimeSinceStartup - this.startTime < NewInputManager.Instance.MinDuration) {
                if (this.hasShortInterface)
                    this.shortHandler.Value.HandleEnded(input);
            }
            else {
                if (this.hasLongInterface)
                    this.longHandler.Value.HandleEnded(input);
            }
        }
        #endregion
    }
}