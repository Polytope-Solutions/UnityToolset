#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Input {
    public abstract class ObjectInputHandler<T> : ExposedActionInputHandler<T> where T : InputReceiver {
        [SerializeField] protected List<Collider> colliders;

        ///////////////////////////////////////////////////////////////////////
        protected override void Start() {
            base.Start();
            if (this.colliders == null || this.colliders.Count == 0)
                this.LogWarning("No Colliders were given!");
            if (this.inputReceiver && this.colliders != null)
                this.inputReceiver.RegisterInputHandler(this);
        }
        ///////////////////////////////////////////////////////////////////////
        protected override bool IsRelevantHandler(RaycastHit hitInfo) {
            bool isValid = this.colliders.Contains(hitInfo.collider);
            #if DEBUG2
            this.Log($"Clicked on an interactable object: {isValid}");
            #endif
            return isValid;
        }
    }
}
