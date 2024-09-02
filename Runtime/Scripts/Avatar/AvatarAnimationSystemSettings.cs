using UnityEngine;

using System;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    [Serializable]
    public class AvatarAnimationSystemAvatarAnimationSettings {
        [Header("Core Rig Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip walkClip;
        [SerializeField] private AvatarMask movementMask;

        public Animator Animator {
            get => this.animator;
            set => this.animator = value;
        }
        public AnimationClip IdleClip => this.idleClip;
        public AnimationClip WalkClip => this.walkClip;
        public AvatarMask MovementMask => this.movementMask;
    }
    [Serializable]
    public class AvatarAnimationSystemActionAnimationSettings {
        [Header("Animation")]
        [SerializeField] private AnimationClip actionClip;
        [SerializeField] private AvatarMask actionAvatarMask;
        [Header("Audio")]
        [SerializeField] private AudioClip speechAudioClip;

        public AnimationClip ActionClip => this.actionClip;
        public AvatarMask ActionAvatarMask => this.actionAvatarMask;
        public AudioClip SpeechAudioClip => this.speechAudioClip;

        public void LoadData() => this.speechAudioClip?.LoadAudioData();
        public void UnloadData() => this.speechAudioClip?.UnloadAudioData();
    }
    [Serializable]
    public class AvatarAnimationSystemAvatarAudioSettings {
        [Header("Main")]
        [SerializeField] private AudioSource mainAudioSource;
        public AudioSource MainAudioSource => this.mainAudioSource;
        public bool IsValidMainAudioData => this.mainAudioSource != null;
        [Header("Footsteps")]
        // Footsteps
        [SerializeField] private AudioSource leftFootAudioSource;
        [SerializeField] private AudioSource rightFootAudioSource;
        [SerializeField] private AudioClip[] footstepAudioClips;
        public AudioSource LeftFootAudioSource => this.leftFootAudioSource;
        public AudioSource RightFootAudioSource => this.rightFootAudioSource;
        public bool IsValidFootStepData => this.leftFootAudioSource != null && this.rightFootAudioSource != null && this.footstepAudioClips != null && this.footstepAudioClips.Length > 0;
        public AudioClip[] FootstepAudioClips => this.footstepAudioClips;
    }
}