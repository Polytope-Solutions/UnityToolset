#define DEBUG
// #undef DEBUG
// #define DEBUG2
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace PolytopeSolutions.Toolset.UI {
    //[CreateAssetMenu(fileName = "GenericUIState", menuName = "PolytopeSolutions/UI", order = 0)]
    [Serializable]
    public class UIState { // ScriptableObject
        [SerializeField] protected string stateName;
        [SerializeField] protected string stateID;
        [SerializeField] protected List<Canvas> canvases;

        public string StateName => this.stateName;
        public string StateID => this.stateID;

        protected bool isActive = false;
        public bool Contains(Canvas canvas) {
            return this.canvases.Contains(canvas);
        }

        public virtual void Activate(UIState previous=null, bool immediate = false) {
            if (!this.isActive) { 
                #if DEBUG2
                this.Log($"Activating {this.name}");
                #endif
                this.isActive = true;
            }
        }
        public virtual void Deactivate(UIState previous=null, bool immediate = false) {
            if (this.isActive) { 
                #if DEBUG2
                this.Log($"Deactivating {this.name}");
                #endif
                this.isActive = false;
            }
        }
        public virtual void Toggle() {
            if (this.isActive) {
                this.Deactivate();
            } else {
                this.Activate();
            }
        }
    }
}
