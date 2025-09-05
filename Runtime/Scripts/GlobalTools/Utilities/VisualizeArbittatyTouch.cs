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
            this.touchSprites[0].gameObject.SetActive(true);
            Vector2 itemPosition = Pointer.current.position.ReadValue();
            this.touchSprites[0].position = itemPosition;

            for (int i = 1; i < this.touchSprites.Length; i++) {
                if (Touchscreen.current != null && i < Touchscreen.current.touches.Count) {
                    TouchControl touch = Touchscreen.current.touches[i];
                    if (touch.isInProgress) {
                        this.touchSprites[i].gameObject.SetActive(true);
                        itemPosition = touch.position.ReadValue();
                        this.touchSprites[i].position = itemPosition;
                    } else {
                        this.touchSprites[i].gameObject.SetActive(false);
                    }
                } else {
                    this.touchSprites[i].gameObject.SetActive(false);
                }
            }

        }
    }
}
