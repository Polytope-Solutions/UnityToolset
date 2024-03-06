using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    [RequireComponent(typeof(Animator))]
    public class AnimationTrigger : MonoBehaviour {
        [SerializeField] private string triggerName;
        [SerializeField] private bool triggerOnEnable;

        private Animator _animator;
        private Animator animator {
            get {
                if (this._animator == null) {
                    this._animator = GetComponent<Animator>();
                }
                return this._animator;
            }
        }

        protected virtual void OnEnable() {
            if (this.triggerOnEnable) Trigger();
        }

        public void Trigger() {
            this.animator.SetTrigger(this.triggerName);
        }
    }
}
