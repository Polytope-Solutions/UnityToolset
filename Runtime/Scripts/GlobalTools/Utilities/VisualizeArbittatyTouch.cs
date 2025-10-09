using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace PolytopeSolutions.Toolset.GlobalTools.Utilities {
    [RequireComponent(typeof(Canvas))]
    public class VisualizeArbittatyTouch : MonoBehaviour {
        [SerializeField] private Transform touchSpritePrefab;

        private Transform[] touchSprites = new Transform[10];

        private void Start() {
            for (int i = 0; i < this.touchSprites.Length; i++) {
                Transform tItem = Instantiate(this.touchSpritePrefab, transform);
                tItem.gameObject.SetActive(false);
                this.touchSprites[i] = tItem;
            }
        }
        private void Update() {
            // Handle pointer universally
            bool show =
                (Pointer.current != null &&
                (Pointer.current is not Touchscreen || Touchscreen.current.primaryTouch.isInProgress));
            Vector2 itemPosition = Vector2.zero;
            if (this.touchSprites[0].gameObject.activeSelf != show) {
                this.touchSprites[0].gameObject.SetActive(show);
            }
            if (show) {
                itemPosition = Pointer.current.position.ReadValue();
                this.touchSprites[0].position = itemPosition;
            }

            for (int i = 1; i < this.touchSprites.Length; i++) {
                show = false;
                if (Touchscreen.current != null && i < Touchscreen.current.touches.Count) {
                    TouchControl touch = Touchscreen.current.touches[i];
                    if (touch.isInProgress) {
                        show = true;
                        itemPosition = touch.position.ReadValue();
                    }
                }

                if (this.touchSprites[i].gameObject.activeSelf != show) {
                    this.touchSprites[i].gameObject.SetActive(show);
                }
                if (show)
                    this.touchSprites[i].position = itemPosition;
            }

        }
    }
}
