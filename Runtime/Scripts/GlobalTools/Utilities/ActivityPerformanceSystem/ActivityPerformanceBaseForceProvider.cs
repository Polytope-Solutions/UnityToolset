using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class ActivityPerformanceBaseForceProvider : MonoBehaviour {
        protected virtual void OnEnable() {
            ActivityPerformanceManager.Instance.SetForceActive(true);
        }

        protected virtual void OnDisable() {
            ActivityPerformanceManager.Instance.SetForceActive(false);
        }
    }
}