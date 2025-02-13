using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.RegularExpressions;
using PolytopeSolutions.Toolset.GlobalTools.Types;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    [System.Serializable]
    public class RuntimeSettings {
        #region OS
        [Header("OS")]
        [SerializeField] private int androidAPIVersion;
        [SerializeField] private int windowsVersion = 11;

        public virtual int AndroidAPI => this.androidAPIVersion;
        public virtual int Windows => this.windowsVersion;
        #endregion
        #region SETTINGS
        [Header("Settings")]
        [SerializeField] private float targetFrameRate = 60f;
        public float TargetFrameRate => this.targetFrameRate;
        #endregion
    }
    public class RuntimeSettingsConfigurator<TSettings> :
            TManager<RuntimeSettingsConfigurator<TSettings>> where TSettings : RuntimeSettings {
        [SerializeField] protected TSettings minSettings;
        [SerializeField] protected TSettings maxSettings;
        public float PerformanceFactor { private set; get; } = -1;

        private static readonly string androidAPIRegexPattern = @"API-(\d*)";
        private static readonly string windowsRegexPattern = @"Windows (\d*)";

        protected override void Awake() {
            base.Awake();
            EvaluateSystemFactor();
            SetUpSettings();
        }
        #region FACTOR_EVALUATION
        // Factor evaluation
        protected void EvaluateSystemFactor() {
            string operatingSystem = SystemInfo.operatingSystem;
            this.Log($"Operating System: {operatingSystem}");
            float t = 0f;
            if (operatingSystem.Contains("Android")) {
                int APIVersion = EvaluateAndroidAPI(operatingSystem);
                if (this.minSettings.AndroidAPI == this.maxSettings.AndroidAPI)
                    this.PerformanceFactor = .5f;
                else
                    this.PerformanceFactor =
                        Mathf.InverseLerp(this.minSettings.AndroidAPI, this.maxSettings.AndroidAPI, APIVersion);
            }
            else if (operatingSystem.Contains("Windows")) {
                int OSVersion = EvaluateWindows(operatingSystem);
                if (this.minSettings.Windows == this.maxSettings.Windows)
                    this.PerformanceFactor = .5f;
                else
                    this.PerformanceFactor =
                        Mathf.InverseLerp(this.minSettings.Windows, this.maxSettings.Windows, OSVersion);
            }
            this.Log($"Set up perfromance factor: {(this.PerformanceFactor * 100)}%.");
        }
        private int EvaluateAndroidAPI(string operatingSystem) {
            Match detection = Regex.Match(operatingSystem, androidAPIRegexPattern);
            if (!detection.Success) {
                this.LogError("Failed to detect Android API version");
                return this.minSettings.AndroidAPI;
            }
            string APIVersion = detection.Groups[1].Value;
            this.Log($"Android API Version {APIVersion}");
            return int.Parse(APIVersion);
        }
        private int EvaluateWindows(string operatingSystem) {
            Match detection = Regex.Match(operatingSystem, windowsRegexPattern);
            if (!detection.Success) {
                this.LogError("Failed to detect Windows version");
                return this.minSettings.Windows;
            }
            string OSVersion = detection.Groups[1].Value;
            this.Log($"Windows Version {OSVersion}");
            return int.Parse(OSVersion);
        }
        #endregion
        #region SETTINGS_ENFORCEMENT
        protected virtual void SetUpSettings() {
            SetTargetFrameRate();
        }
        protected void SetTargetFrameRate() {
            int targetFrameRate = Mathf.RoundToInt(
                Mathf.Lerp(this.minSettings.TargetFrameRate, this.maxSettings.TargetFrameRate, this.PerformanceFactor)
            );
            //float minTime = 1f / targetFrameRate;
            FrameRateSetter.SetFrameRate(targetFrameRate);
            this.Log($"Set up settings: framerate: {targetFrameRate}");
        }
        #endregion
    }
}
