using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.Events;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class AnimationEventTriggerRelay : MonoBehaviour {
        [SerializeField] private List<AnimationEventDescriptor> descriptors;
        [System.Serializable]
        private class AnimationEventDescriptor {
            [SerializeField] private string identifier;
            [SerializeField] private UnityEvent onEventTriggered;

            public string Identifier => this.identifier;
            public void Invoke() => this.onEventTriggered?.Invoke();
        }

        public void TriggerEvent(string identifier) {
            this.descriptors.FirstOrDefault(descriptor => descriptor.Identifier == identifier)?.Invoke();
        }
    }
}
