using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using PolytopeSolutions.Toolset.GlobalTools.Types;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Events {
    public class EventManager : TManager<EventManager> {
        public delegate void onEventInvoked();
        private Dictionary<string, SafeEvent> events = new();
        private Queue<string> pendingEvents = new();

        private void OnError(Delegate handler, Exception ex) {
            this.LogError($"Error invoking event handler {handler.Method.Name}: {ex.Message}");
        }
        public void Update() {
            if (this.pendingEvents.Count > 0) {
                InvokeEvent(this.pendingEvents.Dequeue());
            }
        }
        public void InvokeEvent(string eventName) {
            if (this.events.ContainsKey(eventName)){
                try {
                    this.events[eventName]?.Invoke(OnError);
                }
                catch (Exception exception) {
                    this.LogError($"Failed to invoke {eventName}: {exception.Message}");
                }
            }
        }
        public void ScheduleInvokeEvent(string eventName) 
        => this.pendingEvents.Enqueue(eventName);

        public void RegisterEvenetCallback(string eventName, Action callback) {
            if (!this.events.ContainsKey(eventName)) {
                this.events.Add(eventName, new SafeEvent());
            }
            this.events[eventName].Subscribe(callback);
        }
        public void UnregisterEvenetCallback(string eventName, Action callback) {
            if (this.events.ContainsKey(eventName)) {
                this.events[eventName].Unsubscribe(callback);
            }
        }
        public void ForceCleanEvent(string eventName) {
            if (this.events.ContainsKey(eventName)) {
                this.events.Remove(eventName);
            }
        }
    }
}