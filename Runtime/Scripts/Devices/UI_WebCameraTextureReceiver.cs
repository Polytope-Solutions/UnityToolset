using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;
using TMPro;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Devices {
    // TODO: make it react to screen resizing
    public class UI_WebCameraTextureReceiver : WebCameraTextureReceiver {
        [SerializeField] private Button requestCameraAccess;

        [SerializeField] private RawImage rawImage;
        [SerializeField] private bool maintainAspect = true;
        [SerializeField] private bool fitInScreen = true;
        private RectTransform rawImageRectTransform;
        private Vector2 originalSize;

        [SerializeField] private TMP_Dropdown cameraSelectorDropdown;

        [SerializeField] private Button startStremButton;
        [SerializeField] private Button pauseStreamButton;
        [SerializeField] private Button stopStreamButton;

        protected void Awake() {
            if (this.requestCameraAccess)
                this.requestCameraAccess.onClick.AddListener(WebCameraAccessor.Instance.RequestAccess);

            if (this.rawImage) { 
                this.rawImageRectTransform = this.rawImage.GetComponent<RectTransform>();
                this.originalSize = new Vector2(this.rawImageRectTransform.rect.width, this.rawImageRectTransform.rect.height);
            }
            if (this.cameraSelectorDropdown) { 
                this.cameraSelectorDropdown.interactable = false;
                this.cameraSelectorDropdown.ClearOptions();
                this.cameraSelectorDropdown.onValueChanged.AddListener(SelectCamera);
            }
            if (this.startStremButton) {
                this.startStremButton.interactable = false;
                this.startStremButton.onClick.AddListener(StartStream);
            }
            if (this.pauseStreamButton) {
                this.pauseStreamButton.interactable = false;
                this.pauseStreamButton.onClick.AddListener(PauseStream);
            }
            if (this.stopStreamButton) {
                this.stopStreamButton.interactable = false;
                this.stopStreamButton.onClick.AddListener(StopStream);
            }
        }

        protected override void OnCamerasConnected() { 
            base.OnCamerasConnected();
            if (this.requestCameraAccess)
                this.requestCameraAccess.interactable = false;
            if (this.cameraSelectorDropdown) {
                this.cameraSelectorDropdown.interactable = true;
                this.cameraSelectorDropdown.ClearOptions();
                this.cameraSelectorDropdown.AddOptions(WebCameraAccessor.Instance.CameraNames);
                this.cameraSelectorDropdown.value = this.SelectedCameraIndex;
                if (this.startStremButton)
                    this.startStremButton.interactable = true;
                if (this.pauseStreamButton)
                    this.pauseStreamButton.interactable = false;
                if (this.stopStreamButton)
                    this.stopStreamButton.interactable = false;
            } else if (!this.startStremButton){
                StartStream();
                if (this.pauseStreamButton)
                    this.pauseStreamButton.interactable = true;
                if (this.stopStreamButton)
                    this.stopStreamButton.interactable = true;
            }
        }

        public override void StartStream() {
            base.StartStream();
            if (this.cameraSelectorDropdown)
                this.cameraSelectorDropdown.interactable = false;
            if (this.startStremButton)
                this.startStremButton.interactable = false;
            if (this.pauseStreamButton)
                this.pauseStreamButton.interactable = true;
            if (this.stopStreamButton)
                this.stopStreamButton.interactable = true;
        }
        public override void PauseStream() {
            base.PauseStream();
            if (this.startStremButton)
                this.startStremButton.interactable = true;
            if (this.pauseStreamButton)
                this.pauseStreamButton.interactable = false;
            if (this.stopStreamButton)
                this.stopStreamButton.interactable = true;
        }
        public override void StopStream() {
            base.StopStream();
            if (this.rawImage)
                this.rawImage.texture = null;
            if (this.cameraSelectorDropdown) {
                this.cameraSelectorDropdown.interactable = true;
                if (this.startStremButton)
                    this.startStremButton.interactable = true;
                if (this.pauseStreamButton)
                    this.pauseStreamButton.interactable = false;
                if (this.stopStreamButton)
                    this.stopStreamButton.interactable = false;
            }
            else if (!this.startStremButton && this.requestCameraAccess) { 
                this.requestCameraAccess.interactable = true;
            }
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