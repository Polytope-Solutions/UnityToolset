//#define DEBUG
#undef DEBUG
#define DEBUG2
//#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.GlobalTools.Types {
    public abstract class KeyValueDebouncer<TKey, TValue> : MonoBehaviour {
        [Range(0.01f, 1f)]
        [SerializeField] private float debounceSeconds = 0.3f;

        private readonly Dictionary<TKey, TValue> pendingPayloads = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, Coroutine> activeTimers = new Dictionary<TKey, Coroutine>();

        private WaitForSecondsRealtime debounceAwaiter;

        #region UNITY_FUNCTIONS
        protected virtual void Awake() {
            this.debounceAwaiter = new WaitForSecondsRealtime(this.debounceSeconds);
        }
        protected virtual void OnDestroy() {
            ClearAll();
        }
        #endregion
        #region  ACCESSORS
        public virtual void QueueData(TKey key, TValue value) {
            // skip if same key value is registered.
            if (this.pendingPayloads.ContainsKey(key) && EqualityComparer<TValue>.Default.Equals(this.pendingPayloads[key], value)) {
                return;
            }
            // Overwrite any existing pending value for this key (latest always wins)
            this.pendingPayloads[key] = value;

            // Cancel previous timer for this key if it exists
            if (this.activeTimers.TryGetValue(key, out Coroutine oldTimer)) {
                StopCoroutine(oldTimer);
                this.activeTimers.Remove(key);
            }

            // Start a fresh debounce timer for this key
            this.activeTimers[key] = StartCoroutine(DebounceCoroutine(key));
        }
        public void ClearAll() {
            foreach (Coroutine timer in this.activeTimers.Values)
                StopCoroutine(timer);

            this.activeTimers.Clear();
            this.pendingPayloads.Clear();
        }
        #endregion
        #region INTERNAL_FUNCTIONS
        private IEnumerator DebounceCoroutine(TKey key) {
            yield return this.debounceAwaiter;

            // Timer expired - process the (latest) pending key
            if (this.pendingPayloads.TryGetValue(key, out TValue value)) {
                ProcessMessage(key, value);
                this.pendingPayloads.Remove(key);
            }

            this.activeTimers.Remove(key);
        }
        protected abstract void ProcessMessage(TKey key, TValue value);
        #endregion
    }
}
