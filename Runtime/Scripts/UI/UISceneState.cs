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
    [System.Serializable]
    public class UISceneSatete {
        [SerializeField] protected string sceneName;
        [SerializeField] protected string controllerID;
        [SerializeField] protected string stateID;

        public string SceneName => this.sceneName;
        public string ControllerID => this.controllerID;
        public string StateID => this.stateID;

        public UISceneSatete(string _sceneName, string _controllerID, string _stateID) {
            this.sceneName = _sceneName;
            this.controllerID = _controllerID;
            this.stateID = _stateID;
        }
    }
}