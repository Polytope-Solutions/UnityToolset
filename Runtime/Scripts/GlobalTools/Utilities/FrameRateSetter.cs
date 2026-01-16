using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class FrameRateSetter : MonoBehaviour {
        [SerializeField] protected int targetFrameRate = 60;
        [SerializeField] protected bool autoSetOnAwake;
        protected virtual void Awake() {
            if (this.autoSetOnAwake)
                SetFrameRate();
        }
        public void SetFrameRate()
            => FrameRateSetter.SetFrameRate(this.targetFrameRate);
        public static void SetFrameRate(int targetFrameRate) {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
