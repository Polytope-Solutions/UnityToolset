using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities { 
    public class DetectTriggerEvents : MonoBehaviour {
        [SerializeField] private List<string> tags;
        [SerializeField] private UnityEvent onValidTriggerEnter;
        [SerializeField] private UnityEvent onValidTriggerStay;
        [SerializeField] private UnityEvent onValidTriggerExit;

        private void OnTriggerEnter(Collider other) {
            if (this.tags.Contains(other.tag)) {
                this.onValidTriggerEnter?.Invoke();
            }
        }
        private void OnTriggerStay(Collider other) {
            if (this.tags.Contains(other.tag)) {
                this.onValidTriggerStay?.Invoke();
            }
        }
        private void OnTriggerExit(Collider other) {
            if (this.tags.Contains(other.tag)) {
                this.onValidTriggerExit?.Invoke();
            }
        }
    }
}
