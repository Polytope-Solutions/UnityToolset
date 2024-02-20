using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.Events;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Devices {
    public abstract class WebCameraTextureReceiver : MonoBehaviour {
        [SerializeField] protected WebCameraAccessor cameraAccessor;

        #region UNITY_FUNCTIONS
        protected virtual void Awake() {
            if (!this.cameraAccessor) {
                this.LogError("No CameraAccessor assigned");
                this.enabled = false;
            }
        }
        protected virtual void Start() {
            EventManager.Instance.RegisterEvenetCallback(WebCameraAccessor.WEBCAMERAS_CONNECTED_EVENTKEY, OnCamerasConnected);
        }
        protected virtual void OnDestroy() {
            EventManager.Instance?.UnregisterEvenetCallback(WebCameraAccessor.WEBCAMERAS_CONNECTED_EVENTKEY, OnCamerasConnected);
        }
        #endregion
        #region CAMERA_CONTROLS
        protected abstract void OnCamerasConnected();
        #endregion
    }
}