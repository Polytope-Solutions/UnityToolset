using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Scenes {
    public class SceneLoader : MonoBehaviour {
        [SerializeField] private string sceneName;
        [SerializeField] private bool autoloadOnEnable = false;

        private void OnEnable() {
            if (this.autoloadOnEnable)
                LoadScene();
        }
        public void LoadScene() {
            SceneManagerExtender.instance?.LoadScene(sceneName);
        }
    }
}