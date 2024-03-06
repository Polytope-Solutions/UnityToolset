using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.Events;

namespace PolytopeSolutions.Toolset.UI {
    public abstract class UIController<T> : MonoBehaviour where T : UIState{
        [SerializeField] private string controllerID;
        [SerializeField] private T initialState;
        [SerializeField] private List<T> sequentialStates;

        private int currentStateIndex = 0;

        private T this[int index] {
            get { 
                index = Mathf.Clamp(index, 0, this.Count);
                if (index == 0)
                    return this.initialState;
                else
                    return this.sequentialStates[index - 1];
            }
        }
        public int Count => this.sequentialStates.Count + 1;
        public T CurrentState => this[this.currentStateIndex];

        protected void Awake() {
            if (string.IsNullOrEmpty(this.controllerID))
                this.controllerID = this.name;
            if (UIManager.Instance.RegisterController(this.controllerID, SetStateFromManager))
                SetInitialState();
        }

        protected virtual void Start() {
            if (EventManager.Instance != null) { 
                EventManager.Instance.RegisterEvenetCallback("NextState", Next);
            }
        }
        protected virtual void OnDestroy() {
            if (EventManager.Instance != null) {
                EventManager.Instance.UnregisterEvenetCallback("NextState", Next);
            }
        }

        public void Next() {
            Transition(1);
        }
        public void Transition(int modifier) {
            int previousIndex = this.currentStateIndex;
            this.currentStateIndex = Mathf.Clamp(this.currentStateIndex + modifier, 0, this.Count);
            if (previousIndex != this.currentStateIndex) {
                this[previousIndex].Deactivate();
                this[this.currentStateIndex].Activate();
                UIManager.Instance.LogSwitchUIState(this.controllerID, this.currentStateIndex);
            }
        }
        private void SetInitialState() {
            this.initialState.Activate();
            for (int i = 0; i < this.sequentialStates.Count; i++) {
                this.sequentialStates[i].Deactivate(immediate: true);
            }
            UIManager.Instance.LogSwitchUIState(this.controllerID, this.currentStateIndex);
        }
        public void SetStateFromManager(int index) {
            int previousIndex = this.currentStateIndex;
            this.currentStateIndex = Mathf.Clamp(index, 0, this.Count);
            if (previousIndex != this.currentStateIndex) {
                this[previousIndex].Deactivate();
                this[this.currentStateIndex].Activate();
            }
        }
    }
}
