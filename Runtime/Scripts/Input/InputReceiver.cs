using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PolytopeSolutions.Toolset.Input {
    public abstract class InputReceiver : MonoBehaviour {
        [Header("Input Receiver Settings")]
        [SerializeField] protected bool isActiveByDefault;
        [SerializeField] protected string inputReceiverKeyName;
        protected bool isReceiverEnabled = false;
        protected bool isTemporaryDisabled = false;
        private bool wasTemporaryDisabled = false;
        protected bool IsInputEnabled => this.isReceiverEnabled && !this.isTemporaryDisabled;
        public bool IsActiveByDefault => this.isActiveByDefault;
        protected bool IsSelfManaged => InputManager.Instance == null;

        protected abstract void EnableInputEvents();
        protected abstract void DisableInputEvents();
        public virtual void SetActiveInputs(bool targetState) {
            if (this.isReceiverEnabled == targetState) return;
            this.isReceiverEnabled = targetState;
            if (this.isReceiverEnabled)
                EnableInputEvents();
            else
                DisableInputEvents();
        }
        public virtual void SetTemporarilyActiveInputs(bool targetState) { 
            this.wasTemporaryDisabled = this.isTemporaryDisabled;
            this.isTemporaryDisabled = !targetState;
        }
        public virtual void RestoreFromTemporaryState() {
            this.isTemporaryDisabled = this.wasTemporaryDisabled;
        }

        protected virtual void Awake() {
            if (string.IsNullOrEmpty(this.inputReceiverKeyName))
                this.inputReceiverKeyName = gameObject.name+"_InputReceiver";
            if (!this.IsSelfManaged)
                InputManager.Instance.RegisterInputReceiver(this.inputReceiverKeyName, this);
        }
        protected virtual void OnEnable() { 
            if (this.IsSelfManaged)
                SetActiveInputs(this.IsActiveByDefault);
        }
        protected virtual void OnDisable() { 
            if (this.IsSelfManaged)
                SetActiveInputs(false);
        }
    }
}