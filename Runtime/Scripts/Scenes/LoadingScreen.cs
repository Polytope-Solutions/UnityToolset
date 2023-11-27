using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Scenes {
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreen : MonoBehaviour {
        protected float progress => (float)SceneManagerExtender.Instance?.SmoothLoadingAnimateInProgress;

        private CanvasGroup canvasGroup;

        private void Awake() {
            this.canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update() {
            this.canvasGroup.alpha = this.progress;
        }
    }
}