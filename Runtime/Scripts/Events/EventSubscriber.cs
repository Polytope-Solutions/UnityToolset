using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.Events;
using PolytopeSolutions.Toolset.GlobalTools.Generic;

namespace PolytopeSolutions.Toolset.Events {
    public class EventSubscriber : MonoBehaviour {
        [System.Serializable]
        public struct EventSubscription {
            public string eventName;
            public UnityEvent callback;

            public void Invokation() {
                try {
                    this.callback?.Invoke();
                }
                catch (Exception exception) {
                    this.LogError($"Trying to invoke a callback for {eventName}, exception: {exception.Message}");
                }
            }
        }
        [SerializeField] private EventSubscription[] eventSusbscriptions;

        protected virtual void Start() {
            foreach (EventSubscription subscription in this.eventSusbscriptions)
                EventManager.Instance.RegisterEvenetCallback(subscription.eventName, subscription.Invokation);
        }
        protected virtual void OnDestroy() {
            if (EventManager.Instance)
                foreach (EventSubscription subscription in this.eventSusbscriptions)
                    EventManager.Instance.UnregisterEvenetCallback(subscription.eventName, subscription.Invokation);
        }
    }
}
