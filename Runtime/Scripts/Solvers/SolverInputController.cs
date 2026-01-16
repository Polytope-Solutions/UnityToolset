// #define DEBUG
#undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

using static PolytopeSolutions.Toolset.GlobalTools.Generic.ObjectHelpers;

namespace PolytopeSolutions.Toolset.Solvers {
    [System.Serializable]
    public class SolverInputController {
        private string owner;
        private Dictionary<string, IInputEntry> states = new Dictionary<string, IInputEntry>();
        public interface IInputEntry {
            public object CurrentValue { get; }
            public bool IsInitialized { get; }
            public bool IsTargetReached { get; }
            public bool IsValueChanged { get; }
            public bool Tick();
            public void Update(object newTargetValue);
        }
        public abstract class InputEntry<T> : IInputEntry {
            private float smoothDuration;

            public InputEntry(float _smoothDuration) {
                this.smoothDuration = Mathf.Clamp(_smoothDuration, 0f, float.MaxValue);
                this.isInitialized = false;
            }

            public object CurrentValue => (object)this.currentValue;

            protected T previousValue;
            protected T currentValue;
            protected T previousTargetValue;
            protected T targetValue;

            protected float progress = 0f;
            private DateTime lastTick;


            protected static float precision = 0.0001f;

            private bool isInitialized;
            public bool IsInitialized => this.isInitialized;
            public virtual bool IsValueChanged => !this.previousValue.Equals(this.currentValue);

            public virtual bool IsTargetReached => true;

            protected abstract T CopyValue(T value);
            public bool Tick() {
                T temp = CopyValue(this.currentValue);
                if (this.previousValue != null && this.IsTargetReached) {
                    this.previousValue = temp;
                    return true;
                }
                if (this.smoothDuration == 0f)
                    this.progress = 1f;
                else
                    this.progress += (float)(DateTime.Now - this.lastTick).TotalSeconds / this.smoothDuration;
                this.progress = Mathf.Clamp01(this.progress);
                CurrentValueTick();
                this.lastTick = DateTime.Now;
                this.previousValue = temp;
                return false;
            }
            protected abstract void CurrentValueTick();
            public void Update(object newTargetValue) {
                T newValue = (T)newTargetValue;
                if (!this.isInitialized) {
                    this.isInitialized = true;
                    this.currentValue = CopyValue(newValue);
                    this.targetValue = CopyValue(newValue);
                }
                this.previousTargetValue = CopyValue(this.targetValue);
                this.targetValue = CopyValue(newValue);
                if (!this.IsTargetReached) {
                    this.progress = 0f;
                    this.lastTick = DateTime.Now;
                }
            }
        }
        public class FloatInputEntry : InputEntry<float>{
            public FloatInputEntry(float _smoothDuration) : base(_smoothDuration) { }

            public override bool IsTargetReached => Mathf.Abs(this.currentValue - this.targetValue) < precision;

            protected override float CopyValue(float value) {
                return value;
            }
            protected override void CurrentValueTick() {
                this.currentValue = Mathf.Lerp(this.previousTargetValue, this.targetValue, this.progress);
            }
        }
        public class Vector2InputEntry : InputEntry<Vector2>{
            public Vector2InputEntry(float _smoothDuration) : base(_smoothDuration) { }

            public override bool IsTargetReached => Mathf.Abs((this.currentValue - this.targetValue).sqrMagnitude) < precision;
            public override bool IsValueChanged => Mathf.Abs((this.currentValue - this.previousValue).sqrMagnitude) > precision;
            protected override Vector2 CopyValue(Vector2 value) {
                return new Vector2(value.x, value.y);
            }
            protected override void CurrentValueTick() {
                this.currentValue = Vector2.Lerp(this.previousTargetValue, this.targetValue, this.progress);
            }
        }
        public class Vector3ArrayInputEntry : InputEntry<Vector3[]> {
            public Vector3ArrayInputEntry(float _smoothDuration) : base(_smoothDuration) { }

            protected override Vector3[] CopyValue(Vector3[] value) {
                Vector3[] newValue = new Vector3[value.Length];
                for(int i = 0; i < value.Length; i++)
                    newValue[i] = new Vector3(value[i].x, value[i].y, value[i].z);
                return newValue;
            }
            public override bool IsTargetReached {
                get {
                    bool reached = (this.currentValue.Length == this.targetValue.Length);
                    for (int i = 0; i < this.currentValue.Length && reached; i++)
                        reached &= ((this.currentValue[i] - this.targetValue[i]).sqrMagnitude < precision);
                    return reached;
                }
            }
            public override bool IsValueChanged {
                get {
                    bool changed = this.previousValue == null
                        || (this.currentValue.Length != this.previousValue.Length);
                    for (int i = 0; i < this.currentValue.Length && !changed; i++)
                        changed |= ((this.currentValue[i] - this.targetValue[i]).sqrMagnitude > precision);
                    return changed;
                }
            }

            protected override void CurrentValueTick() {
                if (this.currentValue.Length != this.targetValue.Length) {
                    Vector3[] temp = CopyValue(this.currentValue);
                    this.currentValue = new Vector3[this.targetValue.Length];
                    for(int i = 0; i < Mathf.Min(temp.Length, this.currentValue.Length); i++)
                        this.currentValue[i] = temp[i];
                }

                for (int i = 0; i < this.currentValue.Length; i++)
                    this.currentValue[i] = Vector3.Lerp(this.previousTargetValue[i], this.targetValue[i], this.progress);
            }
        }

        public bool IsInitialized => this.states != null;
        public bool IsComplete {
            get {
                #if DEBUG
                this.Log($"[{this.owner}] IsComplete check");
                #endif
                bool complete = true;
                this.states.Values.ToList().ForEach(states => complete &= states.IsInitialized);
                return complete;
            }
        }

        public SolverInputController(string _owner, List<string> keys, List<IInputEntry> entries) {
            this.owner = _owner;
            this.states = new Dictionary<string, IInputEntry>();
            for (int i = 0; i < keys.Count; i++)
                this.states.Add(keys[i], entries[i]);
            #if DEBUG
            this.Log($"[{this.owner}] created with: {keys.Count}");
            #endif
        }
        public void Update(string key, object targetValue) {
            if (this.states == null || !this.states.ContainsKey(key)) {
                this.LogError($"[{this.owner}] updating: key [{key}] was not added on initialization.");
                return;
            }
            #if DEBUG
            this.Log($"[{this.owner}] updating: key [{key}]");
            #endif
            this.states[key].Update(targetValue);
        }
        public bool IsTargetReached(string key) => this.states[key].IsTargetReached;
        public bool IsValueChanged(string key) => this.states[key].IsValueChanged;
        public bool IsAnyValueChanged => this.states.Any(item => item.Value.IsValueChanged);
        public object this[string key] {
            get {
                if (this.states == null || !this.states.ContainsKey(key)) {
                    this.LogError($"[{this.owner}] accessing: key [" + key + "] was not added on initialization.");
                    return null;
                }
                #if DEBUG
                this.Log($"[{this.owner}] accessing: key [{key}]");
                #endif
                return (object)this.states[key].CurrentValue;
            }
        }
        public bool TickInputs() {
            bool stable = true;
            foreach (KeyValuePair<string, IInputEntry> state in this.states) {
                stable &= state.Value.Tick();
            }
            if (!stable) {
                #if DEBUG
                this.Log($"[{this.owner}] updating");
                #endif
            }
            return stable;
        }
    }
}
