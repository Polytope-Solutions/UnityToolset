using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.Input {
    public class ObjectRotationInputHandler : ObjectInputHandler<ObjectRotationInputReceiver> {
        [SerializeField] private bool fixUpDirection = true;
        protected virtual Vector3 upDirection => Vector3.up;

        protected override void Start() {
            base.Start();
            this.onInteractionStarted.AddListener(() => this.isReset = true);
            this.onInteractionPerformed.AddListener(OnInteracting);
        }

        ///////////////////////////////////////////////////////////////////////
        private bool isReset = true;
        private Vector3 startingReference;
        private Quaternion startingRotation;
        private void OnInteracting(RaycastHit hitInfo) {
            if (this.isReset) {
                this.startingReference = hitInfo.point - transform.position;
                if (this.fixUpDirection)
                    this.startingReference = this.startingReference.DoubleCross(this.upDirection);
                this.startingRotation = transform.rotation;
                this.isReset = false;
                return;
            }
            Vector3 currentReference = hitInfo.point - transform.position;
            if (this.fixUpDirection)
                currentReference = currentReference.DoubleCross(this.upDirection);
            transform.rotation
                = Quaternion.FromToRotation(this.startingReference, currentReference) * this.startingRotation;
        }

    }
}
