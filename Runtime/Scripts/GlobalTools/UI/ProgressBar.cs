using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
    public class ProgressBar : MonoBehaviour {
        [SerializeField] private Image progressBarImage;

        protected virtual float progress {
            get { return 0f; }
        }

        void Update() { 
            this.progressBarImage.fillAmount = this.progress;
        }
    }
}