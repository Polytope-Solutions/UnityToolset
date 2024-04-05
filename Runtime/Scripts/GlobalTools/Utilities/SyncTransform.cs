using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class SyncTransform : MonoBehaviour {
        [SerializeField] protected Transform target;

        protected virtual void Awake() {
            if (this.target == null) { 
                this.LogWarning($"No target transform assigned on {gameObject.name}. Disabling SyncTransform.");
                this.enabled = false;
            }
        }
        protected virtual void LateUpdate() {
            transform.position = this.target.position;
            transform.rotation = this.target.rotation;
            transform.localScale = this.target.localScale;
        }
    }
}
