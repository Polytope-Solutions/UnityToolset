using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.Events;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Devices {
    public abstract class WebCameraTextureReceiver : MonoBehaviour {
        private int selectedCameraIndex = -1;
        private int activatedCameraIndex = -1;
        protected int SelectedCameraIndex => this.selectedCameraIndex;
        protected int ActivatedCameraIndex => this.activatedCameraIndex;
        #region UNITY_FUNCTIONS
        protected virtual void Start() {
            if (!WebCameraAccessor.Instance) {
                this.LogError("No CameraAccessor found");
                this.enabled = false;
                return;
            }
            EventManager.Instance.RegisterEvenetCallback(WebCameraAccessor.WEBCAMERAS_CONNECTED_EVENTKEY, OnCamerasConnected);
        }
        protected virtual void OnDestroy() {
            EventManager.Instance?.UnregisterEvenetCallback(WebCameraAccessor.WEBCAMERAS_CONNECTED_EVENTKEY, OnCamerasConnected);
        }
        #endregion
        #region CAMERA_CONTROLS
        protected virtual void OnCamerasConnected() {
            this.selectedCameraIndex = 0;
        }
        public virtual void SelectCamera(int index) {
            this.selectedCameraIndex = index;
        }
        public virtual void StartStream() {
            if (this.selectedCameraIndex < 0) return;
            if (this.activatedCameraIndex != this.selectedCameraIndex) {
                StopStream();
                this.activatedCameraIndex = this.selectedCameraIndex;
            }
            WebCameraAccessor.Instance.StartCameraTexture(this.activatedCameraIndex, OnTextureInitialized);
        }
        public virtual void PauseStream() {
            if (this.activatedCameraIndex < 0) return;
            WebCameraAccessor.Instance.PauseCameraTexture(this.activatedCameraIndex);
        }
        public virtual void StopStream() {
            if (this.activatedCameraIndex < 0) return;
            WebCameraAccessor.Instance.StopCameraTexture(this.activatedCameraIndex);
        }
        protected WebCamTexture ActiveTexture => WebCameraAccessor.Instance[this.activatedCameraIndex];
        protected bool IsHorizontallyFlipped => WebCameraAccessor.Instance.IsFrontFacing(this.activatedCameraIndex);
        protected bool IsVerticallyFlipped => this.ActiveTexture.videoVerticallyMirrored;
        protected float CameraRotation => this.ActiveTexture.videoRotationAngle;

        protected abstract void OnTextureInitialized();
        #endregion
    }
}