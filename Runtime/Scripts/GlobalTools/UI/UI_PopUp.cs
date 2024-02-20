using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
    public abstract class UI_PopUp : MonoBehaviour {
        [SerializeField] private GameObject goHolder;
        [SerializeField] private GameObject goMessageLabel;

        protected abstract bool AreDependenciesReady { get; }
        protected abstract Action<string> Callback { get; set; }

        #region UNITY_FUNCTIONS
        protected virtual void Start() {
            if (this.goHolder && this.goMessageLabel && this.AreDependenciesReady)
                this.Callback += ShowPopUp;
        }

        protected virtual void OnDestroy() {
            if (this.goHolder && this.goMessageLabel && this.AreDependenciesReady)
                this.Callback -= ShowPopUp;
        }
        #endregion

        private void ShowPopUp(string exception) {
            this.goHolder.SetActive(true);
            this.goMessageLabel.SetText(exception);
        }
    }
}