using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Scenes {
    public class SceneLoaderBase : MonoBehaviour {
        [SerializeField] protected string sceneName;
        [SerializeField] protected bool skipLoadingScene = false;

        public virtual void LoadScene() {
            SceneManagerExtender.Instance?.LoadScene(this.sceneName, this.skipLoadingScene);
        }
    }
}