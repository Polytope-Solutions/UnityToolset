using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class ActivityPerformanceBaseFSceneSingleShotProvider : MonoBehaviour {
        protected virtual void OnEnable() {
            SceneManager.sceneLoaded += OnSceneChanged;
            SceneManager.sceneUnloaded += OnSceneChanged;
        }

        protected virtual void OnDisaable() {
            SceneManager.sceneLoaded -= OnSceneChanged;
            SceneManager.sceneUnloaded -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene _, LoadSceneMode __)
            => OnSceneChanged(_);
        private void OnSceneChanged(Scene _) {
            ActivityPerformanceManager.Instance.InformActivity();
        }
    }
}