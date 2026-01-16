using System;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities.Rig {
    public static class RigUtilities {
        public const float epsilon = 0.0001f;
        public const float epsilonSqr = epsilon * epsilon;

        public static uint ToUInt32(this Enum joint)
            => Convert.ToUInt32(joint);
    }
    [Serializable]
    public abstract class Rig<TRigState, TRigJoint>
            where TRigState : RigState<TRigJoint>, new()
            where TRigJoint : Enum {
        [SerializeField] protected Transform tCamera;
        protected Camera camera;
        public Transform TCamera => this.tCamera;
        public Camera Camera {
            get {
                if (this.camera == null)
                    this.camera = this.tCamera.GetComponent<Camera>();
                return this.camera;
            }
        }
        public abstract uint AllJoints { get; }
        protected bool IsChangedAndRelevant(TRigState state, TRigJoint joint, TRigJoint mask)
            => state.IsChanged(joint) && mask.HasFlag(joint);

        public virtual void ApplyRigStateImmediate(ref TRigState state, uint updateState) {
            state.Constrain();
        }
        public virtual void ApplyRigStateSmoothed(ref TRigState state, float smoothTime, uint updateState) {
            state.Constrain();
        }
        public abstract TRigState FromRig();

        public virtual bool IsStateApplied(TRigState state) {
            return (ResolvedMask(state) & this.AllJoints) == this.AllJoints;
        }
        public virtual uint ResolvedMask(TRigState state)
            => 0;
    }
    [Serializable]
    public abstract class RigState<TRigJoint>
            where TRigJoint : Enum {
        protected uint changeMask;
        public const uint allJoints = 0;

        protected void MarkChanged(uint joint) {
            this.changeMask |= joint;
        }
        public bool IsChanged(TRigJoint joint)
            => IsChanged(joint.ToUInt32());
        public bool IsChanged(uint testMask) => (this.changeMask & testMask) == testMask;
        public bool IsAnyChanged => this.changeMask != 0;
        public virtual void Constrain() {

        }
        public virtual void ClearMask(uint acknowledgedChanges = uint.MaxValue) {
            this.changeMask &= ~acknowledgedChanges;
        }
    }
    [Serializable]
    public abstract class TRigStateValue<TValue> {
        protected TValue value;
        protected TValue velocity;
        private readonly uint jointValue;
        private Action<uint> markChangedAction;

        public TRigStateValue(uint jointValue, Action<uint> markChangedAction) {
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
        public uint JointValue => this.jointValue;
        public abstract bool TolleranceCondition(TValue newValue);
        public abstract TValue ValueSmoothed(TValue currentValue, float smoothTime);
    }
    [Serializable]
    public class RigStateFloatValue : TRigStateValue<float> {
        public RigStateFloatValue(uint jointValue, Action<uint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(float newValue)
            => Mathf.Abs(newValue - Value) < RigUtilities.epsilon;
        public override float ValueSmoothed(float currentValue, float smoothTime)
            => Mathf.SmoothDamp(currentValue, Value, ref base.velocity, smoothTime);
    }
    [Serializable]
    public class RigStateAngleValue : TRigStateValue<float> {
        public RigStateAngleValue(uint jointValue, Action<uint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(float newValue)
            => Mathf.Abs(Mathf.DeltaAngle(newValue, Value)) < RigUtilities.epsilon;
        public override float ValueSmoothed(float currentValue, float smoothTime)
            => Mathf.SmoothDampAngle(currentValue, Value, ref base.velocity, smoothTime);
    }
    [Serializable]
    public class RigStateVector2Value : TRigStateValue<Vector2> {
        public RigStateVector2Value(uint jointValue, Action<uint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(Vector2 newValue)
            => (newValue - Value).sqrMagnitude < RigUtilities.epsilonSqr;
        public override Vector2 ValueSmoothed(Vector2 currentValue, float smoothTime)
            => Vector2.SmoothDamp(currentValue, Value, ref base.velocity, smoothTime);
    }
    [Serializable]
    public class RigStateVector3Value : TRigStateValue<Vector3> {
        public RigStateVector3Value(uint jointValue, Action<uint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(Vector3 newValue)
            => (newValue - Value).sqrMagnitude < RigUtilities.epsilonSqr;
        public override Vector3 ValueSmoothed(Vector3 currentValue, float smoothTime)
            => Vector3.SmoothDamp(currentValue, Value, ref base.velocity, smoothTime);
    }
    [Serializable]
    public class RigStateQuaternionValue : TRigStateValue<Quaternion> {
        public RigStateQuaternionValue(uint jointValue, Action<uint> markChangedAction)
            : base(jointValue, markChangedAction) {
        }
        public override bool TolleranceCondition(Quaternion newValue)
            => Quaternion.Angle(newValue, Value) < RigUtilities.epsilon;
        public override Quaternion ValueSmoothed(Quaternion currentValue, float smoothTime)
            => Quaternion.Slerp(currentValue, this.value, smoothTime);
    }
}