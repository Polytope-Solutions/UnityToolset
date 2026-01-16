#if ALLOW_AI_NAVIGATION
using UnityEngine;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    [RequireComponent(typeof(AvatarAnimationNavMeshMoveToTarget))]
    public class AvatarAnimationNavMeshVisualizeTarget : MonoBehaviour {
        [SerializeField] private Transform tGizmoPrefab;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;
        private Transform tGizmo;
        private MeshRenderer[] renderers;
        private bool wasValid = false;
        private void Awake() {
            if (this.tGizmoPrefab) {
                this.tGizmo = Instantiate(this.tGizmoPrefab);
                this.tGizmo.position = transform.position;
                this.tGizmo.localRotation = Quaternion.identity;
                this.tGizmo.localScale = Vector3.one;
                this.renderers = this.tGizmo.GetComponentsInChildren<MeshRenderer>();
                this.tGizmo.gameObject.SetActive(false);
            }
        }

        public void OnValidTarget(Vector3 position) {
            if (!this.tGizmo) return;
            this.tGizmo.position = position;
            if (!this.wasValid || !this.tGizmo.gameObject.activeSelf) {
                this.wasValid = true;
                this.tGizmo.gameObject.SetActive(true);
                for (int i = 0; i < this.renderers.Length; i++) {
                    this.renderers[i].sharedMaterial = this.validMaterial;
                }
            }
        }
        public void OnInvalidTarget(Vector3 position) {
            if (!this.tGizmo) return;
            this.tGizmo.position = position;

            if (this.wasValid || !this.tGizmo.gameObject.activeSelf) {
                this.wasValid = false;
                this.tGizmo.gameObject.SetActive(true);
                for (int i = 0; i < this.renderers.Length; i++) {
                    this.renderers[i].sharedMaterial = this.invalidMaterial;
                }
            }
        }
    }
}
#endif