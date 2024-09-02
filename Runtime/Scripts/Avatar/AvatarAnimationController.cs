using UnityEngine;

using PolytopeSolutions.Toolset.GlobalTools.Generic;
using System.Collections;
using System;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    public class AvatarAnimationController : MonoBehaviour {
        [SerializeField] private AvatarAnimationSystemAvatarAnimationSettings avatarAnimationSettings;
        [SerializeField] private AvatarAnimationSystemAvatarAudioSettings avatarAudioSettings;

        private IAvatarMoveProvider avatarMoveProvider;
        public void SetAvatarMoveProvider(IAvatarMoveProvider avatarMoveProvider) {
            this.avatarMoveProvider = avatarMoveProvider;
        }

        public Func<(Vector3, float)> OnUpdateMove;

        private AvatarAnimationSystem animationSystem;

        ///////////////////////////////////////////////////////////////////////
        #region UNITY_FUNCTIONS
        private void Awake() {
            if (this.avatarAnimationSettings.Animator == null) {
                this.avatarAnimationSettings.Animator = GetComponentInChildren<Animator>();
                if (this.avatarAnimationSettings.Animator == null) {
                    this.LogError($"On avatar {name} Animator not found, Turning off AnimationController");
                    this.enabled = false;
                    return;
                }
            }
            this.animationSystem = new AvatarAnimationSystem(this, this.avatarAnimationSettings, this.avatarAudioSettings);
        }
        private void OnDestroy() {
            this.animationSystem.Destroy();
        }
        private void Update() {
            UpdateMovementState();
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        #region EXPOSED_INTERFACING
        private void UpdateMovementState() {
            (Vector3 velocity, float maxSpeed) = this.avatarMoveProvider.GetMoveState();
            this.animationSystem.UpdateMovement(velocity, maxSpeed);
        }
        public void SetAction(AvatarAnimationSystemActionAnimationSettings actionAnimationSettings) {
            this.avatarMoveProvider.OnInterruptMove();
            this.animationSystem.PlayAction(actionAnimationSettings);
        }
        public void PlayLeftFootstep() => PlayFootstep(AvatarAnimationSystem.Footstep.Left);
        public void PlayRightFootstep() => PlayFootstep(AvatarAnimationSystem.Footstep.Right);
        private void PlayFootstep(AvatarAnimationSystem.Footstep footstep) {
            if (this.avatarAudioSettings.IsValidFootStepData)
                this.animationSystem.PlayFootstep(footstep);
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
    }
}
