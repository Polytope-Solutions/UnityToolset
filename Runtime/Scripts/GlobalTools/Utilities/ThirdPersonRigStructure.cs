using System;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    [Flags]
    public enum ThirdPersonRigJoint {
        HorizontalPosition = 1,
        HorizontalRotation = 2,
        VerticalRotation = 4,
        Distance = 8,
        Camera = 16
    }
    [Serializable]
    public class ThirdPersonRig<TRigState> where TRigState : ThirdPersonRigState, new() {
        [SerializeField] private Transform tHorizontalPositioningJoint;
        [SerializeField] private Transform tHorizontalRotationJoint;
        [SerializeField] private Transform tVerticalRotationJoint;
        [SerializeField] private Transform tDistanceControlJoint;
        [SerializeField] private Transform tCamera;
        private Camera camera;

        public Transform THorizontalPositioningJoint => this.tHorizontalPositioningJoint;
        public Transform THorizontalRotationJoint => this.tHorizontalRotationJoint;
        public Transform TVerticalRotationJoint => this.tVerticalRotationJoint;
        public Transform TDistanceControlJoint => this.tDistanceControlJoint;
        public Transform TCamera => this.tCamera;

        public Camera Camera {
            get {
                if (this.camera == null)
                    this.camera = this.tCamera.GetComponent<Camera>();
                return this.camera;
            }
        }

        public virtual void ApplyRigStateImmediate(TRigState state, uint updateState = ThirdPersonRigState.AllJoints) {
            ThirdPersonRigJoint joints = (ThirdPersonRigJoint)updateState;
            if (state.IsChanged((uint)ThirdPersonRigJoint.HorizontalPosition) && joints.HasFlag(ThirdPersonRigJoint.HorizontalPosition))
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPosition.ToXZ();
            if (state.IsChanged((uint)ThirdPersonRigJoint.HorizontalRotation) && joints.HasFlag(ThirdPersonRigJoint.HorizontalRotation))
                this.tHorizontalRotationJoint.rotation
                    = Quaternion.Euler(Vector3.up * state.HorizontalAngle);
            if (state.IsChanged((uint)ThirdPersonRigJoint.VerticalRotation) && joints.HasFlag(ThirdPersonRigJoint.VerticalRotation))
                this.tVerticalRotationJoint.localRotation
                    = Quaternion.Euler(Vector3.right * state.VerticalAngle);
            if (state.IsChanged((uint)ThirdPersonRigJoint.Distance) && joints.HasFlag(ThirdPersonRigJoint.Distance))
                this.tDistanceControlJoint.localPosition
                    = Vector3.forward * -state.Depth;
        }
        public virtual TRigState FromRig() {
            TRigState rigState = new();
            rigState.HorizontalPosition = this.tHorizontalPositioningJoint.position.XZ();
            rigState.HorizontalAngle = this.tHorizontalRotationJoint.eulerAngles.y;
            rigState.VerticalAngle = this.tVerticalRotationJoint.eulerAngles.x;
            rigState.Depth = Mathf.Abs(this.tDistanceControlJoint.localPosition.z);
            return rigState;
        }
    }
    [Serializable]
    public class ThirdPersonRigState {
        protected const float epsilon = 0.0001f;
        protected const float epsilonSqr = epsilon * epsilon;
        [SerializeField] private Vector2 horizontalPosition;
        [SerializeField] private float horizontalAngle;
        [SerializeField] private float verticalAngle;
        [SerializeField] private float depth;
        protected uint changeMask;
        public const uint AllJoints = (uint)
            (ThirdPersonRigJoint.HorizontalPosition
            | ThirdPersonRigJoint.HorizontalRotation
            | ThirdPersonRigJoint.VerticalRotation
            | ThirdPersonRigJoint.Distance
            | ThirdPersonRigJoint.Camera);

        public Vector2 HorizontalPosition {
            get => this.horizontalPosition;
            set {
                if ((this.horizontalPosition - value).sqrMagnitude < epsilonSqr)
                    return;
                this.horizontalPosition = value;
                this.changeMask |= (uint)ThirdPersonRigJoint.HorizontalPosition;
            }
        }
        public float HorizontalAngle {
            get => this.horizontalAngle;
            set {
                if (Mathf.Abs(this.horizontalAngle - value) < epsilon)
                    return;
                this.horizontalAngle = value;
                this.changeMask |= (uint)ThirdPersonRigJoint.HorizontalRotation;
            }
        }
        public float VerticalAngle {
            get => this.verticalAngle;
            set {
                if (Mathf.Abs(this.verticalAngle - value) < epsilon)
                    return;
                this.verticalAngle = value;
                this.changeMask |= (uint)ThirdPersonRigJoint.VerticalRotation;
            }
        }
        public float Depth {
            get => this.depth;
            set {
                if (Mathf.Abs(this.depth - value) < epsilon)
                    return;
                this.depth = value;
                this.changeMask |= (uint)ThirdPersonRigJoint.Distance;
            }
        }
        public bool IsChanged(uint testMask) => (this.changeMask & testMask) == testMask;
        public bool IsAnyChanged => this.changeMask != 0;

        public virtual void ClearMask(uint acknowledgedChanges = uint.MaxValue) {
            this.changeMask &= ~acknowledgedChanges;
        }
    }
}
