#define DEBUG
//#undef DEBUG
//#define DEBUG2
#undef DEBUG2

using UnityEngine;
using System.Collections.Generic;

using System;

using UnityEngine.Rendering;

using PolytopeSolutions.Toolset.GlobalTools.Types;
using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Collections;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class ActivityPerformanceManager : TManager<ActivityPerformanceManager> {
        [Header("Idle Settings")]
        [SerializeField] private int idleRenderInterval = 2;
        [SerializeField] private int idleFPS = 8;
        [SerializeField] private float idleTimeScale = 0.1f;
        [SerializeField] private float idleForcedFrameTimeSeconds = 0.5f;
        [Header("Active Settings")]
        [SerializeField] private int activeRenderInterval = 1;
        [SerializeField] private int activeFPS = -1, activeWarmUpFPS = 30;
        [SerializeField] private float activeTimeScale = 1f;
        [SerializeField] private float activeWarmUpTimeSceonds = 0.05f;
        [Header("Transition Settings")]
        [SerializeField] private float interactionTimeOut = 5f;
        [SerializeField] private bool touchPhysics = true;

        private List<IActivityProvider> providers = new();

        private enum State {
            Idle,
            Active,
            ForceActive
        }
        private State state = State.Idle;
        public bool IsIdle => this.state == State.Idle;
        public bool IsActive => this.state == State.Active;
        private DateTime lastInteractionTime;
        private Coroutine idleForcedFrameCoroutine;
        private WaitForSecondsRealtime idleForcedFrameAwaiter;
        private Coroutine warmUpCoroutine;
        private WaitForSecondsRealtime warmUpAwaiter;

        #region UNITY_FUNCTIONS
        protected override void Awake() {
            this.warmUpCoroutine = null;
            this.idleForcedFrameAwaiter = new(this.idleForcedFrameTimeSeconds);
            this.warmUpAwaiter = new(this.activeWarmUpTimeSceonds);
#if DEBUG2
            Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobDebuggerEnabled = true;
#endif
            base.Awake();
            Prep();
            InformActivity();
        }
        protected virtual void Update() {
            if (this.state == State.ForceActive || this.state == State.Idle) return;
            if (!IsAnyProviderActive()) {
                if ((DateTime.UtcNow - this.lastInteractionTime).TotalSeconds > this.interactionTimeOut) {
                    EnterState(State.Idle);
                }
            }
        }
        protected virtual void OnDestroy() {
            if (this.warmUpCoroutine != null)
                StopCoroutine(this.warmUpCoroutine);
            if (this.idleForcedFrameCoroutine != null)
                StopCoroutine(this.idleForcedFrameCoroutine);
        }
        #endregion
        #region  EXTERNAL_FUNCTIONS
        public void InformActivity() {
            this.lastInteractionTime = DateTime.UtcNow;
            if (this.state == State.Idle) {
                EnterState(State.Active, false);
            }
        }
        public void RegisterProvider(IActivityProvider provider) {
            if (!this.providers.Contains(provider)) {
                this.providers.Add(provider);
                provider.OnActivityStateChanged += OnProviderStateChanged;
            }
        }
        public void UnregisterProvider(IActivityProvider provider) {
            if (this.providers.Contains(provider)) {
                this.providers.Remove(provider);
                provider.OnActivityStateChanged -= OnProviderStateChanged;
            }
        }
        public void SetForceActive(bool value) {
            if (value)
                EnterState(State.ForceActive);
            else
                EnterState(State.Idle);
        }
        #endregion
        #region INTERNAL_OPERATIONS
        private void Prep() {
            QualitySettings.vSyncCount = 0;
        }
        private void EnterState(State state, bool immediate = true) {
            State oldState = this.state;
            if (state == oldState) return;
            this.state = state;
#if DEBUG
            this.Log($"Entering {this.state.ToString()} state, setting targetFrameRate to {Application.targetFrameRate} and renderFrameInterval to {OnDemandRendering.renderFrameInterval} and timeScale to {Time.timeScale}");
#endif
            switch (this.state) {
                case State.Idle:
                    EnforceIdle();
                    break;
                case State.ForceActive:
                case State.Active:
                    if (immediate) {
                        if (this.warmUpCoroutine != null)
                            StopCoroutine(this.warmUpCoroutine);
                        this.warmUpCoroutine = StartCoroutine(WarmUpEnforceActive());
                    } else
                        EnforceActive();
                    break;
            }
        }

        private IEnumerator WarmUpEnforceActive() {
            OnDemandRendering.renderFrameInterval = this.activeRenderInterval;
            Application.targetFrameRate = this.activeWarmUpFPS;
            Time.timeScale = this.activeTimeScale;
            yield return this.warmUpAwaiter;
            EnforceActive();
            this.warmUpCoroutine = null;
        }
        private IEnumerator IdleForcedFrame() {
            while (this.state == State.Idle) {
                yield return this.idleForcedFrameAwaiter;
                OnDemandRendering.renderFrameInterval = 1;
                yield return null;
                OnDemandRendering.renderFrameInterval = this.idleRenderInterval;
            }
            this.idleForcedFrameCoroutine = null;
        }
        private void EnforceIdle() {
            if (this.warmUpCoroutine != null)
                StopCoroutine(this.warmUpCoroutine);
            OnDemandRendering.renderFrameInterval = this.idleRenderInterval;
            Application.targetFrameRate = this.idleFPS;
            Time.timeScale = this.idleTimeScale;
            Physics.autoSimulation = true;
            if (this.idleForcedFrameCoroutine != null)
                StopCoroutine(this.idleForcedFrameCoroutine);
            this.idleForcedFrameCoroutine = StartCoroutine(IdleForcedFrame());
            // if (Camera.main != null)
            // {
            //     Camera.main.enabled = false;
            // }
        }
        private void EnforceActive() {
            if (this.idleForcedFrameCoroutine != null)
                StopCoroutine(this.idleForcedFrameCoroutine);
            OnDemandRendering.renderFrameInterval = this.activeRenderInterval;
            Application.targetFrameRate = this.activeFPS;
            Time.timeScale = this.activeTimeScale;
            Physics.autoSimulation = false;
            // if (Camera.main != null)
            // {
            //     Camera.main.enabled = true;
            // }
        }

        private bool IsAnyProviderActive() {
            foreach (IActivityProvider provider in this.providers) {
                if (provider != null && provider.IsActive) {
                    return true;
                }
            }
            return false;
        }
        private void OnProviderStateChanged() {
            RecheckState();
        }
        private void RecheckState() {
            if (this.state == State.ForceActive) return;
            if (IsAnyProviderActive()) {
                EnterState(State.Active);
            } else {
                this.lastInteractionTime = DateTime.UtcNow;
            }
        }
        #endregion
    }
}