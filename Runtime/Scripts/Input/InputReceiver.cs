using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Input {
    public abstract class InputReceiver : MonoBehaviour {
        [Header("Input Receiver Settings")]
        [SerializeField] protected bool isActiveByDefault;
        [SerializeField] protected string inputReceiverKeyName;
        [SerializeField] protected bool allowUIOnStart = false;
        protected bool isReceiverEnabled = false;
        protected bool isTemporaryDisabled = false;
        private bool wasTemporaryDisabled = false;
        protected bool IsInputEnabled => this.isReceiverEnabled && !this.isTemporaryDisabled;
        public bool IsActiveByDefault => this.isActiveByDefault;
        protected bool IsSelfManaged => InputManager.Instance == null;
        protected bool IsPointerOverUI => InputManager.Instance.IsPointerOverUI;

        private bool isStartingInteraction;
        private bool isEndingInteraction;
        private bool isInteracting;
        protected HashSet<IInputHandler> currentHandlers = new HashSet<IInputHandler>();
        protected List<IInputHandler> activeHandlers = new List<IInputHandler>();

        ///////////////////////////////////////////////////////////////////////
        #region UNITY_FUNCTIONS
        protected virtual void Update() {
            HandleInputValues();
        }
        #endregion

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

        ///////////////////////////////////////////////////////////////////////
        public void RegisterInputHandler(IInputHandler handler) {
            if (!this.currentHandlers.Contains(handler))
                this.currentHandlers.Add(handler);
        }
        public void UnregisterInputHandler(IInputHandler handler) {
            if (this.currentHandlers.Contains(handler))
                this.currentHandlers.Remove(handler);
        }
        protected void TriggerStartInteraction() { 
            this.isStartingInteraction = true;
        }
        protected void TriggerPerformInteraction() {
            this.isInteracting = true;
        }
        protected void TriggerEndInteraction() {
            this.isEndingInteraction = true;
        }
        private void HandleInputValues() {
            // Handle Start
            if (this.isStartingInteraction) {
                UpdateActiveHandlers();
                if ((!this.allowUIOnStart || this.IsPointerOverUI) 
                        && TryPassStartToActiveHandlers()) { 
                    #if DEBUG2
                    this.Log($"Starting interaction. Active Handlers: [{this.activeHandlers.Count}]. Disabling other interactors.");
                    #endif
                    OnInteractionStart();
                }
                this.isStartingInteraction = false;
            }
            // Handle Perform
            if (this.isInteracting) {
                TryPassPermormedToActiveHandlers(OnInteractionPerformed());
            }
            // Handle End
            if (this.isEndingInteraction) {
                if (this.isInteracting && this.activeHandlers.Count > 0) {
                    this.activeHandlers.ForEach(handler => handler.OnInteractionEnded());
                    this.activeHandlers.Clear();
                    #if DEBUG2
                    this.Log($"Ending interaction. Active Handlers: [{this.activeHandlers.Count}]. Unblock other interactors.");
                    #endif
                    OnInteractionEnded();
                }
                this.isEndingInteraction = false;
                this.isInteracting = false;
            }
        }
        private bool TryPassStartToActiveHandlers() {
            if (this.activeHandlers.Count > 0) {
                this.activeHandlers.ForEach(handler => handler.OnInteractionStarted());
                #if DEBUG2
                this.Log($"Starting interaction. Active Handlers: [{this.activeHandlers.Count}].");
                #endif
                return true;
            }
            return false;
        }
        private void TryPassPermormedToActiveHandlers(object data) { 
            if (data != null)
                this.activeHandlers.ForEach(handler => handler.OnInteractionPerformed(data));
        }

        protected virtual void UpdateActiveHandlers() {
            this.activeHandlers.Clear();
            RaycastHit? currentInteractionRayCast = CurrentInteractionRay();
            if (currentInteractionRayCast.HasValue) {
                foreach (IInputHandler handler in this.currentHandlers) {
                    if (handler.IsRelevantHandler(currentInteractionRayCast.Value))
                        this.activeHandlers.Add(handler);
                }
            }
            if (this.activeHandlers.Count == 0) { 
                this.isEndingInteraction = true;
            }
        }
        protected virtual RaycastHit? CurrentInteractionRay() { return null; }
        protected virtual void OnInteractionStart() { }
        protected virtual object OnInteractionPerformed() { return null; }
        protected virtual void OnInteractionEnded() { }
    }
}