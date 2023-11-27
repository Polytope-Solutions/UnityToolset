using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Events {
    public class EventManager : TManager<EventManager> {
        public delegate void onEventInvoked();
        private Dictionary<string, Action> events = new Dictionary<string, Action>();

        public void InvokeEvent(string eventName) { 
            if (this.events.ContainsKey(eventName))
                this.events[eventName]?.Invoke();
        }

        private void OnEventInvoked() {}
        public void RegisterEvenetCallback(string eventName, Action callback) {
            if (!this.events.ContainsKey(eventName)) {
                this.events.Add(eventName, OnEventInvoked);
            }
            this.events[eventName] += callback;
        }
        public void UnregisterEvenetCallback(string eventName, Action callback) {
            if (this.events.ContainsKey(eventName)) {
                this.events[eventName] -= callback;
            }
        }
    }
}