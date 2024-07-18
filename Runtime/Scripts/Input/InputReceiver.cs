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
        protected bool IsInputEnabled => this.isReceiverEnabled && !this.isTemporaryDisabled;
        public bool IsActiveByDefault => this.isActiveByDefault;
        protected bool IsSelfManaged => InputManager.Instance == null;
        protected bool IsPointerOverUI => InputManager.Instance.IsPointerOverUI;
        protected virtual bool CanHaveHandlers => true;

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
        protected virtual void OnEnable() {
            EnableInputEvents();
            if (this.IsSelfManaged)
                SetActiveInputs(this.IsActiveByDefault);
        }
        protected virtual void OnDisable() {
            DisableInputEvents();
            if (this.IsSelfManaged)
                SetActiveInputs(false);
        }

        protected virtual void Awake() {
            if (string.IsNullOrEmpty(this.inputReceiverKeyName))
                this.inputReceiverKeyName = gameObject.name + "_InputReceiver";
            if (!this.IsSelfManaged)
                InputManager.Instance.RegisterInputReceiver(this.inputReceiverKeyName, this);
        }
        protected void OnDestroy() {
            DisableInputEvents();

            if (!this.IsSelfManaged)
                InputManager.Instance.UnregisterInputReceiver(this.inputReceiverKeyName);
        }
        #endregion

        protected abstract void EnableInputEvents();
        protected abstract void DisableInputEvents();
        public virtual void SetActiveInputs(bool targetState) {
            if (this.isReceiverEnabled == targetState) return;
            this.isReceiverEnabled = targetState;
            //if (this.isReceiverEnabled)
            //    EnableInputEvents();
            //else
            //    DisableInputEvents();
        }
        public virtual void SetTemporarilyActiveInputs(bool targetState) {
            this.isTemporaryDisabled = !targetState;
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
        public void TriggerEndInteraction() {
            this.isEndingInteraction = true;
        }
        private void HandleInputValues() {
            // Handle End
            if (this.isEndingInteraction) {
                if (this.isInteracting) {
                    if (this.CanHaveHandlers && this.activeHandlers.Count > 0) { 
                        this.activeHandlers.ForEach(handler => handler.OnInteractionEnded());
                        this.activeHandlers.Clear();
                        #if DEBUG2
                        this.Log($"Ending interaction. Active Handlers: [{this.activeHandlers.Count}].");
                        #endif
                    }
                    #if DEBUG2
                    this.Log($"Ending interaction.");
                    #endif
                    OnInteractionEnded();
                }
                this.isEndingInteraction = false;
                this.isInteracting = false;
                return;
            }
            // Handle Perform
            if (this.isInteracting) {
                TryPassPermormedToActiveHandlers(OnInteractionPerformed());
                return;
            }
            // Handle Start
            if (this.isStartingInteraction) {
                if (this.CanHaveHandlers) { 
                    UpdateActiveHandlers();
                    if (this.activeHandlers.Count == 0) {
                        this.isEndingInteraction = true;
                    }
                    #if DEBUG2
                    this.Log($"Trying to start interaction. Active Handlers: [{this.activeHandlers.Count}].");
                    #endif
                }
                if ((!this.allowUIOnStart || this.IsPointerOverUI) 
                        && TryPassStartToActiveHandlers()) { 
                    #if DEBUG2
                    this.Log($"Starting interaction.");
                    #endif
                    this.isInteracting = true;
                    OnInteractionStarted();
                } else {
                    //this.isEndingInteraction = true;
                }
                this.isStartingInteraction = false;
            }
        }
        private bool TryPassStartToActiveHandlers() {
            if (!this.CanHaveHandlers)
                return true;
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
        }
        protected virtual RaycastHit? CurrentInteractionRay() { return null; }
        protected virtual void OnInteractionStarted() { }
        protected virtual object OnInteractionPerformed() { return null; }
        protected virtual void OnInteractionEnded() { }
    }
}