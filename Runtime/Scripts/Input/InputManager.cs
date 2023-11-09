using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Types;

namespace PolytopeSolutions.Toolset.Input {
    public class InputManager : TManager<InputManager> {
        protected Dictionary<string, InputReceiver> inputReceivers = new Dictionary<string, InputReceiver>();

        public void RegisterInputReceiver(string key, InputReceiver receiver) { 
            if (!this.inputReceivers.ContainsKey(key))
                this.inputReceivers.Add(key, receiver);
            else
                this.inputReceivers[key] = receiver;
            receiver.SetActiveInputs(receiver.IsActiveByDefault);
        }
        protected virtual void OnEnable() {
            if (this.inputReceivers.Count > 0)
                foreach (KeyValuePair<string, InputReceiver> receiver in this.inputReceivers)
                    receiver.Value.SetActiveInputs(receiver.Value.IsActiveByDefault);
        }
        protected virtual void OnDisable() { 
            if (this.inputReceivers.Count > 0)
                foreach (KeyValuePair<string, InputReceiver> receiver in this.inputReceivers)
                    receiver.Value.SetActiveInputs(false);
        }

        public void InputReceiverSetActive(string key, bool targetState) { 
            if (this.inputReceivers.ContainsKey(key))
                this.inputReceivers[key].SetActiveInputs(targetState);
        }
    }
}