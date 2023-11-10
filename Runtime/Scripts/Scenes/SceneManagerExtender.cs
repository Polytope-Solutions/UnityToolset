using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using UnityEngine.SceneManagement;

using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Scenes {
    public class SceneManagerExtender : TManager<SceneManagerExtender> {
        [SerializeField] private string loaderSceneName;
        [SerializeField] private float smoothTransitionTime = 0.5f;
        [SerializeField] private float minWaitTime=2f;
        private Coroutine currentLoadingProcess;
        private float smoothProgress;
        public float SmoothProgress => this.smoothProgress;
        private float progress;
        public float Progress => this.progress;

        public Scene activeScene => SceneManager.GetActiveScene();
        public string activeSceneName => this.activeScene.name;

        public delegate void SceneEvent(string sceneName);
        private Dictionary<string, Dictionary<string, SceneEvent>> OnSceneLoaded = new Dictionary<string, Dictionary<string, SceneEvent>>();
        private Dictionary<string, Dictionary<string, SceneEvent>> OnSceneUnloaded = new Dictionary<string, Dictionary<string, SceneEvent>>();

        public void RegisterSingletonOnSceneLoadedEvent(string sceneName, string source, SceneEvent sceneEvent) { 
            if (!this.OnSceneLoaded.ContainsKey(sceneName))
                this.OnSceneLoaded.Add(sceneName, new Dictionary<string, SceneEvent>());
            if (!this.OnSceneLoaded[sceneName].ContainsKey(source))
                this.OnSceneLoaded[sceneName].Add(source, sceneEvent);
            else
                this.OnSceneLoaded[sceneName][source] = sceneEvent;
        }
        public void RegisterSingletonOnSceneUnloadedEvent(string sceneName, string source, SceneEvent sceneEvent) {
            if (!this.OnSceneUnloaded.ContainsKey(sceneName))
                this.OnSceneUnloaded.Add(sceneName, new Dictionary<string, SceneEvent>());

            if (!this.OnSceneUnloaded[sceneName].ContainsKey(source))
                this.OnSceneUnloaded[sceneName].Add(source, sceneEvent);
            else
                this.OnSceneUnloaded[sceneName][source] = sceneEvent;
        }
        public void UnregisterSingletonOnSceneLoadedEvent(string sceneName, string source, SceneEvent sceneEvent) {
            if (this.OnSceneLoaded.ContainsKey(sceneName) && this.OnSceneLoaded[sceneName].ContainsKey(source))
                this.OnSceneLoaded[sceneName][source] = null;
        }
        public void UnregisterSingletonOnSceneUnloadedEvent(string sceneName, string source, SceneEvent sceneEvent){
            if (this.OnSceneUnloaded.ContainsKey(sceneName) && this.OnSceneUnloaded[sceneName].ContainsKey(source))
                this.OnSceneUnloaded[sceneName][source] = null;
        }

        public void LoadScene(string sceneName, bool skipLoadingScene) {
            if (this.activeSceneName != sceneName) {
                if (this.currentLoadingProcess != null) StopCoroutine(this.currentLoadingProcess);
                this.currentLoadingProcess = StartCoroutine(LoadSceneCoroutine(sceneName, skipLoadingScene));
            }
        }
        private IEnumerator LoadSceneCoroutine(string sceneName, bool skipLoadingScene) {
            skipLoadingScene &= !string.IsNullOrEmpty(this.loaderSceneName);
            DateTime startTime = DateTime.Now;
            this.progress = 0f;
            string currentScene = this.activeSceneName;
            if (!skipLoadingScene) {
                // 1. Load the loading scene if it was specified.
                // - If there is loading scene - load it additively.
                AsyncOperation loaderSceneLoad = SceneManager.LoadSceneAsync(this.loaderSceneName, LoadSceneMode.Additive);
                yield return AwaitSceneAsyncOperation(loaderSceneLoad, Vector2.zero);
                // - Trigger SmoothTransition coroutine.
                yield return SmoothTransition(true);
                // 2. Unload the old scene
                // (NB! after loading scene as unloading can't happen without any scenes)
                yield return UnloadOldScene(currentScene, skipLoadingScene);
            }
            // 3. Load the new scene
            yield return LoadNewScene(sceneName, skipLoadingScene, startTime);

            if (!skipLoadingScene) {
                // 4. Unload the loader scene if it was loaded.
                // - Trigger SmoothTransition coroutine.
                yield return SmoothTransition(false);
                // - Unload the loading scene.
                AsyncOperation loaderSceneUnload = SceneManager.UnloadSceneAsync(this.loaderSceneName);
                yield return AwaitSceneAsyncOperation(loaderSceneUnload, Vector2.one);
            } else {
                // 5. Unload the old scene
                // (NB! after as if loading scene is skipped unloading can't happen without any scenes)
                yield return UnloadOldScene(currentScene, skipLoadingScene);
            }
            // 6. Reset loading coroutine.
            this.currentLoadingProcess = null;
        }
        private IEnumerator SmoothTransition(bool inTransition) { 
            this.smoothProgress = (inTransition) ? 0f : 1f;
            DateTime startTime = DateTime.Now;
            float timePassed = 0f;
            do {
                yield return null;
                timePassed = (float)(DateTime.Now - startTime).TotalSeconds;
                this.smoothProgress = (inTransition) ? 
                    Mathf.InverseLerp(0, this.smoothTransitionTime, timePassed) :
                    Mathf.InverseLerp(this.smoothTransitionTime, 0, timePassed);
            } while (timePassed < this.smoothTransitionTime);
            this.smoothProgress = (inTransition) ? 1f : 0f;
            yield return null;
        }
        private IEnumerator AwaitSceneAsyncOperation(AsyncOperation asyncOperation, Vector2 progressTargetRange) {
            while (!asyncOperation.isDone && asyncOperation.progress < 0.9f) {
                this.progress = Mathf.Lerp(progressTargetRange.x, progressTargetRange.y,
                    Mathf.InverseLerp(0f, 0.9f, asyncOperation.progress));
                yield return null;
            }
        }
        private IEnumerator LoadNewScene(string sceneName, bool skipLoadingScene, DateTime startTime) {
            // - Launch new scene loading and pause it's activation until complete.
            AsyncOperation newSceneLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            newSceneLoad.allowSceneActivation = false;
            // - Wait until the next scene fully loads asynchronously.
            Vector2 progressBarRange = (skipLoadingScene)?
                new Vector2(0f, 0.5f) :
                new Vector2(0.5f, 1f);
            yield return AwaitSceneAsyncOperation(newSceneLoad, progressBarRange);
            // - Trigger OnScene Loaded events
            if (this.OnSceneLoaded.ContainsKey(sceneName))
                this.OnSceneLoaded[sceneName].Values.ToList().ForEach(item => item?.Invoke(sceneName));
            this.progress = 1f;
            // - Wait for the remaining minimum wait time if relevant.
            float waitTime = (skipLoadingScene) ? 0f :
                Mathf.Max(0, this.minWaitTime - (float)(DateTime.Now - startTime).TotalSeconds - this.smoothTransitionTime * 2);
            yield return new WaitForSeconds(waitTime);
            // - Realease the scene loading async queue.
            newSceneLoad.allowSceneActivation = true;
            // - Scene is loaded - set it as active
            // (NB! has to happen after it is loaded, which won't happen till async queue is released)
            yield return null;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }
        private IEnumerator UnloadOldScene(string oldScene, bool skipLoadingScene) {
            // - Trigger OnSceneUnloaded events
            if (this.OnSceneUnloaded.ContainsKey(oldScene))
                this.OnSceneUnloaded[oldScene].Values.ToList().ForEach(item => item?.Invoke(oldScene));
            AsyncOperation oldSceneUnload = SceneManager.UnloadSceneAsync(oldScene);
            oldSceneUnload.allowSceneActivation = false;
            // - Wait until the last scene fully unloads asynchronously.
            Vector2 progressBarRange = (skipLoadingScene) ?
                new Vector2(0.5f, 1f) :
                new Vector2(1f, 0.5f);
            yield return AwaitSceneAsyncOperation(oldSceneUnload, progressBarRange);
            // - Realease the scene loading async queue.
            oldSceneUnload.allowSceneActivation = true;
            yield return null;
        }
    }
}