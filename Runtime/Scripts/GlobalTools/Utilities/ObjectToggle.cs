using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    public class ObjectToggle : MonoBehaviour {
        [SerializeField] private bool startActive = true;

        private void Start() {
            gameObject.SetActive(this.startActive);
        }

        public void Toggle() {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}

