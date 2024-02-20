using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Scenes {
    public class SceneLoader : SceneLoaderBase {
        [SerializeField] protected bool autoloadOnStart = false;

        protected virtual void Start() {
            if (this.autoloadOnStart)
                LoadScene();
        }
    }
}