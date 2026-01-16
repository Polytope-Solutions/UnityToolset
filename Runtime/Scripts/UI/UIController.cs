using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.Events;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.UI {
    public abstract class UIController<T> : MonoBehaviour where T : UIState {
        [SerializeField] private string controllerName;
        [SerializeField] private string controllerID;
        [SerializeField] private List<T> states;
        [SerializeField] private bool autoSetFirstStateOnStart = true;

        private int currentStateIndex = -1;

        private T this[int index] {
            get {
                index = Mathf.Clamp(index, 0, this.Count-1);
                return this.states[index];
            }
        }
        public T GetStateByID(string stateID) {
            int index = GetStateIndexByID(stateID);
            if (index >= 0 && index < this.Count)
                return this.states[index];
            return null;
        }
        protected int GetStateIndexByID(string stateID) {
            return this.states.FindIndex(item => item.StateID == stateID);
        }
        public int Count => this.states.Count;
        public T CurrentState => this[this.currentStateIndex];
        public string CurrentStateID => this[this.currentStateIndex].StateID;

        private UIManager.UIControllerInteractions interactions => new UIManager.UIControllerInteractions() {
            Activation = this.SetState,
            RequestState = this.RequestState
        };

        protected virtual void Awake() {
            if (string.IsNullOrEmpty(this.controllerID))
                this.controllerID = this.name;
        }

        protected virtual void Start() {
            if (UIManager.Instance.RegisterController(this.controllerID, this.interactions)
                && this.autoSetFirstStateOnStart)
                SetInitialState();
            if (EventManager.Instance != null) {
                EventManager.Instance.RegisterEvenetCallback(UIManager.NEXT_STATE_EVENTKEY, Next);
            }
        }
        protected virtual void OnDestroy() {
            if (EventManager.Instance != null) {
                EventManager.Instance.UnregisterEvenetCallback(UIManager.NEXT_STATE_EVENTKEY, Next);
            }
        }

        public void Next() {
            Transition(1);
        }
        public void Transition(int modifier) {
            int previousIndex = this.currentStateIndex;
            this.currentStateIndex = Mathf.Clamp(this.currentStateIndex + modifier, 0, this.Count);
            if (previousIndex != this.currentStateIndex) {
                this[previousIndex].Deactivate(this[this.currentStateIndex]);
                this[this.currentStateIndex].Activate(this[previousIndex]);
                UIManager.Instance.LogSwitchUIState(this.controllerID, this.CurrentStateID);
            }
        }
        private void SetInitialState()
            => SetInitialState(0);
        protected void SetInitialState(int index) {
                for (int i = 0; i < this.states.Count; i++) {
                if (i != index)
                    this.states[i].Deactivate(this[index], immediate: true);
                else
                    this[index].Activate();
            }
            this.currentStateIndex = index;
            UIManager.Instance.LogSwitchUIState(this.controllerID, this.CurrentStateID);
        }
        private void SetState(string stateID)
            => SetState(stateID, false);
        public void SetState(string stateID, bool logSwitch=true) {
            int previousIndex = this.currentStateIndex;
            int index = GetStateIndexByID(stateID);
            this.currentStateIndex = Mathf.Clamp(index, 0, this.Count);
            if (previousIndex != this.currentStateIndex) {
                this[previousIndex].Deactivate(this[this.currentStateIndex]);
                this[this.currentStateIndex].Activate(this[previousIndex]);
                if (logSwitch)
                    UIManager.Instance.LogSwitchUIState(this.controllerID, this.CurrentStateID);
            }
        }

        private void RequestState(string stateID, bool targetState) {
            UIState state = GetStateByID(stateID);
            if (state == null) {
                this.LogError($"State {stateID} not found in {this.controllerID}");
                return;
            }
            state.SetState(targetState);
        }
    }
}
