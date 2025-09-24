using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    [RequireComponent(typeof(AudioListener))]
    public class AutoDisableAudioListeners : MonoBehaviour {
        private void Start() {
            AudioListener[] listeners = GameObject.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            foreach (AudioListener listener in listeners) {
                if (listener.gameObject != gameObject)
                    listener.enabled = false;
            }
        }
    }
}
