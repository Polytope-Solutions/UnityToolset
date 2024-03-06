using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.UI;

namespace PolytopeSolutions.Toolset.GlobalTools.UI {
    public class UIAnyChoiceActionHolder : MonoBehaviour {
        [SerializeField] private UnityEvent OnAnyInteracted;
        [SerializeField] private bool disableButtonsOnStart;
        private Button[] buttons;

        private void Start() {
            this.buttons = GetComponentsInChildren<Button>();
            if (this.disableButtonsOnStart) {
                DisableButtons();
            }
            foreach (Button button in buttons) {
                button.onClick.AddListener(() => OnAnyInteracted.Invoke());
            }
        }
        public void EnableButtons() {
            foreach (Button button in buttons) {
                button.interactable = true;
            }
        }
        public void DisableButtons() {
            foreach (Button button in buttons) {
                button.interactable = false;
            }
        }
    }
}
