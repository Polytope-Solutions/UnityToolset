using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PolytopeSolutions.Toolset.Input {
    public class InputManager : TManager<InputManager> {
        #region UI_HOVER_CHECK
        private GraphicRaycaster[] raycasters;
        private PointerEventData pointerEventData;
        private List<RaycastResult> rayCastUIResults = new List<RaycastResult>();
        public bool IsPointerOverUI {
            get {
                this.raycasters = GameObject.FindObjectsOfType<GraphicRaycaster>();
                this.rayCastUIResults.Clear();
                this.pointerEventData = new PointerEventData(EventSystem.current);
                this.pointerEventData.position = Pointer.current.position.value;
                foreach (GraphicRaycaster raycaster in this.raycasters) {
                    raycaster.Raycast(this.pointerEventData, this.rayCastUIResults);
                    if (this.rayCastUIResults.Count > 0)
                        return true;
                }
                return this.rayCastUIResults.Count > 0;
            }
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        protected Dictionary<string, InputReceiver> inputReceivers = new Dictionary<string, InputReceiver>();

        #region UNITY_FUNCTIONS
        protected IEnumerator Start() {
            // Wait one frame for all input receivers to be registered
            yield return null;
            ActivateReceivers();
        }
        protected virtual void OnEnable() {
            ActivateReceivers();
        }
        protected virtual void OnDisable() {
            DeactivateReceivers();
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        public void RegisterInputReceiver(string key, InputReceiver receiver) {
            if (!this.inputReceivers.ContainsKey(key))
                this.inputReceivers.Add(key, receiver);
            else
                this.inputReceivers[key] = receiver;
            receiver.SetActiveInputs(receiver.IsActiveByDefault);
        }
        public void UnregisterInputReceiver(string key) {
            if (this.inputReceivers.ContainsKey(key))
                this.inputReceivers.Remove(key);
        }
        private void ActivateReceivers() {
            if (this.inputReceivers.Count > 0)
                foreach (KeyValuePair<string, InputReceiver> receiver in this.inputReceivers)
                    receiver.Value.SetActiveInputs(receiver.Value.IsActiveByDefault);
        }
        private void DeactivateReceivers() {
            if (this.inputReceivers.Count > 0)
                foreach (KeyValuePair<string, InputReceiver> receiver in this.inputReceivers)
                    receiver.Value.SetActiveInputs(false);
        }

        public void InputReceiverSetActive(string key, bool targetState) { 
            if (this.inputReceivers.ContainsKey(key))
                this.inputReceivers[key].SetActiveInputs(targetState);
        }
        public void InputReceiverSetActiveExclusive(string key, bool targetState) {
            if (this.inputReceivers.Count > 0)
                foreach (KeyValuePair<string, InputReceiver> receiver in this.inputReceivers) { 
                    if (receiver.Key == key)
                        receiver.Value.SetTemporarilyActiveInputs(targetState);
                    else
                        receiver.Value.SetTemporarilyActiveInputs(!targetState);
                }
        }
        public void InputReceiverRestoreExclusive() {
            if (this.inputReceivers.Count > 0)
                foreach (KeyValuePair<string, InputReceiver> receiver in this.inputReceivers) {
                    receiver.Value.SetTemporarilyActiveInputs(true);
                }
        }
    }
}