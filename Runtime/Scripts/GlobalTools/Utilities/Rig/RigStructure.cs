using System;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using static PolytopeSolutions.Toolset.GlobalTools.Types.EnumFlags;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig {

    public static class RigJointUtility {
        public const uint None   = 0;
        public const uint Camera    = 1 << 0;
    }
    [Flags]
    public enum RigJoint : uint {
        None                        = RigJointUtility.None,
        Camera                      = RigJointUtility.Camera,
    }
    public static class RigUtilities {
        public const float epsilon = 0.0001f;
        public const float epsilonSqr = epsilon * epsilon;
    }
    public interface IRigJointCollection<TRigJoint>
            where TRigJoint : Enum {
        public TRigJoint CameraJoint { get; }
        public TRigJoint NoJoints { get; }
        public TRigJoint AllRelevantJoints { get; }
    }
    [Serializable]
    public abstract class Rig<TRigState, TRigJoint> 
            : IRigJointCollection<TRigJoint>
            where TRigState : RigState<TRigJoint>, new()
            where TRigJoint : Enum {
        [SerializeField] protected Transform tCamera;
        public abstract TRigJoint CameraJoint { get; }
        public abstract TRigJoint AllRelevantJoints { get; }
        public abstract TRigJoint NoJoints { get; }

        public Transform TCamera => this.tCamera;

        protected Camera camera;
        public Camera Camera {
            get {
                if (this.camera == null)
                    this.camera = this.tCamera.GetComponent<Camera>();
                return this.camera;
            }
        }
        protected bool IsChangedAndRelevant(TRigState state, TRigJoint joint, TRigJoint mask)
            => state.IsChanged(joint) && mask.HasFlag(joint);

        public void ApplyRigStateImmediate(ref TRigState state)
            => ApplyRigStateImmediate(ref state, this.AllRelevantJoints);
        public void ApplyRigStateImmediate(ref TRigState state, 
                TRigJoint updateState) 
            => ApplyRigStateImmediateCore(ref state, updateState);
        public virtual void ApplyRigStateImmediateCore(ref TRigState state, 
                TRigJoint updateState) {
            state.Constrain();
        }
        public void ApplyRigStateSmoothed(ref TRigState state, float smoothTime)
            => ApplyRigStateSmoothed(ref state, smoothTime, this.AllRelevantJoints);
        public virtual void ApplyRigStateSmoothed(ref TRigState state, float smoothTime,
                TRigJoint updateState)
            => ApplyRigStateSmoothedCore(ref state, smoothTime, updateState);
        public virtual void ApplyRigStateSmoothedCore(ref TRigState state, float smoothTime,
                TRigJoint updateState) {
            state.Constrain();
        }
        public abstract TRigState FromRig();

        public virtual bool IsStateApplied(TRigState state) {
            return ResolvedMask(state).HasAll(this.AllRelevantJoints);
        }
        public virtual TRigJoint ResolvedMask(TRigState state)
            => this.NoJoints;
    }
    public interface IRigState{}
    [Serializable]
    public abstract class RigState<TRigJoint>
            : IRigJointCollection<TRigJoint>, IRigState
            where TRigJoint : Enum {
        protected TRigJoint changeMask;
        
        public abstract TRigJoint CameraJoint { get; }
        public abstract TRigJoint AllRelevantJoints { get; }
        public abstract TRigJoint NoJoints { get; }

        protected void MarkChanged(TRigJoint joint) {
            this.changeMask = this.changeMask.Set(joint);
        }
        public bool IsChanged(TRigJoint joint)
            => this.changeMask.HasAll(joint);
        public bool IsAnyChanged => this.changeMask.HasAny(this.AllRelevantJoints);
        public virtual void Constrain() {}
        public void ClearMask() 
            => ClearMask(this.AllRelevantJoints);
        public virtual void ClearMask(TRigJoint acknowledgedChanges) {
            this.changeMask = this.changeMask.Clear(acknowledgedChanges);
        }
    }
    [Serializable]
    public abstract class TRigStateValue<TValue, TRigJoint> 
            where TRigJoint : Enum {
        protected TValue value;
        protected TValue velocity;
        private readonly TRigJoint jointValue;
        private Action<TRigJoint> markChangedAction;

        public TRigStateValue(TRigJoint jointValue, Action<TRigJoint> markChangedAction) {
            this.jointValue = jointValue;
            this.markChangedAction = markChangedAction;
        }
        public TValue Value {
            get => this.value;
            set {
                if (TolleranceCondition(value))
                    return;
                this.value = value;
                this.markChangedAction?.Invoke(this.jointValue);
            }
        }
        public TRigJoint JointValue => this.jointValue;
        public abstract bool TolleranceCondition(TValue newValue);
        public abstract TValue ValueSmoothed(TValue currentValue, float smoothTime);
    }
    [Serializable]
    public class RigStateFloatValue<TRigJoint> : TRigStateValue<float, TRigJoint> 
            where TRigJoint : Enum {
        public RigStateFloatValue(TRigJoint jointValue, Action<TRigJoint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(float newValue)
            => Mathf.Abs(newValue - Value) < RigUtilities.epsilon;
        public override float ValueSmoothed(float currentValue, float smoothTime)
            => Mathf.SmoothDamp(currentValue, Value, ref base.velocity, smoothTime);
    }
    [Serializable]
    public class RigStateAngleValue<TRigJoint> : TRigStateValue<float, TRigJoint>
            where TRigJoint : Enum {
        public RigStateAngleValue(TRigJoint jointValue, Action<TRigJoint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(float newValue)
            => Mathf.Abs(Mathf.DeltaAngle(newValue, Value)) < RigUtilities.epsilon;
        public override float ValueSmoothed(float currentValue, float smoothTime)
            => Mathf.SmoothDampAngle(currentValue, Value, ref base.velocity, smoothTime);
    }
    [Serializable]
    public class RigStateVector2Value<TRigJoint> : TRigStateValue<Vector2, TRigJoint> 
            where TRigJoint : Enum {
        public RigStateVector2Value(TRigJoint jointValue, Action<TRigJoint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(Vector2 newValue)
            => (newValue - Value).sqrMagnitude < RigUtilities.epsilonSqr;
        public override Vector2 ValueSmoothed(Vector2 currentValue, float smoothTime)
            => Vector2.SmoothDamp(currentValue, Value, ref base.velocity, smoothTime);
    }
    [Serializable]
    public class RigStateVector3Value<TRigJoint> : TRigStateValue<Vector3, TRigJoint> 
            where TRigJoint : Enum {
        public RigStateVector3Value(TRigJoint jointValue, Action<TRigJoint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(Vector3 newValue)
            => (newValue - Value).sqrMagnitude < RigUtilities.epsilonSqr;
        public override Vector3 ValueSmoothed(Vector3 currentValue, float smoothTime)
            => Vector3.SmoothDamp(currentValue, Value, ref base.velocity, smoothTime);
    }
    [Serializable]
    public class RigStateQuaternionValue<TRigJoint> : TRigStateValue<Quaternion, TRigJoint> 
            where TRigJoint : Enum {
        public RigStateQuaternionValue(TRigJoint jointValue, Action<TRigJoint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(Quaternion newValue)
            => Quaternion.Angle(newValue, Value) < RigUtilities.epsilon;
        public override Quaternion ValueSmoothed(Quaternion currentValue, float smoothTime)
            => Quaternion.Slerp(currentValue, this.value, smoothTime);
    }
}