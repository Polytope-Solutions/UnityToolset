#define DEBUG
// #undef DEBUG
#define DEBUG2
// #undef DEBUG2

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
        [SerializeField] private int historyBufferSize = 50;

        public static string PREVIOUS_STATE_EVENTKEY = "PreviousState";
        public static string NEXT_STATE_EVENTKEY = "NextState";
        public static string UISTATECHANGE_EVENTKEY = "UIStateChange";

        #region UNITY_FUNCTIONS
        protected override void Awake() {
            base.Awake();
            ResetHistory();
        }
        protected virtual void Start() {
            SceneManagerExtender.Instance.RegisterSingletonOnAfterSceneActivatedEvent("*", "UIManager", UIManager_OnAfterSceneActivated);
            EventManager.Instance.RegisterEvenetCallback(UIManager.PREVIOUS_STATE_EVENTKEY, this.Undo);
        }
        protected virtual void OnDestroy() {
            if (SceneManagerExtender.Instance)
                SceneManagerExtender.Instance.UnregisterSingletonOnAfterSceneActivatedEvent("*", "UIManager");
            if (EventManager.Instance)
                EventManager.Instance.UnregisterEvenetCallback(UIManager.PREVIOUS_STATE_EVENTKEY, this.Undo);
        }
        #endregion
        #region UI_CONTROLLERS
        private Dictionary<string, Action<string>> controllers = new Dictionary<string, Action<string>>();
        private void UIManager_OnAfterSceneActivated(string sceneName) {
            #if DEBUG
            this.Log($"Entering scene: {sceneName}. Refreshing controller references");
            #endif
            List<string> removeList = new List<string>();
            foreach (KeyValuePair<string, Action<string>> controllerHolder in this.controllers) {
                if (!controllerHolder.Key.Contains(sceneName + "|"))
                    removeList.Add(controllerHolder.Key);
            }
            for (int i = removeList.Count - 1; i >= 0; i--) {
                #if DEBUG2
                this.Log($"Removing controller reference: {removeList[i]}");
                #endif
                this.controllers.Remove(removeList[i]);
            }
        }
        public bool RegisterController(string controllerID, Action<string> ActivateSpecific) {
            string sceneName = SceneManagerExtender.Instance.CurrentSceneName;
            string controllerKey = sceneName + "|" + controllerID;
            this.controllers.Add(controllerKey, ActivateSpecific);
            #if DEBUG
            this.Log($"Adding a controller reference [{controllerKey}]");
            #endif
            // TODO: handle with scene initialization
            // Come to a scene that is the current in the history - activate corresponding UI
            //if (this.IsValidCurrentUISceneState
            //        && this.CurrentUISceneState.SceneName == SceneManagerExtender.Instance.CurrentSceneName 
            //        && this.CurrentUISceneState.ControllerID == controllerID) {
            //    ActivateCurrentState();
            //    return false;
            //}
            // Normal case - tell to activate the default one.
            return true;
        }
        #endregion
        #region UI_HISTORY
        private UISceneSatete[] uiSceneSatetes;
        private int currentUISceneSateteIndex = -1;
        private int firstUISceneStateIndex = 0, lastUISceneStateIndex = 0;
        private bool IsValidCurrentUISceneState =>
            this.currentUISceneSateteIndex >= 0 && this.currentUISceneSateteIndex < this.historyBufferSize
            && this.uiSceneSatetes[this.currentUISceneSateteIndex] != null;
        private UISceneSatete CurrentUISceneState => this.uiSceneSatetes[this.currentUISceneSateteIndex];
        public bool HistoryPresent => this.currentUISceneSateteIndex != this.firstUISceneStateIndex;
        public void LogSwitchUIState(string controllerID, string stateID) {
            string sceneName = SceneManagerExtender.Instance.CurrentSceneName;
            bool firstBufferloop = this.currentUISceneSateteIndex == -1;
            this.currentUISceneSateteIndex = (this.currentUISceneSateteIndex + 1) % this.historyBufferSize;
            if (!firstBufferloop && this.firstUISceneStateIndex == this.currentUISceneSateteIndex)
                this.firstUISceneStateIndex = (this.firstUISceneStateIndex + 1) % this.historyBufferSize;
            this.lastUISceneStateIndex = this.currentUISceneSateteIndex;
            this.uiSceneSatetes[this.currentUISceneSateteIndex] = new UISceneSatete(sceneName, controllerID, stateID);
            EventManager.Instance.InvokeEvent(UIManager.UISTATECHANGE_EVENTKEY);
        }
        public void ResetHistory() {
            this.uiSceneSatetes = new UISceneSatete[this.historyBufferSize];
            this.currentUISceneSateteIndex = -1;
            this.firstUISceneStateIndex = 0;
            this.lastUISceneStateIndex = 0;
            EventManager.Instance.InvokeEvent(UIManager.UISTATECHANGE_EVENTKEY);
        }
        public void Undo() {
            if (this.currentUISceneSateteIndex == this.firstUISceneStateIndex)
                return;
            this.currentUISceneSateteIndex = (this.currentUISceneSateteIndex - 1 + this.historyBufferSize) % this.historyBufferSize;
            ActivateCurrentState();
            EventManager.Instance.InvokeEvent(UIManager.UISTATECHANGE_EVENTKEY);
        }
        public void Redo() {
            if (this.currentUISceneSateteIndex == this.lastUISceneStateIndex)
                return;
            this.currentUISceneSateteIndex = (this.currentUISceneSateteIndex + 1) % this.historyBufferSize;
            ActivateCurrentState();
            EventManager.Instance.InvokeEvent(UIManager.UISTATECHANGE_EVENTKEY);
        }
        #endregion
        #region MISC
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
        #endregion
    }
}
