using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;
using TMPro;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Devices {
    // TODO: make it react to screen resizing
    public class UI_WebCameraTextureReceiverControlled : UI_WebCameraTextureReceiver {
        protected override bool IsAutoRequest
            => ((this.streamControl == null || !this.streamControl.IsValid) && base.IsAutoRequest) || this.streamControl.IsAutoRequest;
        protected override bool IsAutoStart
            => ((this.streamControl == null || !this.streamControl.IsValid) && base.IsAutoStart) || this.streamControl.IsAutoStart;

        [SerializeField] private StreamControl streamControl;
        [System.Serializable]
        public class StreamControl {
            [SerializeField] private bool blockUI;

            [SerializeField] private Button requestCameraAccessButton;

            [SerializeField] private TMP_Dropdown cameraSelectorDropdown;

            [SerializeField] private Button startStreamButton;
            [SerializeField] private Button pauseStreamButton;
            [SerializeField] private Button stopStreamButton;

            public enum StreamState {
                StartPauseStop,
                StartStop,
                Start
            }
            public bool IsAutoRequest => this.requestCameraAccessButton == null;
            public bool IsManualStart => this.startStreamButton != null;
            public bool IsAutoStart => !(this.startStreamButton != null || this.cameraSelectorDropdown != null);
            public bool IsValid
                => (this.requestCameraAccessButton
                    || ((this.startStreamButton && this.pauseStreamButton && this.stopStreamButton)
                        || (this.startStreamButton && !this.pauseStreamButton && this.stopStreamButton)
                        || (this.startStreamButton && this.pauseStreamButton && !this.stopStreamButton)
                        || (this.startStreamButton && !this.pauseStreamButton && !this.stopStreamButton)
                        || (!this.startStreamButton && !this.pauseStreamButton && this.stopStreamButton)
                        )
                );

            public void SetActive(bool state) {
                if (this.requestCameraAccessButton) this.requestCameraAccessButton.gameObject.SetActive(state);
                if (this.cameraSelectorDropdown) this.cameraSelectorDropdown.gameObject.SetActive(state);
                if (this.startStreamButton) this.startStreamButton.gameObject.SetActive(state);
                if (this.pauseStreamButton) this.pauseStreamButton.gameObject.SetActive(state);
                if (this.stopStreamButton) this.stopStreamButton.gameObject.SetActive(state);
            }
            public void Initialize(Action onRequest, Action<int> onSelectCamera, Action onStart, Action onPause, Action onStop) {
                if (this.requestCameraAccessButton)
                    this.requestCameraAccessButton.onClick.AddListener(() => onRequest?.Invoke());

                if (this.cameraSelectorDropdown) {
                    this.cameraSelectorDropdown.ClearOptions();
                    this.cameraSelectorDropdown.onValueChanged.AddListener(value => onSelectCamera?.Invoke(value-1));
                }

                if (this.startStreamButton)
                    this.startStreamButton.onClick.AddListener(() => onStart?.Invoke());
                if (this.pauseStreamButton)
                    this.pauseStreamButton.onClick.AddListener(() => onPause?.Invoke());
                if (this.stopStreamButton)
                    this.stopStreamButton?.onClick.AddListener(() => onStop?.Invoke());

                if (!this.startStreamButton && this.pauseStreamButton && this.stopStreamButton)
                    this.pauseStreamButton.gameObject.SetActive(false);
            }
            public void SetState(bool cameraAvailable, bool startStream, bool pauseStream, bool stopStream) {
                if (!cameraAvailable) {
                    SetMode(this.requestCameraAccessButton, true);
                    SetMode(this.cameraSelectorDropdown, false);
                    SetMode(this.startStreamButton, false);
                    SetMode(this.pauseStreamButton, false);
                    SetMode(this.stopStreamButton, false);
                }
                else if (this.requestCameraAccessButton && (!this.startStreamButton && startStream && !this.cameraSelectorDropdown)) {
                    SetMode(this.requestCameraAccessButton, true);
                    SetMode(this.cameraSelectorDropdown, true);
                    SetMode(this.pauseStreamButton, pauseStream);
                    SetMode(this.stopStreamButton, stopStream);
                }
                else {
                    SetMode(this.requestCameraAccessButton, false);
                    SetMode(this.cameraSelectorDropdown, startStream && !stopStream && !pauseStream);
                    SetMode(this.startStreamButton, startStream);
                    SetMode(this.pauseStreamButton, pauseStream);
                    SetMode(this.stopStreamButton, stopStream);
                }
            }
            public void ResetOptions(List<string> options) {
                if (this.cameraSelectorDropdown) {
                    this.cameraSelectorDropdown.ClearOptions();
                    this.cameraSelectorDropdown.AddOptions(new List<string> { "Select" });
                    this.cameraSelectorDropdown.AddOptions(options);
                }
            }
            private void SetMode(Button button, bool state) {
                if (!button) return;
                if (this.blockUI)
                    button.interactable = state;
                else
                    button.gameObject.SetActive(state);
            }
            private void SetMode(TMP_Dropdown dropdown, bool state) {
                if (!dropdown) return;
                if (this.blockUI)
                    dropdown.interactable = state;
                else
                    dropdown.gameObject.SetActive(state);
            }
        }

        protected override void Awake() {
            if (this.streamControl.IsValid) {
                this.streamControl.Initialize(
                    WebCameraAccessor.Instance.RequestAccess, SelectCamera,
                    StartStream, PauseStream, StopStream
                );
                // cameras not available, start stream mode.
                this.streamControl.SetState(false, false, false, false);
            }
            else {
                this.streamControl.SetActive(false);
            }
            base.Awake();
        }

        protected override void OnCamerasConnected() {
            base.OnCamerasConnected();
            if (this.streamControl.IsValid) {
                this.streamControl.ResetOptions(WebCameraAccessor.Instance.CameraNames);
                if (this.IsAutoStart)
                    this.streamControl.SetState(true, false, true, true);
                else
                    this.streamControl.SetState(true, true, false, false);
            }
        }

        public override void StartStream() {
            base.StartStream();
            if (this.streamControl.IsValid)
                this.streamControl.SetState(true, false, true, true);
        }
        public override void PauseStream() {
            base.PauseStream();
            if (this.streamControl.IsValid)
                this.streamControl.SetState(true, true, false, true);
        }
        public override void StopStream() {
            base.StopStream();
            if (this.streamControl.IsValid)
                this.streamControl.SetState(true, true, false, false);
        }
    }
}