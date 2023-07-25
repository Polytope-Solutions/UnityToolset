using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class AnimatorBoolToggle : MonoBehaviour {
        public string animatorPropertyName;

        public void Toggle() {
            Animator animator = GetComponent<Animator>();
            animator.SetBool(this.animatorPropertyName, !animator.GetBool(this.animatorPropertyName));
        }
    }
}
