using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public abstract class OptionGenerator<TKey, TValue> : MonoBehaviour {
        [SerializeField] protected GameObject goOptionPrefab;
        [SerializeField] protected List<OptionOverrideInfo> overrideOptions;

        [System.Serializable]
        public class OptionOverrideInfo {
            public TKey key;
            public GameObject goOverride;
        }

        protected abstract bool AreDependenciesReady { get; }
        protected abstract TValue GetValue(GameObject goItem);
        protected abstract void CleanUpValue(TValue value);
        protected abstract IEnumerable<TKey> options { get; }
        protected Dictionary<TKey, TValue> optionObjects = new Dictionary<TKey, TValue>();

        protected virtual void Start() {
            InitializeAllOptions();
        }

        protected virtual void InitializeAllOptions() {
            if (!this.AreDependenciesReady) {
                Debug.LogWarning("Dependencies not ready: " + this.name);
            }
            HashSet<TKey> currentOptions = new HashSet<TKey>(this.optionObjects.Keys);
            foreach(TKey option in this.options) {
                if (!this.optionObjects.ContainsKey(option)) {
                    OnKeyAdded(option);
                }
                currentOptions.Remove(option);
            }
            if (currentOptions.Count > 0) {
                // Remove unused options
                foreach (TKey option in currentOptions) {
                    OnKeyRemoved(option);
                }
            }
        }

        protected virtual void OnKeyAdded(TKey key) {
            if (this.optionObjects.ContainsKey(key)) {
                UpdateOption(key);
                return;
            }
            int i = this.options.ToList().IndexOf(key);
            GameObject goPrefab = (this.overrideOptions.Any(item => Equals(item.key, key))) 
                ? this.overrideOptions.First(item => Equals(item.key, key)).goOverride 
                : this.goOptionPrefab;
            GameObject goItem = Instantiate(goPrefab, transform);
            TValue value = GetValue(goItem);
            this.optionObjects.Add(key, value);
            InitializeOption(i, key, value);
        }
        protected virtual void OnKeyRemoved(TKey key) {
            if (!this.optionObjects.ContainsKey(key)) return;
            CleanUpOption(key, this.optionObjects[key]);
        }

        protected abstract void InitializeOption(int i, TKey option, TValue value);
        protected virtual void UpdateOption(TKey option) { }
        protected virtual void CleanUpOption(TKey option, TValue value) {
            this.optionObjects.Remove(option);
            CleanUpValue(value);
        }
    }
}