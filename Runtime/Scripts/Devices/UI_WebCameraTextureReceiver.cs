using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace PolytopeSolutions.Toolset.Devices {
    // TODO: make it react to screen resizing
    public class UI_WebCameraTextureReceiver : WebCameraTextureReceiver {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private bool maintainAspect = true;
        [SerializeField] private bool fitInScreen = true;
        private RectTransform rawImageRectTransform;
        private Vector2 originalSize;
        [SerializeField] private bool autoRequestCameraAccess;
        [SerializeField] private bool autoStart;

        protected virtual bool IsAutoRequest => this.autoRequestCameraAccess;
        protected virtual bool IsAutoStart => this.autoStart;

        protected virtual void Awake() {
            if (this.rawImage) { 
                this.rawImageRectTransform = this.rawImage.GetComponent<RectTransform>();
                this.originalSize = new Vector2(this.rawImageRectTransform.rect.width, this.rawImageRectTransform.rect.height);
            }
            if (this.IsAutoRequest)
                WebCameraAccessor.Instance.RequestAccess();
        }
        protected override void OnCamerasConnected() {
            base.OnCamerasConnected();
            if (this.IsAutoStart)
                StartStream();
        }
        public override void SelectCamera(int index) {
            if (index < 0) return;
            base.SelectCamera(index);
            if (this.IsAutoStart)
                StartStream();
        }
        public override void StopStream() {
            base.StopStream();
            if (this.rawImage)
                this.rawImage.texture = null;
        }
        protected override void OnTextureInitialized() { 
            UpdateRawImage();
        }
        private void UpdateRawImage() {
            if (!this.rawImage) return;
            WebCamTexture texture = this.ActiveTexture;
            this.rawImage.texture = texture;
            this.rawImage.uvRect = new Rect((this.IsHorizontallyFlipped) ? 1 : 0, (this.IsVerticallyFlipped) ? 1 : 0,
                (this.IsHorizontallyFlipped) ? -1 : 1, (this.IsVerticallyFlipped) ? -1 : 1);
            this.rawImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.originalSize.x);
            this.rawImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.originalSize.y);
            if (this.maintainAspect) {
                float holderAspect = this.rawImageRectTransform.rect.width / this.rawImageRectTransform.rect.height;
                float textureAspect = (float)texture.width / (float)texture.height;
                if (this.fitInScreen) {
                    if (holderAspect < textureAspect)
                        this.rawImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.rawImageRectTransform.rect.width / textureAspect);
                    else
                        this.rawImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.rawImageRectTransform.rect.height * textureAspect);
                }
                else {
                    if (holderAspect > textureAspect)
                        this.rawImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.rawImageRectTransform.rect.width / textureAspect);
                    else
                        this.rawImageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.rawImageRectTransform.rect.height * textureAspect);
                }
            }
            // TODO: Handle rotation
            #if DEBUG2
            this.Log($"RawImage Container Updated.")
            #endif
        }
    }
}