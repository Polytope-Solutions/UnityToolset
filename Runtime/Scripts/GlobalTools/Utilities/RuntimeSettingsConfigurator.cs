using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class RuntimeSettingsConfigurator : MonoBehaviour {
        [SerializeField] private int targetFrameRate = 60;
        void Start() {
            Application.targetFrameRate = this.targetFrameRate;
        }
    }
}
