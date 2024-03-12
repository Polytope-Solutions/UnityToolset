using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Scenes {
    public class SceneLoaderBase : MonoBehaviour {
        [SerializeField] protected string sceneName;
        [SerializeField] protected bool skipLoadingScene = false;

        public virtual void LoadScene() {
            if (SceneManagerExtender.Instance)
                SceneManagerExtender.Instance.LoadScene(this.sceneName, this.skipLoadingScene);
            else {
                this.LogWarning("No Scene Manager Extender: using default loading.");
                SceneManager.LoadScene(this.sceneName);
            }
        }
    }
}