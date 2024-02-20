#define DEBUG
// #undef DEBUG
#define DEBUG2
// #undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;
using PolytopeSolutions.Toolset.Events;

namespace PolytopeSolutions.Toolset.Devices {
    public class WebCameraAccessor : MonoBehaviour {
        private WebCamDevice[] cameras;
        public static string WEBCAMERAS_CONNECTED_EVENTKEY = "WEBCAMERAS_CONNECTED";
        private Dictionary<int, WebCamTexture> webCamTextures = new Dictionary<int, WebCamTexture>();
        #region UNITY_FUNCTIONS
        protected void Start() {
            StartCoroutine(RequestAndListCameras());
        }
        protected void OnDestroy() {
            CleanUp();
        }
        #endregion
        #region GENERAL_CONTROLS
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
        public WebCamTexture StartCameraTexture(int cameraIndex) {
            if (cameraIndex >= this.cameras.Length) { 
                this.LogError($"Camera index {cameraIndex} is out of range");
                return null;
            }

            if (!this.webCamTextures.ContainsKey(cameraIndex)) { 
                WebCamTexture texture = new WebCamTexture(this.cameras[cameraIndex].name);
                this.webCamTextures.Add(cameraIndex, texture);
                texture.Play();
            }
            return this.webCamTextures[cameraIndex];
        }
        public void StopCameraTexture(int cameraIndex) {
            if (this.webCamTextures.ContainsKey(cameraIndex))
                this.webCamTextures[cameraIndex].Stop();
        }
        public void PauseCameraTexture(int cameraIndex) {
            if (this.webCamTextures.ContainsKey(cameraIndex))
                this.webCamTextures[cameraIndex].Pause();
        }
        #endregion
    }
}