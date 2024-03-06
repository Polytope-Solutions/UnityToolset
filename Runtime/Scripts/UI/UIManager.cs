using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.SceneManagement;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using PolytopeSolutions.Toolset.Scenes;
using PolytopeSolutions.Toolset.Events;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.UI {
    public class UIManager : TManager<UIManager> {
        [SerializeField] private int bufferSize = 50;

        [System.Serializable]
        public class UISceneSatete {
            [SerializeField] protected string sceneName;
            [SerializeField] protected string controllerID;
            [SerializeField] protected int stateID;

            public string SceneName => this.sceneName;
            public string ControllerID => this.controllerID;
            public int StateID => this.stateID;

            public UISceneSatete(string _sceneName, string _controllerID, int _stateID) {
                this.sceneName = _sceneName;
                this.controllerID = _controllerID;
                this.stateID = _stateID;
            }
        }
        private UISceneSatete[] uiSceneSatetes;
        private int currentUISceneSateteIndex = -1;
        private int firstUISceneStateIndex = 0, lastUISceneStateIndex = 0;

        private Dictionary<string, Action<int>> controllers = new Dictionary<string, Action<int>>();

        private bool IsValidCurrentUISceneState =>
            this.currentUISceneSateteIndex >= 0 && this.currentUISceneSateteIndex < this.bufferSize
            && this.uiSceneSatetes[this.currentUISceneSateteIndex] != null;
        private UISceneSatete CurrentUISceneState => this.uiSceneSatetes[this.currentUISceneSateteIndex];


        protected override void Awake() {
            base.Awake();
            ResetHistory();
        }
        protected virtual void Start() {
            SceneManagerExtender.Instance.RegisterSingletonOnBeforeSceneUnloadedEvent("*", "UIManager", UIManager_BeforeSceneChange);
            EventManager.Instance.RegisterEvenetCallback("PreviousState", this.Undo);
        }
        protected virtual void OnDestroy() {
            if (SceneManagerExtender.Instance)
                SceneManagerExtender.Instance.UnregisterSingletonOnBeforeSceneUnloadedEvent("*", "UIManager");
            if (EventManager.Instance)
                EventManager.Instance.UnregisterEvenetCallback("PreviousState", this.Undo);
        }

        private void UIManager_BeforeSceneChange() {
            this.controllers.Clear();
            this.Log("Clearing controller references");
        }
        public bool RegisterController(string controllerID, Action<int> ActivateSpecific) {
            string sceneName = SceneManagerExtender.Instance.CurrentSceneName;
            string controllerKey = sceneName + "|" + controllerID;
            this.controllers.Add(controllerKey, ActivateSpecific);
            this.Log($"Adding a controller reference [{controllerKey}]");

            if (this.IsValidCurrentUISceneState
                    && this.CurrentUISceneState.SceneName == SceneManagerExtender.Instance.CurrentSceneName 
                    && this.CurrentUISceneState.ControllerID == controllerID) {
                // Come to a scene that is the current in the history - activate corresponding UI
                ActivateCurrentState();
                return false;
            }
            // Normal case - tell to activate the default one.
            return true;
        }


        public void ResetHistory() {
            this.uiSceneSatetes = new UISceneSatete[this.bufferSize];
            this.currentUISceneSateteIndex = -1;
            this.firstUISceneStateIndex = 0;
            this.lastUISceneStateIndex = 0;
        }

        public void LogSwitchUIState(string controllerID, int UIID) {
            string sceneName = SceneManagerExtender.Instance.CurrentSceneName;
            bool firstBufferloop = this.currentUISceneSateteIndex == -1;
            this.currentUISceneSateteIndex = (this.currentUISceneSateteIndex + 1) % this.bufferSize;
            if (!firstBufferloop && this.firstUISceneStateIndex == this.currentUISceneSateteIndex)
                this.firstUISceneStateIndex = (this.firstUISceneStateIndex + 1) % this.bufferSize;
            this.lastUISceneStateIndex = this.currentUISceneSateteIndex;
            this.uiSceneSatetes[this.currentUISceneSateteIndex] = new UISceneSatete(sceneName, controllerID, UIID);
        }
        public void Undo() {
            if (this.currentUISceneSateteIndex == this.firstUISceneStateIndex)
                return;
            this.currentUISceneSateteIndex = (this.currentUISceneSateteIndex - 1 + this.bufferSize) % this.bufferSize;
            ActivateCurrentState();
        }

        public void Redo() {
            if (this.currentUISceneSateteIndex == this.lastUISceneStateIndex)
                return;
            this.currentUISceneSateteIndex = (this.currentUISceneSateteIndex + 1) % this.bufferSize;
            ActivateCurrentState();
        }

        protected void ActivateCurrentState() {
            if (!this.IsValidCurrentUISceneState) { 
                this.LogError($"Current UI state is null");
                return;
            }
            if (this.CurrentUISceneState.SceneName != SceneManagerExtender.Instance.CurrentSceneName) {
                SceneManagerExtender.Instance.LoadScene(this.CurrentUISceneState.SceneName, false);
                return;
            }
            string controllerKey = this.CurrentUISceneState.SceneName + "|" + this.CurrentUISceneState.ControllerID;
            this.controllers[controllerKey](this.CurrentUISceneState.StateID);
        }
    }
}
