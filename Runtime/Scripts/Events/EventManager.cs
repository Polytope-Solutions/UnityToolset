using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using PolytopeSolutions.Toolset.GlobalTools.Types;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Events {
    public class EventManager : TManager<EventManager> {
        public delegate void onEventInvoked();
        private Dictionary<string, Action> events = new();
        private Queue<string> pendingEvents = new();

        public void Update() {
            if (this.pendingEvents.Count > 0) {
                InvokeEvent(this.pendingEvents.Dequeue());
            }
        }
        public void InvokeEvent(string eventName) { 
            if (this.events.ContainsKey(eventName)){
                try {
                    this.events[eventName]?.Invoke();
                }
                catch (Exception exception) { 
                    this.LogError($"Failed to invoke {eventName}: {exception.Message}");
                }
            }
        }
        public void ScheduleInvokeEvent(string eventName) => this.pendingEvents.Enqueue(eventName);

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