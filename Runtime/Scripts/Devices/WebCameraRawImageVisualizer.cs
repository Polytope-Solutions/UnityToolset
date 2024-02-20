using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace PolytopeSolutions.Toolset.Devices {
    [RequireComponent(typeof(RawImage))]
    public class WebCameraRawImageVisualizer : WebCameraTextureReceiver {
        private RawImage rawImage;

        protected override void Awake() {
            this.rawImage = this.GetComponent<RawImage>();
            base.Awake();
        }

        protected override void OnCamerasConnected() {
            WebCamTexture texture = this.cameraAccessor.StartCameraTexture(0);
            this.rawImage.texture = texture;
        }
    }
}