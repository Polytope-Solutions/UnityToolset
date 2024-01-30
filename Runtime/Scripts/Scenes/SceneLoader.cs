using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Scenes {
    public class SceneLoader : MonoBehaviour {
        [SerializeField] protected string sceneName;
        [SerializeField] protected bool skipLoadingScene = false;
        [SerializeField] protected bool autoloadOnStart = false;

        protected virtual void Start() {
            if (this.autoloadOnStart)
                LoadScene();
        }
        public virtual void LoadScene() {
            SceneManagerExtender.Instance?.LoadScene(this.sceneName, this.skipLoadingScene);
        }
    }
}