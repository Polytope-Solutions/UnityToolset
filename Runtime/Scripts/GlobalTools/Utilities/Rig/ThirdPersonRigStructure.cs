using System;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using static PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig.RigUtilities;
using static PolytopeSolutions.Toolset.GlobalTools.Types.EnumFlags;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig {
    public static class ThirdPersonRigJointUtility  {
        public const uint Camera                = RigJointUtility.Camera;
        public const uint HorizontalPosition    = 1 << 1;
        public const uint HorizontalRotation    = 1 << 2;
        public const uint VerticalRotation      = 1 << 3;
        public const uint Distance              = 1 << 4;
    }

    [Flags]
    public enum ThirdPersonRigJoint : uint {
        Camera              = ThirdPersonRigJointUtility.Camera,
        HorizontalPosition  = ThirdPersonRigJointUtility.HorizontalPosition,
        HorizontalRotation  = ThirdPersonRigJointUtility.HorizontalRotation,
        VerticalRotation    = ThirdPersonRigJointUtility.VerticalRotation,
        Distance            = ThirdPersonRigJointUtility.Distance,
    }
    public interface IThirdPersonRigJointCollection<TRigJoint>
            where TRigJoint : Enum {
        public TRigJoint HorizontalPositionJoint { get;}
        public TRigJoint HorizontalRotationJoint { get; }
        public TRigJoint VerticalRotationJoint { get; }
        public TRigJoint DistanceJoint { get; }
    }
    [Serializable]
    public class DefaultThirdPersonRig
            : ThirdPersonRig<DefaultThirdPersonRigState, ThirdPersonRigJoint> {
        public override ThirdPersonRigJoint CameraJoint 
            => ThirdPersonRigJoint.Camera;
        public override ThirdPersonRigJoint HorizontalPositionJoint 
            => ThirdPersonRigJoint.HorizontalPosition;
        public override ThirdPersonRigJoint HorizontalRotationJoint 
            => ThirdPersonRigJoint.HorizontalRotation;
        public override ThirdPersonRigJoint VerticalRotationJoint 
            => ThirdPersonRigJoint.VerticalRotation;
        public override ThirdPersonRigJoint DistanceJoint 
            => ThirdPersonRigJoint.Distance;
        public override ThirdPersonRigJoint AllRelevantJoints 
            => (
                ThirdPersonRigJoint.Camera
                | ThirdPersonRigJoint.HorizontalPosition
                | ThirdPersonRigJoint.HorizontalRotation
                | ThirdPersonRigJoint.VerticalRotation
                | ThirdPersonRigJoint.Distance
            );
    }
    [Serializable]
    public abstract class ThirdPersonRig<TRigState, TRigJoint> : 
            Rig<TRigState, TRigJoint>, IThirdPersonRigJointCollection<TRigJoint> 
            where TRigState : ThirdPersonRigState<TRigJoint>, new() 
            where TRigJoint : Enum {
        [SerializeField] private Transform tHorizontalPositioningJoint;
        [SerializeField] private Transform tHorizontalRotationJoint;
        [SerializeField] private Transform tVerticalRotationJoint;
        [SerializeField] private Transform tDistanceControlJoint;

        public Transform THorizontalPositioningJoint => this.tHorizontalPositioningJoint;
        public Transform THorizontalRotationJoint => this.tHorizontalRotationJoint;
        public Transform TVerticalRotationJoint => this.tVerticalRotationJoint;
        public Transform TDistanceControlJoint => this.tDistanceControlJoint;

        public abstract TRigJoint HorizontalPositionJoint { get;}
        public abstract TRigJoint HorizontalRotationJoint { get; }
        public abstract TRigJoint VerticalRotationJoint { get; }
        public abstract TRigJoint DistanceJoint { get; }

        public override void ApplyRigStateImmediateCore(ref TRigState state, 
                TRigJoint updateState) {
            base.ApplyRigStateImmediateCore(ref state, updateState);
            bool horizontalPositionChanged = IsChangedAndRelevant(state, this.HorizontalPositionJoint, updateState),
                horizontalRotationChanged = IsChangedAndRelevant(state, this.HorizontalRotationJoint, updateState),
                verticalRotationChanged = IsChangedAndRelevant(state, this.VerticalRotationJoint, updateState),
                distanceChanged = IsChangedAndRelevant(state, this.DistanceJoint, updateState);
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
        public override void ApplyRigStateSmoothedCore(ref TRigState state, float smoothTime, 
                TRigJoint updateState) {
            base.ApplyRigStateSmoothedCore(ref state, smoothTime, updateState);
            bool horizontalPositionChanged = IsChangedAndRelevant(state, this.HorizontalPositionJoint, updateState),
                horizontalRotationChanged = IsChangedAndRelevant(state, this.HorizontalRotationJoint, updateState),
                verticalRotationChanged = IsChangedAndRelevant(state, this.VerticalRotationJoint, updateState),
                distanceChanged = IsChangedAndRelevant(state, this.DistanceJoint, updateState);
            if (horizontalPositionChanged)
                this.tHorizontalPositioningJoint.position
                    = state.HorizontalPositionSmoothed(this.tHorizontalPositioningJoint.position.XZ(), smoothTime).ToXZ();
            if (horizontalRotationChanged)
                this.tHorizontalRotationJoint.rotation
                    = Quaternion.Euler(Vector3.up * state.HorizontalAngleSmoothed(this.tHorizontalRotationJoint.eulerAngles.y, smoothTime));
            if (verticalRotationChanged)
                this.tVerticalRotationJoint.localRotation
                    = Quaternion.Euler(Vector3.right * state.VerticalAngleSmoothed(this.tVerticalRotationJoint.eulerAngles.x, smoothTime));
            if (distanceChanged)
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
        public override TRigJoint ResolvedMask(TRigState state) {
            TRigJoint mask = base.ResolvedMask(state);
            if ((this.tHorizontalPositioningJoint.position.XZ() - state.HorizontalPosition).sqrMagnitude < RigUtilities.epsilonSqr)
                mask = mask.Set(this.HorizontalPositionJoint);
            if (Mathf.Abs(Mathf.DeltaAngle(this.tHorizontalRotationJoint.eulerAngles.y, state.HorizontalAngle)) < RigUtilities.epsilon)
                mask = mask.Set(this.HorizontalRotationJoint);
            if (Mathf.Abs(Mathf.DeltaAngle(this.tVerticalRotationJoint.eulerAngles.x, state.VerticalAngle)) < RigUtilities.epsilon)
                mask = mask.Set(this.VerticalRotationJoint);
            if (Mathf.Abs(Mathf.Abs(this.tDistanceControlJoint.localPosition.z) - state.Depth) < RigUtilities.epsilon)
                mask = mask.Set(this.DistanceJoint);
            return mask;
        }
    }
    [Serializable]
    public class DefaultThirdPersonRigState
            : ThirdPersonRigState<ThirdPersonRigJoint> {
        public override ThirdPersonRigJoint CameraJoint => ThirdPersonRigJoint.Camera;
        public override ThirdPersonRigJoint HorizontalPositionJoint => ThirdPersonRigJoint.HorizontalPosition;
        public override ThirdPersonRigJoint HorizontalRotationJoint => ThirdPersonRigJoint.HorizontalRotation;
        public override ThirdPersonRigJoint VerticalRotationJoint => ThirdPersonRigJoint.VerticalRotation;
        public override ThirdPersonRigJoint DistanceJoint => ThirdPersonRigJoint.Distance;
        public override ThirdPersonRigJoint AllRelevantJoints => 
            (
                ThirdPersonRigJoint.Camera
                | ThirdPersonRigJoint.HorizontalPosition
                | ThirdPersonRigJoint.HorizontalRotation
                | ThirdPersonRigJoint.VerticalRotation
                | ThirdPersonRigJoint.Distance
            );
    }
    [Serializable]
    public abstract class ThirdPersonRigState<TRigJoint> 
            : RigState<TRigJoint>, IThirdPersonRigJointCollection<TRigJoint> 
            where TRigJoint : Enum {
        [SerializeField] private RigStateVector2Value<TRigJoint> horizontalPosition;
        [SerializeField] private RigStateAngleValue<TRigJoint> horizontalAngle;
        [SerializeField] private RigStateAngleValue<TRigJoint> verticalAngle;
        [SerializeField] private RigStateFloatValue<TRigJoint> depth;

        public abstract TRigJoint HorizontalPositionJoint { get; }
        public abstract TRigJoint HorizontalRotationJoint { get; }
        public abstract TRigJoint VerticalRotationJoint { get; }
        public abstract TRigJoint DistanceJoint { get; }

        public ThirdPersonRigState() {
            this.horizontalPosition = new(
                this.HorizontalPositionJoint,
                this.MarkChanged
            );
            this.horizontalAngle = new(
                this.HorizontalRotationJoint,
                this.MarkChanged
            );

            this.verticalAngle = new(
                this.VerticalRotationJoint,
                this.MarkChanged
            );
            this.depth = new(
                this.DistanceJoint,
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
