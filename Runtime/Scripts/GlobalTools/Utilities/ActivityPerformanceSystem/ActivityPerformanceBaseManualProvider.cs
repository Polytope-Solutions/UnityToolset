using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class ActivityPerformanceBaseManualProvider : MonoBehaviour, IActivityProvider {
        private int counter = 0;
        public bool IsActive => this.counter > 0;
        public event System.Action OnActivityStateChanged;

        protected virtual void OnEnable() {
            this.counter = 0;
            ActivityPerformanceManager.Instance.RegisterProvider(this);
        }
        protected virtual void OnDisable() {
            if (ActivityPerformanceManager.Instance != null) {
                ActivityPerformanceManager.Instance.UnregisterProvider(this);
            }
        }
        public void InformActivity() {
            bool wasActive = this.IsActive;
            this.counter++;
            if (!wasActive) {
                this.OnActivityStateChanged?.Invoke();
            }
        }
        public void InformRelease() {
            bool wasActive = this.IsActive;
            this.counter = Mathf.Max(0, this.counter - 1);
            if (wasActive && !this.IsActive) {
                this.OnActivityStateChanged?.Invoke();
            }
        }
    }
}