using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Events {
    public class EventInvoker : MonoBehaviour {
        [SerializeField] private string eventName;
        public void InvokeEvent() {
            EventManager.Instance?.InvokeEvent(eventName);
        }
    }
}