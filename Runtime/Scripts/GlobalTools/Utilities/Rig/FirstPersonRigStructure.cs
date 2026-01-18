using System;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using static PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig.RigUtilities;
using static PolytopeSolutions.Toolset.GlobalTools.Types.EnumFlags;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig {
    public static class FirstPersonRigJointUtility {
        public const uint Camera                = RigJointUtility.Camera;
        public const uint HorizontalPosition    = 1 << 1;
        public const uint Height                = 1 << 2;
        public const uint LocalOffset           = 1 << 3;
        public const uint Rotation              = 1 << 4;
    }
    [Flags]
    public enum FirstPersonRigJoint : uint {
        Camera              = FirstPersonRigJointUtility.Camera,
        HorizontalPosition  = FirstPersonRigJointUtility.HorizontalPosition,
        Height              = FirstPersonRigJointUtility.Height,
        LocalOffset         = FirstPersonRigJointUtility.LocalOffset,
        Rotation            = FirstPersonRigJointUtility.Rotation,
    }
    public interface IFirstPersonRigJointCollection<TRigJoint>
            where TRigJoint : Enum {
        public TRigJoint HorizontalPositionJoint { get;}
        public TRigJoint HeightJoint { get;}
        public TRigJoint LocalOffsetJoint { get; }
        public TRigJoint RotationJoint { get; }
    }

    [Serializable]
    public class DefaultFirstPersonRig 
            : FirstPersonRig<DefaultFirstPersonRigState, FirstPersonRigJoint> {
        public override FirstPersonRigJoint CameraJoint 
            => FirstPersonRigJoint.Camera;
        public override FirstPersonRigJoint HorizontalPositionJoint 
            => FirstPersonRigJoint.HorizontalPosition;
        public override FirstPersonRigJoint HeightJoint
            => FirstPersonRigJoint.Height;
        public override FirstPersonRigJoint LocalOffsetJoint 
            => FirstPersonRigJoint.LocalOffset;
        public override FirstPersonRigJoint RotationJoint 
            => FirstPersonRigJoint.Rotation;
        public override FirstPersonRigJoint AllRelevantJoints 
            => (
                FirstPersonRigJoint.Camera
                | FirstPersonRigJoint.HorizontalPosition
                | FirstPersonRigJoint.Height
                | FirstPersonRigJoint.LocalOffset
                | FirstPersonRigJoint.Rotation
            );
    }
    [Serializable]
    public abstract class FirstPersonRig<TRigState, TRigJoint> 
            : Rig<TRigState, TRigJoint>, IFirstPersonRigJointCollection<TRigJoint>
            where TRigState : FirstPersonRigState<TRigJoint>, new()
            where TRigJoint : Enum {
        [SerializeField] private Transform tHorizontalPositioningJoint;
        [SerializeField] private Transform tHeightJoint;
        [SerializeField] private Transform tLocalOffsetJoint;
        [SerializeField] private Transform tRotationJoint;

        public Transform THorizontalPositioningJoint => this.tHorizontalPositioningJoint;
        public Transform THeightJoint => this.tHeightJoint;
        public Transform TLocalOffsetJoint => this.tLocalOffsetJoint;
        public Transform TRotationJoint => this.tRotationJoint;

        public abstract TRigJoint HorizontalPositionJoint { get;}
        public abstract TRigJoint HeightJoint { get;}
        public abstract TRigJoint LocalOffsetJoint { get; }
        public abstract TRigJoint RotationJoint { get; }

        public override void ApplyRigStateImmediateCore(ref TRigState state, TRigJoint updateState) {
            base.ApplyRigStateImmediateCore(ref state, updateState);
            bool horizontalPositionChanged = IsChangedAndRelevant(state, this.HorizontalPositionJoint, updateState),
                heightChanged = IsChangedAndRelevant(state, this.HeightJoint, updateState),
                localOffsetChanged = IsChangedAndRelevant(state, this.LocalOffsetJoint, updateState),
                rotationChanged = IsChangedAndRelevant(state, this.RotationJoint, updateState);
            if (horizontalPositionChanged)
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPosition.ToXZ();
            if (heightChanged)
                this.tHeightJoint.localPosition
                    = Vector3.up * state.Height;
            if (localOffsetChanged)
                this.tLocalOffsetJoint.localPosition
                    = state.LocalOffset;
            if (rotationChanged)
                this.tRotationJoint.localRotation
                    = state.Rotation;
        }
        public override void ApplyRigStateSmoothedCore(ref TRigState state, float smoothTime, TRigJoint updateState) {
            base.ApplyRigStateSmoothedCore(ref state, smoothTime, updateState);
            bool horizontalPositionChanged = IsChangedAndRelevant(state, this.HorizontalPositionJoint, updateState),
                heightChanged = IsChangedAndRelevant(state, this.HeightJoint, updateState),
                localOffsetChanged = IsChangedAndRelevant(state, this.LocalOffsetJoint, updateState),
                rotationChanged = IsChangedAndRelevant(state, this.RotationJoint, updateState);
            if (horizontalPositionChanged)
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPositionSmoothed(this.tHorizontalPositioningJoint.position.XZ(), smoothTime).ToXZ();
            if (heightChanged)
                this.tHeightJoint.localPosition
                    = Vector3.up * state.HeightSmoothed(this.tHeightJoint.localPosition.y, smoothTime);
            if (localOffsetChanged)
                this.tLocalOffsetJoint.localPosition
                    = state.LocalOffsetSmoothed(this.tLocalOffsetJoint.localPosition, smoothTime);
            if (rotationChanged)
                this.tRotationJoint.localRotation
                    = state.RotationSmoothed(this.tRotationJoint.localRotation, smoothTime);
        }
        public override TRigState FromRig() {
            TRigState rigState = new();
            rigState.HorizontalPosition = this.tHorizontalPositioningJoint.position.XZ();
            rigState.Height = this.tHeightJoint.localPosition.y;
            rigState.LocalOffset = this.tLocalOffsetJoint.localPosition;
            rigState.Rotation = this.tRotationJoint.localRotation;
            return rigState;
        }
        public override TRigJoint ResolvedMask(TRigState state) {
            TRigJoint mask = base.ResolvedMask(state);
            if ((this.tHorizontalPositioningJoint.position.XZ() - state.HorizontalPosition).sqrMagnitude < RigUtilities.epsilonSqr)
                mask = mask.Set(this.HorizontalPositionJoint);
            if (Mathf.Abs(this.tHeightJoint.localPosition.y - state.Height) < RigUtilities.epsilon)
                mask = mask.Set(this.HeightJoint);
            if ((this.tLocalOffsetJoint.localPosition - state.LocalOffset).sqrMagnitude < RigUtilities.epsilonSqr)
                mask = mask.Set(this.LocalOffsetJoint);
            if (state.Rotation.eulerAngles != this.tRotationJoint.localRotation.eulerAngles)
                mask = mask.Set(this.RotationJoint);
            return mask;
        }
    }
    [Serializable]
    public class DefaultFirstPersonRigState
        : FirstPersonRigState<FirstPersonRigJoint> {
        public override FirstPersonRigJoint CameraJoint 
            => FirstPersonRigJoint.Camera;
        public override FirstPersonRigJoint HorizontalPositionJoint 
            => FirstPersonRigJoint.HorizontalPosition;
        public override FirstPersonRigJoint HeightJoint
            => FirstPersonRigJoint.Height;
        public override FirstPersonRigJoint LocalOffsetJoint 
            => FirstPersonRigJoint.LocalOffset;
        public override FirstPersonRigJoint RotationJoint 
            => FirstPersonRigJoint.Rotation;
        public override FirstPersonRigJoint AllRelevantJoints 
            => (
                FirstPersonRigJoint.Camera
                | FirstPersonRigJoint.HorizontalPosition
                | FirstPersonRigJoint.Height
                | FirstPersonRigJoint.LocalOffset
                | FirstPersonRigJoint.Rotation
            );
    }
    [Serializable]
    public abstract class FirstPersonRigState<TRigJoint> 
            : RigState<TRigJoint>, IFirstPersonRigJointCollection<TRigJoint>
            where TRigJoint : Enum {
        [SerializeField] private RigStateVector2Value<TRigJoint> horizontalPosition;
        [SerializeField] private RigStateFloatValue<TRigJoint> height;
        [SerializeField] private RigStateVector3Value<TRigJoint> localOffset;
        [SerializeField] private RigStateQuaternionValue<TRigJoint> rotation;
        
        public abstract TRigJoint HorizontalPositionJoint { get; }
        public abstract TRigJoint HeightJoint { get; }
        public abstract TRigJoint LocalOffsetJoint { get; }
        public abstract TRigJoint RotationJoint { get; }

        public FirstPersonRigState() {
            this.horizontalPosition = new (
                this.HorizontalPositionJoint,
                this.MarkChanged
            );
            this.height = new (
                this.HeightJoint,
                this.MarkChanged
            );
            this.localOffset = new (
                this.LocalOffsetJoint,
                this.MarkChanged
            );
            this.rotation = new (
                this.RotationJoint,
                this.MarkChanged
            );
        }

        public Vector2 HorizontalPosition {
            get => this.horizontalPosition.Value;
            set => this.horizontalPosition.Value = value;
        }
        public Vector2 HorizontalPositionSmoothed(Vector2 current, float smoothTime)
            => this.horizontalPosition.ValueSmoothed(current, smoothTime);
        public float Height {
            get => this.height.Value;
            set => this.height.Value = value;
        }
        public float HeightSmoothed(float current, float smoothTime)
            => this.height.ValueSmoothed(current, smoothTime);
        public Vector3 LocalOffset {
            get => this.localOffset.Value;
            set => this.localOffset.Value = value;
        }
        public Vector3 LocalOffsetSmoothed(Vector3 current, float smoothTime)
            => this.localOffset.ValueSmoothed(current, smoothTime);
        public Quaternion Rotation {
            get => this.rotation.Value;
            set => this.rotation.Value = value;
        }
        public Quaternion RotationSmoothed(Quaternion current, float smoothTime)
            => this.rotation.ValueSmoothed(current, smoothTime);
    }
}
