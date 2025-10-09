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

        protected bool IsChangedAndRelevant(TRigState state, ThirdPersonRigJoint joint, ThirdPersonRigJoint mask)
            => state.IsChanged(joint) && mask.HasFlag(joint);

        public virtual void ApplyRigStateImmediate(TRigState state, uint updateState = ThirdPersonRigState.AllJoints) {
            state.Constrain();
            ThirdPersonRigJoint joints = (ThirdPersonRigJoint)updateState;
            bool horizontalPositionChanged = IsChangedAndRelevant(state, ThirdPersonRigJoint.HorizontalPosition, joints),
                horizontalRotationChanged = IsChangedAndRelevant(state, ThirdPersonRigJoint.HorizontalRotation, joints),
                verticalRotationChanged = IsChangedAndRelevant(state, ThirdPersonRigJoint.VerticalRotation, joints),
                distanceChanged = IsChangedAndRelevant(state, ThirdPersonRigJoint.Distance, joints);
            if (horizontalPositionChanged)
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPosition.ToXZ();
            if (horizontalRotationChanged)
                this.tHorizontalRotationJoint.rotation
                    = Quaternion.Euler(Vector3.up * state.HorizontalAngle);
            if (verticalRotationChanged)
                this.tVerticalRotationJoint.localRotation
                    = Quaternion.Euler(Vector3.right * state.VerticalAngle);
            if (distanceChanged)
                this.tDistanceControlJoint.localPosition
                    = Vector3.forward * -state.Depth;
        }
        public virtual void ApplyRigStateSmoothed(TRigState state, float smoothTime, uint updateState = ThirdPersonRigState.AllJoints) {
            state.Constrain();
            ThirdPersonRigJoint joints = (ThirdPersonRigJoint)updateState;
            if (state.IsChanged((uint)ThirdPersonRigJoint.HorizontalPosition) && joints.HasFlag(ThirdPersonRigJoint.HorizontalPosition))
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPositionSmoothed(this.tHorizontalPositioningJoint.position.XZ(), smoothTime).ToXZ();
            if (state.IsChanged((uint)ThirdPersonRigJoint.HorizontalRotation) && joints.HasFlag(ThirdPersonRigJoint.HorizontalRotation))
                this.tHorizontalRotationJoint.rotation
                    = Quaternion.Euler(Vector3.up * state.HorizontalAngleSmoothed(this.tHorizontalRotationJoint.eulerAngles.y, smoothTime));
            if (state.IsChanged((uint)ThirdPersonRigJoint.VerticalRotation) && joints.HasFlag(ThirdPersonRigJoint.VerticalRotation))
                this.tVerticalRotationJoint.localRotation
                    = Quaternion.Euler(Vector3.right * state.VerticalAngleSmoothed(this.tVerticalRotationJoint.eulerAngles.x, smoothTime));
            if (state.IsChanged((uint)ThirdPersonRigJoint.Distance) && joints.HasFlag(ThirdPersonRigJoint.Distance))
                this.tDistanceControlJoint.localPosition
                    = Vector3.forward * -state.DepthSmoothed(Mathf.Abs(this.tDistanceControlJoint.localPosition.z), smoothTime);
        }
        public virtual TRigState FromRig() {
            TRigState rigState = new();
            rigState.HorizontalPosition = this.tHorizontalPositioningJoint.position.XZ();
            rigState.HorizontalAngle = this.tHorizontalRotationJoint.eulerAngles.y;
            rigState.VerticalAngle = this.tVerticalRotationJoint.eulerAngles.x;
            rigState.Depth = Mathf.Abs(this.tDistanceControlJoint.localPosition.z);
            return rigState;
        }
        public virtual bool IsStateApplied(TRigState state) {
            return (ResolvedMask(state) & ThirdPersonRigState.AllJoints) == ThirdPersonRigState.AllJoints;
        }
        public virtual uint ResolvedMask(TRigState state) {
            uint mask = 0;
            if ((this.tHorizontalPositioningJoint.position.XZ() - state.HorizontalPosition).sqrMagnitude < ThirdPersonRigState.epsilonSqr)
                mask |= (uint)ThirdPersonRigJoint.HorizontalPosition;
            if (Mathf.Abs(Mathf.DeltaAngle(this.tHorizontalRotationJoint.eulerAngles.y, state.HorizontalAngle)) < ThirdPersonRigState.epsilon)
                mask |= (uint)ThirdPersonRigJoint.HorizontalRotation;
            if (Mathf.Abs(Mathf.DeltaAngle(this.tVerticalRotationJoint.eulerAngles.x, state.VerticalAngle)) < ThirdPersonRigState.epsilon)
                mask |= (uint)ThirdPersonRigJoint.VerticalRotation;
            if (Mathf.Abs(Mathf.Abs(this.tDistanceControlJoint.localPosition.z) - state.Depth) < ThirdPersonRigState.epsilon)
                mask |= (uint)ThirdPersonRigJoint.Distance;
            return mask;
        }
    }
    [Serializable]
    public class ThirdPersonRigState {
        public const float epsilon = 0.0001f;
        public const float epsilonSqr = epsilon * epsilon;
        [SerializeField] private Vector2 horizontalPosition, horizontalPositionVelocity;
        [SerializeField] private float horizontalAngle, horizontalAngleVelocity;
        [SerializeField] private float verticalAngle, verticalAngleVelocity;
        [SerializeField] private float depth, depthVelocity;
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
        public Vector2 HorizontalPositionSmoothed(Vector2 current, float smoothTime)
            => Vector2.SmoothDamp(current, this.horizontalPosition, ref this.horizontalPositionVelocity, smoothTime);
        public float HorizontalAngle {
            get => this.horizontalAngle;
            set {
                if (Mathf.Abs(this.horizontalAngle - value) < epsilon)
                    return;
                this.horizontalAngle = value;
                this.changeMask |= (uint)ThirdPersonRigJoint.HorizontalRotation;
            }
        }
        public float HorizontalAngleSmoothed(float current, float smoothTime)
            => Mathf.SmoothDampAngle(current, this.horizontalAngle, ref this.horizontalAngleVelocity, smoothTime);
        public float VerticalAngle {
            get => this.verticalAngle;
            set {
                if (Mathf.Abs(this.verticalAngle - value) < epsilon)
                    return;
                this.verticalAngle = value;
                this.changeMask |= (uint)ThirdPersonRigJoint.VerticalRotation;
            }
        }
        public float VerticalAngleSmoothed(float current, float smoothTime)
            => Mathf.SmoothDampAngle(current, this.verticalAngle, ref this.verticalAngleVelocity, smoothTime);
        public float Depth {
            get => this.depth;
            set {
                if (Mathf.Abs(this.depth - value) < epsilon)
                    return;
                this.depth = value;
                this.changeMask |= (uint)ThirdPersonRigJoint.Distance;
            }
        }
        public float DepthSmoothed(float current, float smoothTime)
            => Mathf.SmoothDamp(current, this.depth, ref this.depthVelocity, smoothTime);
        public bool IsChanged(uint testMask) => (this.changeMask & testMask) == testMask;
        public bool IsChanged(ThirdPersonRigJoint joint)
            => IsChanged((uint)joint);
        public bool IsAnyChanged => this.changeMask != 0;

        public virtual void Constrain() {

        }
        public virtual void ClearMask(uint acknowledgedChanges = uint.MaxValue) {
            this.changeMask &= ~acknowledgedChanges;
        }
    }
}
