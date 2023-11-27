using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
    public class ProgressBar : MonoBehaviour {
        [SerializeField] protected Image progressBarImage;
        [SerializeField] protected float smoothDuration = 0.1f;

        protected virtual float targetProgress {
            get { return 0f; }
        }
        private float currentProgress;
        private float currentVelocity;

        private void Update() { 
            this.currentProgress = Mathf.SmoothDamp(this.currentProgress, this.targetProgress, ref this.currentVelocity, this.smoothDuration);
            this.progressBarImage.fillAmount = this.currentProgress;
        }
    }
}