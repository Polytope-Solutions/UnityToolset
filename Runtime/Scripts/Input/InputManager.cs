using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PolytopeSolutions.Toolset.Scenes;

namespace PolytopeSolutions.Toolset.Input {
    public class InputManager : TManager<InputManager> {
        [SerializeField] private bool isUIStatic = true;
        #region UI_HOVER_CHECK
        private GraphicRaycaster[] raycasters;
        private PointerEventData pointerEventData;
        private List<RaycastResult> rayCastUIResults = new List<RaycastResult>();
        private bool isOverUIThisFrame = false;
        private int frameOfPointerUpdate = -1;
        public bool IsPointerOverUI {
            get {
                // Check if already dcalculated this frame and return cached result
                if (this.frameOfPointerUpdate == Time.frameCount)
                    return this.isOverUIThisFrame;
                // Otherwise, calculate and cache the result
                this.frameOfPointerUpdate = Time.frameCount;
                if (!this.isUIStatic || this.raycasters == null)
                    this.raycasters = GameObject.FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
                this.rayCastUIResults.Clear();
                this.pointerEventData = new PointerEventData(EventSystem.current);
                this.pointerEventData.position = Pointer.current.position.value;
                foreach (GraphicRaycaster raycaster in this.raycasters) {
                    raycaster.Raycast(this.pointerEventData, this.rayCastUIResults);
                    if (this.rayCastUIResults.Count > 0) {
                        this.isOverUIThisFrame = true;
                        return this.isOverUIThisFrame;
                    }
                }
                this.isOverUIThisFrame = this.rayCastUIResults.Count > 0;
                return this.isOverUIThisFrame;
            }
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        protected Dictionary<string, InputReceiver> inputReceivers = new Dictionary<string, InputReceiver>();

        #region UNITY_FUNCTIONS
        protected override void Awake() {
            base.Awake();
            if (this.isUIStatic)
                SceneManagerExtender.Instance.RegisterSingletonOnAfterSceneActivatedEvent("*", "InputManager", OnSceneLoad);
        }
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
        protected virtual void OnDestroy() {
            if (this.isUIStatic)
                SceneManagerExtender.Instance.UnregisterSingletonOnAfterSceneActivatedEvent("*", "InputManager");
        }
        protected void OnSceneLoad(string sceneName) {
            this.raycasters = null;
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