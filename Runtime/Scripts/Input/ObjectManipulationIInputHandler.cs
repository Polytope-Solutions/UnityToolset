using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Input {
    public class ObjectManipulationInputHandler : ObjectInputHandler<ObjectManipulationInputReceiver> {

        protected override void Start() {
            base.Start();
            this.onInteractionStarted.AddListener(() => this.isReset = true);
            this.onInteractionPerformed.AddListener(OnInteracting);
        }

        ///////////////////////////////////////////////////////////////////////
        private bool isReset = true;
        private Vector3 startingReference;
        private void OnInteracting(RaycastHit hitInfo) {
            if (this.isReset) {
                this.startingReference = transform.position - hitInfo.point;
                this.isReset = false;
                return;
            }
            transform.position = hitInfo.point + this.startingReference;
        }
    }
}
