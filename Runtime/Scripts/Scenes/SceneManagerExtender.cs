using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using UnityEngine.SceneManagement;

namespace PolytopeSolutions.Toolset.Scenes {
    public class SceneManagerExtender : TManager<SceneManagerExtender> {
        [SerializeField] private string loaderSceneName;
        [SerializeField] private float minWaitTime=2f;
        private Coroutine currentLoadingProcess;
        private float progress;
        public float Progress => this.progress;

        public void LoadScene(string sceneName) {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != sceneName)
                this.currentLoadingProcess = StartCoroutine(LoadSceneCoroutine(sceneName));
        }
        private IEnumerator LoadSceneCoroutine(string sceneName) {
            this.progress = 0f;
            if (!string.IsNullOrEmpty(this.loaderSceneName))
                SceneManager.LoadScene(this.loaderSceneName);
            yield return new WaitForSeconds(this.minWaitTime);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            //asyncOperation.allowSceneActivation = false;

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone) {
                this.progress = asyncLoad.progress;
                yield return null;
            }
            this.progress = 1f;
            this.currentLoadingProcess = null;
        }
    }
}