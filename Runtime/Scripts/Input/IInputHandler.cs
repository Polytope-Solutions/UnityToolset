using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System;

namespace PolytopeSolutions.Toolset.Input {
    public interface IInputHandler {
        protected InputReceiver InputReceiver { get; }
        public void StartInputHandler() {
            if (!this.InputReceiver)
                this.LogWarning("No InputReceiver found!");
        }
        public void OnDestroyInputHandler() {
            this.InputReceiver?.UnregisterInputHandler((IInputHandler)this);
        }
        public void OnInteractionStarted();
        public void OnInteractionPerformed(object data);
        public void OnInteractionEnded();

        public void RegisterInputHandler() {
            this.InputReceiver?.RegisterInputHandler((IInputHandler)this);
        }
        public void UnregisterInputHandler() {
            this.InputReceiver?.UnregisterInputHandler((IInputHandler)this);
        }
    }
}