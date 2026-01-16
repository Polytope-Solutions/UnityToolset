using System;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using static PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig.RigUtilities;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig {
    [Flags]
    public enum ThirdPersonRigJoint : uint {
        HorizontalPosition = 1,
        HorizontalRotation = 2,
        VerticalRotation = 4,
        Distance = 8,
        Camera = 16,
    }
    [Serializable]
    public class ThirdPersonRig<TRigState> : Rig<TRigState, ThirdPersonRigJoint>
            where TRigState : ThirdPersonRigState, new() {
        [SerializeField] private Transform tHorizontalPositioningJoint;
        [SerializeField] private Transform tHorizontalRotationJoint;
        [SerializeField] private Transform tVerticalRotationJoint;
        [SerializeField] private Transform tDistanceControlJoint;

        public Transform THorizontalPositioningJoint => this.tHorizontalPositioningJoint;
        public Transform THorizontalRotationJoint => this.tHorizontalRotationJoint;
        public Transform TVerticalRotationJoint => this.tVerticalRotationJoint;
        public Transform TDistanceControlJoint => this.tDistanceControlJoint;

        protected const uint allJoints = ThirdPersonRigState.allJoints;
        public override uint AllJoints => allJoints;
        public override void ApplyRigStateImmediate(ref TRigState state, uint updateState = allJoints) {
            base.ApplyRigStateImmediate(ref state, updateState);
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
        public override void ApplyRigStateSmoothed(ref TRigState state, float smoothTime, uint updateState = allJoints) {
            base.ApplyRigStateSmoothed(ref state, smoothTime, updateState);
            ThirdPersonRigJoint joints = (ThirdPersonRigJoint)updateState;
            if (state.IsChanged(ThirdPersonRigJoint.HorizontalPosition) && joints.HasFlag(ThirdPersonRigJoint.HorizontalPosition))
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPositionSmoothed(this.tHorizontalPositioningJoint.position.XZ(), smoothTime).ToXZ();
            if (state.IsChanged(ThirdPersonRigJoint.HorizontalRotation) && joints.HasFlag(ThirdPersonRigJoint.HorizontalRotation))
                this.tHorizontalRotationJoint.rotation
                    = Quaternion.Euler(Vector3.up * state.HorizontalAngleSmoothed(this.tHorizontalRotationJoint.eulerAngles.y, smoothTime));
            if (state.IsChanged(ThirdPersonRigJoint.VerticalRotation) && joints.HasFlag(ThirdPersonRigJoint.VerticalRotation))
                this.tVerticalRotationJoint.localRotation
                    = Quaternion.Euler(Vector3.right * state.VerticalAngleSmoothed(this.tVerticalRotationJoint.eulerAngles.x, smoothTime));
            if (state.IsChanged(ThirdPersonRigJoint.Distance) && joints.HasFlag(ThirdPersonRigJoint.Distance))
                this.tDistanceControlJoint.localPosition
                    = Vector3.forward * -state.DepthSmoothed(Mathf.Abs(this.tDistanceControlJoint.localPosition.z), smoothTime);
        }
        public override TRigState FromRig() {
            TRigState rigState = new();
            rigState.HorizontalPosition = this.tHorizontalPositioningJoint.position.XZ();
            rigState.HorizontalAngle = this.tHorizontalRotationJoint.eulerAngles.y;
            rigState.VerticalAngle = this.tVerticalRotationJoint.eulerAngles.x;
            rigState.Depth = Mathf.Abs(this.tDistanceControlJoint.localPosition.z);
            return rigState;
        }
        public override uint ResolvedMask(TRigState state) {
            uint mask = base.ResolvedMask(state);
            if ((this.tHorizontalPositioningJoint.position.XZ() - state.HorizontalPosition).sqrMagnitude < RigUtilities.epsilonSqr)
                mask |= ThirdPersonRigJoint.HorizontalPosition.ToUInt32();
            if (Mathf.Abs(Mathf.DeltaAngle(this.tHorizontalRotationJoint.eulerAngles.y, state.HorizontalAngle)) < RigUtilities.epsilon)
                mask |= ThirdPersonRigJoint.HorizontalRotation.ToUInt32();
            if (Mathf.Abs(Mathf.DeltaAngle(this.tVerticalRotationJoint.eulerAngles.x, state.VerticalAngle)) < RigUtilities.epsilon)
                mask |= ThirdPersonRigJoint.VerticalRotation.ToUInt32();
            if (Mathf.Abs(Mathf.Abs(this.tDistanceControlJoint.localPosition.z) - state.Depth) < RigUtilities.epsilon)
                mask |= ThirdPersonRigJoint.Distance.ToUInt32();
            return mask;
        }
    }
    [Serializable]
    public class ThirdPersonRigState : RigState<ThirdPersonRigJoint> {
        [SerializeField] private RigStateVector2Value horizontalPosition;
        [SerializeField] private RigStateAngleValue horizontalAngle;
        [SerializeField] private RigStateAngleValue verticalAngle;
        [SerializeField] private RigStateFloatValue depth;
        public new const uint allJoints = (uint)
            (ThirdPersonRigJoint.HorizontalPosition
            | ThirdPersonRigJoint.HorizontalRotation
            | ThirdPersonRigJoint.VerticalRotation
            | ThirdPersonRigJoint.Distance
            | ThirdPersonRigJoint.Camera);
        public ThirdPersonRigState() {
            this.horizontalPosition = new(
                ThirdPersonRigJoint.HorizontalPosition.ToUInt32(),
                this.MarkChanged
            );
            this.horizontalAngle = new(
                ThirdPersonRigJoint.HorizontalRotation.ToUInt32(),
                this.MarkChanged
            );

            this.verticalAngle = new(
                ThirdPersonRigJoint.VerticalRotation.ToUInt32(),
                this.MarkChanged
            );
            this.depth = new(
                ThirdPersonRigJoint.Distance.ToUInt32(),
                this.MarkChanged
            );
        }

        public Vector2 HorizontalPosition {
            get => this.horizontalPosition.Value;
            set => this.horizontalPosition.Value = value;
        }
        public Vector2 HorizontalPositionSmoothed(Vector2 current, float smoothTime)
            => this.horizontalPosition.ValueSmoothed(current, smoothTime);
        public float HorizontalAngle {
            get => this.horizontalAngle.Value;
            set => this.horizontalAngle.Value = value;
        }
        public float HorizontalAngleSmoothed(float current, float smoothTime)
            => this.horizontalAngle.ValueSmoothed(current, smoothTime);
        public float VerticalAngle {
            get => this.verticalAngle.Value;
            set => this.verticalAngle.Value = value;
        }
        public float VerticalAngleSmoothed(float current, float smoothTime)
            => this.verticalAngle.ValueSmoothed(current, smoothTime);
        public float Depth {
            get => this.depth.Value;
            set => this.depth.Value = value;
        }
        public float DepthSmoothed(float current, float smoothTime)
            => this.depth.ValueSmoothed(current, smoothTime);
    }
}
