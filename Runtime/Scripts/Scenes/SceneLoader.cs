using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Scenes {
    public class SceneLoader : MonoBehaviour {
        [SerializeField] private string sceneName;
        [SerializeField] private bool skipLoadingScene = false;
        [SerializeField] private bool autoloadOnStart = false;

        private void Start() {
            if (this.autoloadOnStart)
                LoadScene();
        }
        public void LoadScene() {
            SceneManagerExtender.Instance?.LoadScene(this.sceneName, this.skipLoadingScene);
        }
    }
}