using System;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using static PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig.RigUtilities;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig {
    [Flags]
    public enum FirstPersonRigJoint : uint {
        HorizontalPosition = 1,
        InRigOffset = 2,
        Rotation = 4,
        Camera = 8,
    }
    [Serializable]
    public class FirstPersonRig<TRigState> : Rig<TRigState, FirstPersonRigJoint>
            where TRigState : FirstPersonRigState, new() {
        [SerializeField] private Transform tHorizontalPositioningJoint;
        [SerializeField] private Transform tInRigOffsetJoint;
        [SerializeField] private Transform tRotationJoint;

        public Transform THorizontalPositioningJoint => this.tHorizontalPositioningJoint;
        public Transform TInRigOffsetJoint => this.tInRigOffsetJoint;
        public Transform TRotationJoint => this.tRotationJoint;

        public const uint allJoints = FirstPersonRigState.allJoints;
        public override uint AllJoints => allJoints;

        public override void ApplyRigStateImmediate(ref TRigState state, uint updateState = allJoints) {
            base.ApplyRigStateImmediate(ref state, updateState);
            FirstPersonRigJoint joints = (FirstPersonRigJoint)updateState;
            bool horizontalPositionChanged = IsChangedAndRelevant(state, FirstPersonRigJoint.HorizontalPosition, joints),
                inRigOffsetChanged = IsChangedAndRelevant(state, FirstPersonRigJoint.InRigOffset, joints),
                rotationChanged = IsChangedAndRelevant(state, FirstPersonRigJoint.Rotation, joints);
            if (horizontalPositionChanged)
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPosition.ToXZ();
            if (inRigOffsetChanged)
                this.tInRigOffsetJoint.localPosition
                    = state.InRigOffset;
            if (rotationChanged)
                this.tRotationJoint.localRotation
                    = state.Rotation;
        }
        public override void ApplyRigStateSmoothed(ref TRigState state, float smoothTime, uint updateState = allJoints) {
            base.ApplyRigStateSmoothed(ref state, smoothTime, updateState);
            FirstPersonRigJoint joints = (FirstPersonRigJoint)updateState;
            if (state.IsChanged(FirstPersonRigJoint.HorizontalPosition) && joints.HasFlag(FirstPersonRigJoint.HorizontalPosition))
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPositionSmoothed(this.tHorizontalPositioningJoint.position.XZ(), smoothTime).ToXZ();
            if (state.IsChanged(FirstPersonRigJoint.InRigOffset) && joints.HasFlag(FirstPersonRigJoint.InRigOffset))
                this.tInRigOffsetJoint.localPosition
                    = state.InRigOffsetSmoothed(this.tInRigOffsetJoint.localPosition, smoothTime);
            if (state.IsChanged(FirstPersonRigJoint.Rotation) && joints.HasFlag(FirstPersonRigJoint.Rotation))
                this.tRotationJoint.localRotation
                    = state.RotationSmoothed(this.tRotationJoint.localRotation, smoothTime);
        }
        public override TRigState FromRig() {
            TRigState rigState = new();
            rigState.HorizontalPosition = this.tHorizontalPositioningJoint.position.XZ();
            rigState.InRigOffset = this.tInRigOffsetJoint.localPosition;
            rigState.Rotation = this.tRotationJoint.localRotation;
            return rigState;
        }
        public override uint ResolvedMask(TRigState state) {
            uint mask = base.ResolvedMask(state);
            if ((this.tHorizontalPositioningJoint.position.XZ() - state.HorizontalPosition).sqrMagnitude < RigUtilities.epsilonSqr)
                mask |= FirstPersonRigJoint.HorizontalPosition.ToUInt32();
            if ((this.tInRigOffsetJoint.localPosition - state.InRigOffset).sqrMagnitude < RigUtilities.epsilonSqr)
                mask |= FirstPersonRigJoint.InRigOffset.ToUInt32();
            if (state.Rotation.eulerAngles != this.tRotationJoint.localRotation.eulerAngles)
                mask |= FirstPersonRigJoint.Rotation.ToUInt32();
            return mask;
        }
    }
    [Serializable]
    public class FirstPersonRigState : RigState<FirstPersonRigJoint> {
        [SerializeField] private RigStateVector2Value horizontalPosition;
        [SerializeField] private RigStateVector3Value inRigOffset;
        [SerializeField] private RigStateQuaternionValue rotation;
        public new const uint allJoints = (uint)
            (FirstPersonRigJoint.HorizontalPosition
            | FirstPersonRigJoint.InRigOffset
            | FirstPersonRigJoint.Rotation
            | FirstPersonRigJoint.Camera);
        public FirstPersonRigState() {
            this.horizontalPosition = new (
                FirstPersonRigJoint.HorizontalPosition.ToUInt32(),
                this.MarkChanged
            );
            this.inRigOffset = new (
                FirstPersonRigJoint.InRigOffset.ToUInt32(),
                this.MarkChanged
            );
            this.rotation = new (
                FirstPersonRigJoint.Rotation.ToUInt32(),
                this.MarkChanged
            );
        }

        public Vector2 HorizontalPosition {
            get => this.horizontalPosition.Value;
            set => this.horizontalPosition.Value = value;
        }
        public Vector2 HorizontalPositionSmoothed(Vector2 current, float smoothTime)
            => this.horizontalPosition.ValueSmoothed(current, smoothTime);
        public Vector3 InRigOffset {
            get => this.inRigOffset.Value;
            set => this.inRigOffset.Value = value;
        }
        public Vector3 InRigOffsetSmoothed(Vector3 current, float smoothTime)
            => this.inRigOffset.ValueSmoothed(current, smoothTime);
        public Quaternion Rotation {
            get => this.rotation.Value;
            set => this.rotation.Value = value;
        }
        public Quaternion RotationSmoothed(Quaternion current, float smoothTime)
            => this.rotation.ValueSmoothed(current, smoothTime);
    }
}
