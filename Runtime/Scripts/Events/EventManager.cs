using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Events {
    public class EventManager : TManager<EventManager> {
        public delegate void onEventInvoked();
        private Dictionary<string, onEventInvoked> events;

        public void InvokeEvent(string eventName) { 
            if (this.events.ContainsKey(eventName))
                this.events[eventName]?.Invoke();
        }

        private void OnEventInvoked() { 
            
        }
        public void RegisterEvenetCallback(string eventName, onEventInvoked callback) {
            if (!this.events.ContainsKey(eventName)) {
                this.events.Add(eventName, OnEventInvoked);
            }
            this.events[eventName] += callback;
        }
        public void UnregisterEvenetCallback(string eventName, onEventInvoked callback) {
            if (this.events.ContainsKey(eventName)) {
                this.events[eventName] -= callback;
            }
        }
    }
}