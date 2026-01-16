#define DEBUG
// #undef DEBUG
#define DEBUG2
// #undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;
using PolytopeSolutions.Toolset.Events;
using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Devices {
    public class WebCameraAccessor : TManager<WebCameraAccessor> {
        [SerializeField] private bool autoRequestOnStart = false;
        private WebCamDevice[] cameras;
        public static string WEBCAMERAS_CONNECTED_EVENTKEY = "WEBCAMERAS_CONNECTED";
        private Dictionary<int, WebCamTexture> webCamTextures = new Dictionary<int, WebCamTexture>();

        public bool HasCameras => this.cameras != null && this.cameras.Length > 0;
        public int Count => this.HasCameras ? this.cameras.Length : 0;
        public List<string> CameraNames => this.HasCameras ? this.cameras.Select(item => item.name).ToList() : null;
        public WebCamTexture this[int index] {
            get {
                if (!this.HasCameras)
                    return null;
                if (this.webCamTextures != null && this.webCamTextures.ContainsKey(index))
                    return this.webCamTextures[index];
                return null;
            }
        }
        public bool IsFrontFacing(int index) => this.HasCameras ? this.cameras[index].isFrontFacing : false;

        private Dictionary<int, Action> webCameraStatusChangeCallbacks = new Dictionary<int, Action>();
        public void AddCallback(int cameraIndex, Action callback) {
            if (this.webCameraStatusChangeCallbacks.ContainsKey(cameraIndex))
                this.webCameraStatusChangeCallbacks[cameraIndex] += callback;
            else
                this.webCameraStatusChangeCallbacks.Add(cameraIndex, callback);
        }
        public void RemoveCallback(int cameraIndex, Action callback) {
            if (this.webCameraStatusChangeCallbacks.ContainsKey(cameraIndex))
                this.webCameraStatusChangeCallbacks[cameraIndex] -= callback;
        }

        public bool IsPlaying(int index)
            => this.IsPresent(index) && this.webCamTextures[index].isPlaying;
        public bool IsPresent(int index)
            => this.HasCameras ? this.webCamTextures.ContainsKey(index) : false;

        #region UNITY_FUNCTIONS
        protected void Start() {
            if (this.autoRequestOnStart)
                RequestAccess();
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            CleanUp();
        }
        #endregion
        #region GENERAL_CONTROLS
        [ContextMenu("CORE>Request camera access")]
        public void RequestAccess() {
            StartCoroutine(RequestAndListCameras());
        }
        private IEnumerator RequestAndListCameras() {
            CleanUp();
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
                this.cameras = WebCamTexture.devices;
                #if DEBUG
                this.Log($"Webcams found {this.cameras.Length}");
                #endif
                #if DEBUG2
                for (int cameraIndex = 0; cameraIndex < this.cameras.Length; cameraIndex++)
                    this.Log($"\tCamera name: {this.cameras[cameraIndex].name}: kind: {this.cameras[cameraIndex].kind}, front facing: {this.cameras[cameraIndex].isFrontFacing}"
                        + ((this.cameras[cameraIndex].availableResolutions != null)
                            ? $", resolutions: [{string.Join(",",this.cameras[cameraIndex].availableResolutions.Select(item => $"{item.width.ToString()}:{item.height.ToString()}"))}]" : string.Empty)
                    );
                #endif
                for (int cameraIndex = 0; cameraIndex < this.cameras.Length; cameraIndex++)
                    if (!this.webCameraStatusChangeCallbacks.ContainsKey(cameraIndex))
                        this.webCameraStatusChangeCallbacks.Add(cameraIndex, null);
            }
            else
                this.LogError("No webcams found");

            if (this.cameras != null && this.cameras.Length > 0)
                EventManager.Instance.InvokeEvent(WebCameraAccessor.WEBCAMERAS_CONNECTED_EVENTKEY);
        }
        private void CleanUp() {
            foreach (int cameraIndex in this.webCamTextures.Keys)
                StopCameraTexture(cameraIndex);
            this.webCamTextures.Clear();
            this.cameras = null;
        }
        #endregion
        #region CAMERA_CONTROLS
        public void StartCameraTexture(int cameraIndex, Action OnTextureReady=null) {
            if (!this.HasCameras) {
                this.LogError("No cameras found");
                return;
            }
            if (cameraIndex < 0 || cameraIndex >= this.cameras.Length) {
                this.LogError($"Camera index {cameraIndex} is out of range");
                return;
            }

            if (!this.webCamTextures.ContainsKey(cameraIndex)) {
                WebCamTexture texture = new WebCamTexture();
                texture.deviceName = this.cameras[cameraIndex].name;
                texture.filterMode = FilterMode.Trilinear;
                this.webCamTextures.Add(cameraIndex, texture);
            }
            this.webCamTextures[cameraIndex].Play();
            StartCoroutine(SetUpTextureInBuild(cameraIndex, OnTextureReady));
        }
        private IEnumerator SetUpTextureInBuild(int cameraIndex, Action OnTextureReady) {
            // For some stupid reason on Play the resolution might not yet be ready so need to wait for it.
            while (this.webCamTextures[cameraIndex].width < 100) {
                #if DEBUG2
                this.Log($"Camera is still not initialized: [{this.webCamTextures[cameraIndex].width}, {this.webCamTextures[cameraIndex].height}]");
                #endif
                yield return null;
            }
            #if DEBUG
            this.Log($"Camera is initialized: [{this.webCamTextures[cameraIndex].width}, {this.webCamTextures[cameraIndex].height}]");
            #endif
            OnTextureReady?.Invoke();
            if (this.webCameraStatusChangeCallbacks.ContainsKey(cameraIndex))
                this.webCameraStatusChangeCallbacks[cameraIndex]?.Invoke();
        }
        public void StopCameraTexture(int cameraIndex) {
            if (!this.HasCameras) {
                this.LogError("No cameras found");
                return;
            }
            if (this.webCamTextures.ContainsKey(cameraIndex)){
                this.webCamTextures[cameraIndex].Stop();
                Texture.Destroy(this.webCamTextures[cameraIndex]);
                this.webCamTextures.Remove(cameraIndex);
            }
            if (this.webCameraStatusChangeCallbacks.ContainsKey(cameraIndex))
                this.webCameraStatusChangeCallbacks[cameraIndex]?.Invoke();
        }
        public void PauseCameraTexture(int cameraIndex) {
            if (!this.HasCameras) {
                this.LogError("No cameras found");
                return;
            }
            if (this.webCamTextures.ContainsKey(cameraIndex))
                this.webCamTextures[cameraIndex].Pause();
            if (this.webCameraStatusChangeCallbacks.ContainsKey(cameraIndex))
                this.webCameraStatusChangeCallbacks[cameraIndex]?.Invoke();
        }

        [ContextMenu("DEFAULT>Start Default Camera")]
        public void StartDefaultCameraTexture(Action OnTextureReady) => StartCameraTexture(0, OnTextureReady);
        [ContextMenu("DEFAULT>Stop Default Camera")]
        public void StopDefaultCameraTexture() => StopCameraTexture(0);
        [ContextMenu("DEFAULT>Pause Default Camera")]
        public void PauseDefaultCameraTexture() => PauseCameraTexture(0);

        //[ContextMenu("ALL>Start All Cameras")]
        //public void StartAllCameras() {
        //    if (!this.HasCameras) {
        //        this.LogError("No cameras found");
        //        return;
        //    }
        //    for (int i = 0; i < this.cameras.Length; i++)
        //        StartCameraTexture(i);
        //}
        //[ContextMenu("ALL>Stop All Cameras")]
        //public void StopAllCameras() {
        //    if (!this.HasCameras) {
        //        this.LogError("No cameras found");
        //        return;
        //    }
        //    for (int i = 0; i < this.cameras.Length; i++)
        //        StopCameraTexture(i);
        //}
        //[ContextMenu("ALL>Pause All Cameras")]
        //public void PauseAllCameras() {
        //    if (!this.HasCameras) {
        //        this.LogError("No cameras found");
        //        return;
        //    }
        //    for (int i = 0; i < this.cameras.Length; i++)
        //        PauseCameraTexture(i);
        //}
        #endregion
    }
}