using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class ActivityPerformanceBaseSingleShotProvider : MonoBehaviour {
        public void InformActivity() {
            ActivityPerformanceManager.Instance.InformActivity();
        }
    }
}