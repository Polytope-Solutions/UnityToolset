using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
    public class PercentageIndicator : MonoBehaviour {
        [SerializeField] protected GameObject goIndicator;
        [SerializeField] protected float smoothDuration = 0.1f;

        protected virtual float targetProgress {
            get { return 0f; }
        }
        private float currentProgress;
        private float currentVelocity;

        private void Update() {
            this.currentProgress = Mathf.SmoothDamp(this.currentProgress, this.targetProgress, ref this.currentVelocity, this.smoothDuration);
            this.goIndicator.SetText($"{(this.currentProgress*100).ToString("F2")} %");
        }
    }
}