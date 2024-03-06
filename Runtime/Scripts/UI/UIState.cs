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
        [SerializeField] protected List<Canvas> canvases;

        protected bool isActive = false;

        public virtual void Activate(bool immediate = false) {
            if (!this.isActive) { 
                #if DEBUG2
                this.Log($"Activating {this.name}");
                #endif
                this.isActive = true;
            }
        }
        public virtual void Deactivate(bool immediate = false) {
            if (this.isActive) { 
                #if DEBUG2
                this.Log($"Deactivating {this.name}");
                #endif
                this.isActive = false;
            }
        }
    }
}
