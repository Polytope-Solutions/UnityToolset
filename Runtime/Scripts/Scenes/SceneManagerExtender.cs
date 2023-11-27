#define DEBUG
// #undef DEBUG
#define DEBUG2
// #undef DEBUG2

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
        [SerializeField] private float minWaitTime = 2f;
        private Coroutine currentLoadingProcess;
        private float smoothLoadingAnimateInProgress;
        public float SmoothLoadingAnimateInProgress => this.smoothLoadingAnimateInProgress;
        public float LoadingProgress { get; private set; }
        private void UpdateProgress(float t) {
            this.LoadingProgress = t;
        }

        public string PreviousSceneName { get; private set; }
        public string CurrentSceneName { get; private set; }

        public Scene ActiveScene => SceneManager.GetActiveScene();
        public string ActiveSceneName => this.ActiveScene.name;

        public delegate IEnumerator SceneSetUpCoroutine(int currentOperationIndex, int operationCount, Action<float> updateProgress);
        private Dictionary<string, Dictionary<string, SceneSetUpCoroutine>> OnSceneSetup = new Dictionary<string, Dictionary<string, SceneSetUpCoroutine>>();
        private Dictionary<string, Dictionary<string, Action>> OnAfterSceneLoaded = new Dictionary<string, Dictionary<string, Action>>();
        private Dictionary<string, Dictionary<string, Action>> OnBeforeSceneUnloaded = new Dictionary<string, Dictionary<string, Action>>();
        
        public void RegisterSingletonOnSceneSetupEvent(string sceneName, string source, SceneSetUpCoroutine sceneEvent) { 
            if (!this.OnSceneSetup.ContainsKey(sceneName))
                this.OnSceneSetup.Add(sceneName, new Dictionary<string, SceneSetUpCoroutine>());
            if (!this.OnSceneSetup[sceneName].ContainsKey(source))
                this.OnSceneSetup[sceneName].Add(source, sceneEvent);
            else
                this.OnSceneSetup[sceneName][source] = sceneEvent;
        }
        public void RegisterSingletonOnAfterSceneLoadedEvent(string sceneName, string source, Action sceneEvent) { 
            if (!this.OnAfterSceneLoaded.ContainsKey(sceneName))
                this.OnAfterSceneLoaded.Add(sceneName, new Dictionary<string, Action>());
            if (!this.OnAfterSceneLoaded[sceneName].ContainsKey(source))
                this.OnAfterSceneLoaded[sceneName].Add(source, sceneEvent);
            else
                this.OnAfterSceneLoaded[sceneName][source] = sceneEvent;
        }
        public void RegisterSingletonOnBeforeSceneUnloadedEvent(string sceneName, string source, Action sceneEvent) {
            if (!this.OnBeforeSceneUnloaded.ContainsKey(sceneName))
                this.OnBeforeSceneUnloaded.Add(sceneName, new Dictionary<string, Action>());

            if (!this.OnBeforeSceneUnloaded[sceneName].ContainsKey(source))
                this.OnBeforeSceneUnloaded[sceneName].Add(source, sceneEvent);
            else
                this.OnBeforeSceneUnloaded[sceneName][source] = sceneEvent;
        }
        public void UnregisterSingletonOnSceneSetupEvent(string sceneName, string source) {
            if (this.OnSceneSetup.ContainsKey(sceneName) && this.OnSceneSetup[sceneName].ContainsKey(source))
                this.OnSceneSetup[sceneName].Remove(source);
        }
        public void UnregisterSingletonOnAfterSceneLoadedEvent(string sceneName, string source) {
            if (this.OnAfterSceneLoaded.ContainsKey(sceneName) && this.OnAfterSceneLoaded[sceneName].ContainsKey(source))
                this.OnAfterSceneLoaded[sceneName].Remove(source);
        }
        public void UnregisterSingletonOnBeforeSceneUnloadedEvent(string sceneName, string source){
            if (this.OnBeforeSceneUnloaded.ContainsKey(sceneName) && this.OnBeforeSceneUnloaded[sceneName].ContainsKey(source))
                this.OnBeforeSceneUnloaded[sceneName].Remove(source);
        }

        protected override void Awake() { 
            base.Awake();
            // through intance to ensuere it is assigned from the beginning.
            SceneManagerExtender.Instance.CurrentSceneName = this.ActiveSceneName;
            #if DEBUG
            Debug.Log("SceneManagerExtender: Starting scene: " + this.CurrentSceneName);
            #endif
        }
        public void LoadScene(string sceneName, bool skipLoadingScene) {
            if (this.ActiveSceneName != sceneName) {
                if (this.currentLoadingProcess != null) StopCoroutine(this.currentLoadingProcess);
                this.currentLoadingProcess = StartCoroutine(LoadSceneCoroutine(sceneName, skipLoadingScene));
            }
        }
        private IEnumerator LoadSceneCoroutine(string sceneName, bool skipLoadingScene) {
            if (this.ActiveScene == null)
                yield return ActivateScene(this.CurrentSceneName);
            this.PreviousSceneName = this.CurrentSceneName;
            this.CurrentSceneName = sceneName;

            skipLoadingScene &= !string.IsNullOrEmpty(this.loaderSceneName);
            DateTime startTime = DateTime.Now;
            this.LoadingProgress = 0f;
            int currentOperationIndex = 0;
            List<string> currentSceneNames = GetLoadedSceneNames();
            int extraOperations = (this.OnSceneSetup.ContainsKey(sceneName)) ?
                this.OnSceneSetup[sceneName].Count : 0;
            int operationCount = 2              // Load New, Unload Current Assets
                + currentSceneNames.Count()     // Unload Current
                + extraOperations               // Setup Operations if any
                + ((skipLoadingScene) ? 0 : 2); // Load Loader, Unload Loader
            #if DEBUG
            Debug.Log("SceneManagerExtender: Loading scene: " + this.CurrentSceneName + " from: " + this.PreviousSceneName 
                + ". Skipping loadscene: " + skipLoadingScene + ". Tasks: " + operationCount);
            #endif
            if (!skipLoadingScene) {
                // 1. Load the loading scene if it was specified.
                // - If there is loading scene - load it additively.
                AsyncOperation loaderSceneLoad = SceneManager.LoadSceneAsync(this.loaderSceneName, LoadSceneMode.Additive);
                yield return AwaitSceneAsyncOperation(loaderSceneLoad,
                    currentOperationIndex, operationCount);
                currentOperationIndex++;
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Loader scene loaded");
                #endif
                // - Trigger SmoothTransition coroutine.
                yield return SmoothTransition(true, operationCount);
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Loader smooth transitioning in complete");
                #endif
                // 2. Unload the old scenes
                yield return UnloadOldScenes(currentSceneNames, currentOperationIndex, operationCount);
                currentOperationIndex += currentSceneNames.Count();
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Scenes Unloaded.");
                #endif
                
                // - In case of async unloading assets don't get released automatically. 
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Unloading assets.");
                #endif
                AsyncOperation assetUnload = Resources.UnloadUnusedAssets();
                yield return AwaitSceneAsyncOperation(assetUnload, currentOperationIndex, operationCount);
                currentOperationIndex++;
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Unloaded assets.");
                #endif
            }
            // 3. Load the new scene
            // - Launch new scene loading and pause it's activation until complete.
            AsyncOperation newSceneLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            newSceneLoad.allowSceneActivation = false;
            yield return LoadNewScene(newSceneLoad, sceneName, startTime,
                currentOperationIndex, operationCount);
            currentOperationIndex += 1 + extraOperations;
            #if DEBUG2
            Debug.Log("SceneManagerExtender: New scene loaded.");
            #endif
            // - Wait for the remaining minimum wait time if relevant.
            float waitTime = (skipLoadingScene) ? 0f :
                Mathf.Max(0, this.minWaitTime - (float)(DateTime.Now - startTime).TotalSeconds - this.smoothTransitionTime * 2);
            yield return new WaitForSeconds(waitTime);
            // - Realease the scene loading async queue and Activate the scene.
            newSceneLoad.allowSceneActivation = true;
            yield return ActivateScene(sceneName);
            #if DEBUG2
            Debug.Log("SceneManagerExtender: New scene activated.");
            #endif

            if (!skipLoadingScene) {
                // 4. Unload the loader scene if it was loaded.
                // - Trigger SmoothTransition coroutine.
                yield return SmoothTransition(false, operationCount);
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Loader smooth transitioning out complete");
                #endif
                // - Unload the loading scene.
                AsyncOperation loaderSceneUnload = SceneManager.UnloadSceneAsync(this.loaderSceneName);
                yield return AwaitSceneAsyncOperation(loaderSceneUnload,
                    currentOperationIndex, operationCount);
                currentOperationIndex++;
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Loader scene unloaded");
                #endif
            } else {
                // 5. Unload the old scenes
                yield return UnloadOldScenes(currentSceneNames, currentOperationIndex, operationCount);
                currentOperationIndex += currentSceneNames.Count();
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Scenes Unloaded.");
                #endif
                
                // - In case of async unloading assets don't get released automatically. 
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Unloading assets.");
                #endif
                AsyncOperation assetUnload = Resources.UnloadUnusedAssets();
                yield return AwaitSceneAsyncOperation(assetUnload, currentOperationIndex, operationCount);
                currentOperationIndex++;
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Unloaded assets.");
                #endif
            }
            // 6. Reset loading coroutine.
            this.currentLoadingProcess = null;
            #if DEBUG
            Debug.Log("SceneManagerExtender: Finished loading scene: " + sceneName 
                + ". Active scenes: " + SceneManager.loadedSceneCount
                + ". Active scene: " + this.ActiveSceneName);
            #endif
        }
        private List<string> GetLoadedSceneNames() { 
            List<string> sceneNames = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded || scene.name == this.PreviousSceneName)
                    sceneNames.Add(scene.name);
            }
            return sceneNames;
        }
        private IEnumerator SmoothTransition(bool inTransition, int operationsCount) { 
            this.LoadingProgress = (inTransition) ? 0f : 1f - 1f / operationsCount;
            this.smoothLoadingAnimateInProgress = (inTransition) ? 0f : 1f;
            #if DEBUG2
            Debug.Log("SceneManagerExtender: Transitioning: " + inTransition + ". Transition time: " + this.smoothTransitionTime);
            #endif
            DateTime startTime = DateTime.Now;
            float timePassed = 0f, t = 0f;
            do {
                yield return null;
                timePassed = (float)(DateTime.Now - startTime).TotalSeconds;
                t = timePassed/ this.smoothTransitionTime;
                this.LoadingProgress = (inTransition) ? 
                    Mathf.Lerp(0f, 1f/operationsCount, t) :
                    Mathf.Lerp(1f-1f/operationsCount, 1f, t);
                this.smoothLoadingAnimateInProgress = (inTransition) ? t : 1-t;
            } while (timePassed < this.smoothTransitionTime);
            this.smoothLoadingAnimateInProgress = (inTransition) ? 1f : 0f;
            this.LoadingProgress = (inTransition) ? 1f/operationsCount : 1f;
        }
        private IEnumerator AwaitSceneAsyncOperation(AsyncOperation asyncOperation,
                int currentOperationIndex, int operationCount) {
            Vector2 progressTargetRange = new Vector2(
                Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0, operationCount, currentOperationIndex)),
                Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0, operationCount, currentOperationIndex + 1))
            );
            this.LoadingProgress = progressTargetRange.x;
            while (!asyncOperation.isDone && asyncOperation.progress < 0.9f) {
                yield return null;
                this.LoadingProgress = Mathf.Lerp(progressTargetRange.x, progressTargetRange.y,
                    Mathf.InverseLerp(0f, 0.9f, asyncOperation.progress));
            }
            this.LoadingProgress = progressTargetRange.y;
        }
        private IEnumerator LoadNewScene(AsyncOperation newSceneLoad, string sceneName, DateTime startTime,
                int currentOperationIndex, int operationCount) {
            // - Wait until the next scene fully loads asynchronously.
            yield return AwaitSceneAsyncOperation(newSceneLoad, 
                currentOperationIndex, operationCount);
            currentOperationIndex++;
            // - Trigger OnScene Loaded events
            if (this.OnAfterSceneLoaded.ContainsKey(sceneName))
                this.OnAfterSceneLoaded[sceneName].Values.ToList().ForEach(item => item?.Invoke());
            yield return null;
            // - Wait for setup events to finish
            if (this.OnSceneSetup.ContainsKey(sceneName)) { 
                foreach (KeyValuePair<string, SceneSetUpCoroutine> setupCoroutine in this.OnSceneSetup[sceneName]) { 
                    #if DEBUG2
                    Debug.Log("SceneManagerExtender: Executing setup coroutine: " + setupCoroutine.Key);
                    #endif
                    yield return setupCoroutine.Value(currentOperationIndex, operationCount, UpdateProgress);
                    currentOperationIndex++;
                }
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Setup coroutines finished");
                #endif
            }
        }
        private void BeforeSceneUnload(string oldSceneName) {
            // - Trigger OnBeforeSceneUnloaded events
            if (this.OnBeforeSceneUnloaded.ContainsKey(oldSceneName))
                this.OnBeforeSceneUnloaded[oldSceneName].Values.ToList().ForEach(item => item?.Invoke());
        }
        private IEnumerator UnloadOldScenes(List<string> currentScenes, int currentOperationIndex, int operationCount) {
            foreach (string oldSceneName in currentScenes) { 
                // - Trigger OnBeforeSceneUnloaded events
                BeforeSceneUnload(oldSceneName);
                // - Unload the old scene
                // (NB! after loading loading scene as unloading can't happen without any scenes)
                AsyncOperation oldSceneUnload = SceneManager.UnloadSceneAsync(oldSceneName);
                yield return UnloadOldScene(oldSceneUnload, oldSceneName, 
                    currentOperationIndex, operationCount);
                currentOperationIndex++;
                #if DEBUG2
                Debug.Log("SceneManagerExtender: Old scene ["+ oldSceneName + "] unloaded");
                #endif
            }
        }
        private IEnumerator UnloadOldScene(AsyncOperation oldSceneUnload, string oldSceneName, 
            int currentOperationIndex, int operationCount) {
            oldSceneUnload.allowSceneActivation = false;
            // - Wait until the last scene fully unloads asynchronously.
            yield return AwaitSceneAsyncOperation(oldSceneUnload, currentOperationIndex, operationCount);
            currentOperationIndex++;
            // - Realease the scene loading async queue.
            oldSceneUnload.allowSceneActivation = true;
            yield return null;
        }
        private IEnumerator ActivateScene(string sceneName) {
            bool loaded = false;
            Scene newScene;
            do {
                yield return null;
                if (this.ActiveSceneName == sceneName)
                    break;
                // Fix for build: Scene loading doesn't happen fast enough for the scene to be set as active.
                // - wait and try to set the scene as active until it is set properly.
                try{ 
                    newScene = SceneManager.GetSceneByName(sceneName);
                    loaded = SceneManager.SetActiveScene(newScene);
                }
                catch {
                    Debug.LogWarning("SceneManagerExtender: Scene ["+sceneName+"] not loaded yet. Waiting...");
                    loaded = false;
                }
            }
            while (!loaded);
        }
    }
}