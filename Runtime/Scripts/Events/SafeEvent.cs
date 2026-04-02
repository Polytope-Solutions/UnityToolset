using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using PolytopeSolutions.Toolset.GlobalTools.Types;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Events {
    public class SafeEvent {
        private readonly List<Action> listeners = new(), dead = new();

        public void Subscribe(Action listener)
            => this.listeners.Add(listener);

        public void Unsubscribe(Action listener) 
            => this.listeners.Remove(listener);

        public void Invoke(Action<Action, Exception> onError = null, bool autoremoveOnException = true) {
            this.dead.Clear();

            foreach (Action listener in this.listeners) {
                // Detect destroyed Unity objects
                if (listener.Target is UnityEngine.Object obj && obj == null) {
                    this.dead.Add(listener);
                    continue;
                }

                try { 
                    listener.Invoke();
                }
                catch (Exception ex) {
                    onError?.Invoke(listener, ex);
                    if (autoremoveOnException)
                        this.dead.Add(listener);
                }
            }
            // Prune invalid listeners
            foreach (Action d in this.dead) 
                this.listeners.Remove(d);
        }
    }
    public class SafeEvent<T> {
        private readonly List<Action<T>> listeners = new(), dead = new();

        public void Subscribe(Action<T> listener) 
            => this.listeners.Add(listener);

        public void Unsubscribe(Action<T> listener) 
            => this.listeners.Remove(listener);

        public void Invoke(T arg, Action<Action<T>, Exception> onError = null, bool autoremoveOnException = true) {
            this.dead.Clear();

            foreach (Action<T> listener in this.listeners) {
                // Detect destroyed Unity objects
                if (listener.Target is UnityEngine.Object obj && obj == null) {
                    this.dead.Add(listener);
                    continue;
                }

                try { 
                    listener.Invoke(arg);
                }
                catch (Exception ex) {
                    onError?.Invoke(listener, ex);
                    if (autoremoveOnException)
                        this.dead.Add(listener);
                }
            }
            // Prune invalid listeners
            foreach (Action<T> d in this.dead) 
                this.listeners.Remove(d);
        }
    }
}