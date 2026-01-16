using UnityEngine;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    public class AvatarActionProvider : MonoBehaviour {
        [SerializeField] private AvatarAnimationController aniamationController;
        [SerializeField] private AvatarAnimationSystemActionAnimationSettings actionSettings;

        private void OnEnable() {
            this.actionSettings.LoadData();
        }
        private void OnDisable() {
            this.actionSettings.UnloadData();
        }

        public void TriggerAction() {
            this.aniamationController.SetAction(this.actionSettings);
        }
    }
}
