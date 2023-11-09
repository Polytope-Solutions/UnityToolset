using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PolytopeSolutions.Toolset.Input {
    public abstract class InputReceiver : MonoBehaviour {
        [Header("Input Receiver Settings")]
        [SerializeField] protected bool isActiveByDefault;
        [SerializeField] protected string inputReceiverKeyName;
        protected bool isInputEnabled = false;
        public bool IsActiveByDefault => this.isActiveByDefault;
        private bool isSelfManaged => InputManager.instance == null;

        protected abstract void EnableInputEvents();
        protected abstract void DisableInputEvents();
        public virtual void SetActiveInputs(bool targetState) {
            if (this.isInputEnabled == targetState) return;
            this.isInputEnabled = targetState;
            if (this.isInputEnabled)
                EnableInputEvents();
            else
                DisableInputEvents();
        }

        protected virtual void Awake() {
            if (string.IsNullOrEmpty(this.inputReceiverKeyName))
                this.inputReceiverKeyName = gameObject.name+"_InputReceiver";
            if (!this.isSelfManaged)
                InputManager.instance.RegisterInputReceiver(this.inputReceiverKeyName, this);
        }
        protected virtual void OnEnable() { 
            if (this.isSelfManaged)
                SetActiveInputs(this.IsActiveByDefault);
        }
        protected virtual void OnDisable() { 
            if (this.isSelfManaged)
                SetActiveInputs(false);
        }
    }
}