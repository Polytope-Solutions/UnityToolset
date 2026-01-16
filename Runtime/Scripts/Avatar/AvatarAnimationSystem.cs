#define USE_UNITY_COROUTINE

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

#if USE_UNITY_COROUTINE
#else
#endif

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    public class AvatarAnimationSystem {
        private readonly MonoBehaviour owner;

        private readonly PlayableGraph animationGraph;
        private AnimationMixerPlayable coreAnimationMixer;
        private AnimationLayerMixerPlayable layerAnimationMixer;
        private AnimationMixerPlayable movementAnimationMixer;
        private AnimationClipPlayable idlePlayable;
        private AnimationClipPlayable walkPlayable;
        private AnimationClipPlayable actionPlayable;

        private ScriptPlayable<AvatarAnimationSoundEffectBehaviour> leftFootScrtiptPlayable;
        private ScriptPlayable<AvatarAnimationSoundEffectBehaviour> rightFootScrtiptPlayable;
        public enum Footstep {
            Left,
            Right
        }

        private AudioMixerPlayable coreAudioMixer;
        private AudioClipPlayable speechAudioPlayable;

        private static readonly int ANIMATION_CORE_LAYER_INPUT = 0;
        private static readonly int ANIMATION_LAYER_OUTPUT = 0;
        private static readonly int ANIMATION_LAYER_MOVEMENT_INPUT = 0;
        private static readonly int ANIMATION_LAYER_ACTION_INPUT = 1;
        private static readonly int ANIMATION_MOVEMENT_OUTPUT = 0;
        private static readonly int ANIMATION_MOVEMENT_IDLE_INPUT = 0;
        private static readonly int ANIMATION_MOVEMENT_WALK_INPUT = 1;
        private static readonly int ANIMATION_IDLE_OUTPUT = 0;
        private static readonly int ANIMATION_MWALK_OUTPUT = 0;
        private static readonly int ANIMATION_ACTION_OUTPUT = 0;

        private static readonly int AUDIO_CORE_SPEECH_INPUT = 0;
        private static readonly int AUDIO_SPEECH_OUTPUT = 0;

        private static readonly float NOWEIGHT = 0f;
        private static readonly float FULLWEIGHT = 1f;
        private static readonly float MIN_BLEND_DURATION = 0.2f;
        private static readonly float BLEND_PERCENTAGE = 0.2f;

        #if USE_UNITY_COROUTINE
        private Coroutine currentActionBlendCoroutine;
        #else
        #endif

        ///////////////////////////////////////////////////////////////////////
        #region MAIN
        public AvatarAnimationSystem(MonoBehaviour owner,
                AvatarAnimationSystemAvatarAnimationSettings avatarAnimationSettings,
                AvatarAnimationSystemAvatarAudioSettings avatarAudioSettings) {
            this.owner = owner;
            this.animationGraph = PlayableGraph.Create("AvatarAnimationSystem");
            SetUpCoreAnimations(avatarAnimationSettings);
            SetUpMovementAnimations(avatarAnimationSettings);
            SeyUpMainAudio(avatarAudioSettings);
            SetUpMovementAudio(avatarAudioSettings);
            this.animationGraph.Play();
        }

        public void Destroy() {
            if (this.currentActionBlendCoroutine != null && this.owner != null)
                this.owner.StopCoroutine(this.currentActionBlendCoroutine);
            if (this.animationGraph.IsValid())
                this.animationGraph.Destroy();
        }

        private void SetUpCoreAnimations(AvatarAnimationSystemAvatarAnimationSettings avatarAnimationSettings) {
            // Main setup
            AnimationPlayableOutput coreOutput = AnimationPlayableOutput.Create(
                this.animationGraph, "CoreOutput", avatarAnimationSettings.Animator
            );
            // Core Mixer Setup
            this.coreAnimationMixer = AnimationMixerPlayable.Create(this.animationGraph, 1);
            coreOutput.SetSourcePlayable(this.coreAnimationMixer);
            this.layerAnimationMixer = AnimationLayerMixerPlayable.Create(this.animationGraph, 2);
            this.coreAnimationMixer.ConnectInput(
                AvatarAnimationSystem.ANIMATION_CORE_LAYER_INPUT, this.layerAnimationMixer, AvatarAnimationSystem.ANIMATION_LAYER_OUTPUT, AvatarAnimationSystem.FULLWEIGHT
            );
        }
        private void SetUpMovementAnimations(AvatarAnimationSystemAvatarAnimationSettings avatarAnimationSettings) {
            // Movement Mixer Setup
            this.movementAnimationMixer = AnimationMixerPlayable.Create(this.animationGraph, 2);
            this.layerAnimationMixer.ConnectInput(
                AvatarAnimationSystem.ANIMATION_LAYER_MOVEMENT_INPUT, this.movementAnimationMixer, AvatarAnimationSystem.ANIMATION_MOVEMENT_OUTPUT, AvatarAnimationSystem.FULLWEIGHT
            );
            this.layerAnimationMixer.SetLayerMaskFromAvatarMask((uint)AvatarAnimationSystem.ANIMATION_LAYER_MOVEMENT_INPUT, avatarAnimationSettings.MovementMask);

            avatarAnimationSettings.IdleClip.wrapMode = WrapMode.Loop;
            this.idlePlayable = AnimationClipPlayable.Create(this.animationGraph, avatarAnimationSettings.IdleClip);
            this.idlePlayable.SetApplyFootIK(true);
            avatarAnimationSettings.WalkClip.wrapMode = WrapMode.Loop;
            this.walkPlayable = AnimationClipPlayable.Create(this.animationGraph, avatarAnimationSettings.WalkClip);
            this.walkPlayable.SetApplyFootIK(true);
            this.movementAnimationMixer.ConnectInput(
                AvatarAnimationSystem.ANIMATION_MOVEMENT_IDLE_INPUT, this.idlePlayable, AvatarAnimationSystem.ANIMATION_IDLE_OUTPUT, AvatarAnimationSystem.FULLWEIGHT
            );
            this.movementAnimationMixer.ConnectInput(
                AvatarAnimationSystem.ANIMATION_MOVEMENT_WALK_INPUT, this.walkPlayable, AvatarAnimationSystem.ANIMATION_MWALK_OUTPUT, AvatarAnimationSystem.NOWEIGHT
            );
            //this.movementAnimationMixer.SetInputWeight(
            //    AvatarAnimationSystem.ANIMATION_MOVEMENT_IDLE_INPUT, AvatarAnimationSystem.FULLWEIGHT
            //);
        }
        private void SeyUpMainAudio(AvatarAnimationSystemAvatarAudioSettings avatarAudioSettings) {
            if (!avatarAudioSettings.IsValidMainAudioData) return;

            AudioPlayableOutput audioOutput = AudioPlayableOutput.Create(this.animationGraph, "AudioOutput", avatarAudioSettings.MainAudioSource);
            this.coreAudioMixer = AudioMixerPlayable.Create(this.animationGraph, 1);
            audioOutput.SetSourcePlayable(this.coreAudioMixer);
        }
        private void SetUpMovementAudio(AvatarAnimationSystemAvatarAudioSettings avatarAudioSettings) {
            if (!avatarAudioSettings.IsValidFootStepData) return;

            ScriptPlayableOutput leftFootStepAudioOutput = ScriptPlayableOutput.Create(this.animationGraph, "LeftFootstepAudioOutput");
            this.leftFootScrtiptPlayable = ScriptPlayable<AvatarAnimationSoundEffectBehaviour>.Create(this.animationGraph);
            AvatarAnimationSoundEffectBehaviour leftSoundEffectBehaviour = this.leftFootScrtiptPlayable.GetBehaviour();
            leftSoundEffectBehaviour.Init(avatarAudioSettings.FootstepAudioClips, avatarAudioSettings.LeftFootAudioSource);
            leftFootStepAudioOutput.SetSourcePlayable(this.leftFootScrtiptPlayable);

            ScriptPlayableOutput rightFootStepAudioOutput = ScriptPlayableOutput.Create(this.animationGraph, "RightFootstepAudioOutput");
            this.rightFootScrtiptPlayable = ScriptPlayable<AvatarAnimationSoundEffectBehaviour>.Create(this.animationGraph);
            AvatarAnimationSoundEffectBehaviour rightSoundEffectBehaviour = this.rightFootScrtiptPlayable.GetBehaviour();
            rightSoundEffectBehaviour.Init(avatarAudioSettings.FootstepAudioClips, avatarAudioSettings.RightFootAudioSource);
            rightFootStepAudioOutput.SetSourcePlayable(this.rightFootScrtiptPlayable);
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        #region ANIMATION_MOVEMENT
        public void UpdateMovement(Vector3 velocity, float maxSpeed) {
            float weight = Mathf.InverseLerp(0f, maxSpeed, velocity.magnitude);
            this.movementAnimationMixer.SetInputWeight(
                AvatarAnimationSystem.ANIMATION_MOVEMENT_IDLE_INPUT, AvatarAnimationSystem.FULLWEIGHT - weight
            );
            this.movementAnimationMixer.SetInputWeight(
                AvatarAnimationSystem.ANIMATION_MOVEMENT_WALK_INPUT, weight
            );
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        #region ANIMATION_ACTIONS
        public void PlayAction(AvatarAnimationSystemActionAnimationSettings actionSettings) {
            if (this.actionPlayable.IsValid() && this.actionPlayable.GetAnimationClip() == actionSettings.ActionClip)
                return;
            InteruptAction();

            // Animation
            this.actionPlayable = AnimationClipPlayable.Create(this.animationGraph, actionSettings.ActionClip);
            this.layerAnimationMixer.ConnectInput(
                AvatarAnimationSystem.ANIMATION_LAYER_ACTION_INPUT, this.actionPlayable, AvatarAnimationSystem.ANIMATION_ACTION_OUTPUT, AvatarAnimationSystem.NOWEIGHT
            );
            this.layerAnimationMixer.SetLayerMaskFromAvatarMask((uint)AvatarAnimationSystem.ANIMATION_LAYER_ACTION_INPUT, actionSettings.ActionAvatarMask);
            // Speech
            if (this.coreAudioMixer.IsValid() && actionSettings.SpeechAudioClip != null) {
                this.speechAudioPlayable = AudioClipPlayable.Create(this.animationGraph, actionSettings.SpeechAudioClip, false);
                this.coreAudioMixer.ConnectInput(
                    AvatarAnimationSystem.AUDIO_CORE_SPEECH_INPUT, this.speechAudioPlayable, AvatarAnimationSystem.AUDIO_SPEECH_OUTPUT, AvatarAnimationSystem.NOWEIGHT
                );
            }

            SmoothExecuteAction(actionSettings.ActionClip.length);
        }
        private void InteruptAction() {
            if (this.currentActionBlendCoroutine != null)
                this.owner.StopCoroutine(this.currentActionBlendCoroutine);

            //this.layerAnimationMixer.SetInputWeight(
            //    AvatarAnimationSystem.ANIMATION_LAYER_MOVEMENT_INPUT, AvatarAnimationSystem.FULLWEIGHT
            //);
            this.layerAnimationMixer.SetInputWeight(
                AvatarAnimationSystem.ANIMATION_LAYER_ACTION_INPUT, AvatarAnimationSystem.NOWEIGHT
            );
            if (this.coreAudioMixer.IsValid())
                this.coreAudioMixer.SetInputWeight(
                    AvatarAnimationSystem.AUDIO_CORE_SPEECH_INPUT, AvatarAnimationSystem.NOWEIGHT
                );

            DisconnectAction();
            DisconnectSpeech();
            this.currentActionBlendCoroutine = null;
        }
        private void DisconnectAction() {
            if (!this.actionPlayable.IsValid()) return;
            this.layerAnimationMixer.DisconnectInput(AvatarAnimationSystem.ANIMATION_LAYER_ACTION_INPUT);
            this.animationGraph.DestroyPlayable(this.actionPlayable);
        }
        private void DisconnectSpeech() {
            if (!this.coreAudioMixer.IsValid() || !this.speechAudioPlayable.IsValid()) return;
            this.coreAudioMixer.DisconnectInput(AvatarAnimationSystem.AUDIO_CORE_SPEECH_INPUT);
            this.animationGraph.DestroyPlayable(this.speechAudioPlayable);
        }
        private void SmoothExecuteAction(float fullDuration) {
            this.currentActionBlendCoroutine = this.owner.StartCoroutine(SmoothExecuteActionCoroutine(fullDuration));
        }
        float blendDuration, weight, blendT;
        private IEnumerator SmoothExecuteActionCoroutine(float fullDuration) {
            blendDuration = Mathf.Max(AvatarAnimationSystem.MIN_BLEND_DURATION,
                Mathf.Min(fullDuration * AvatarAnimationSystem.BLEND_PERCENTAGE, fullDuration / 2f));
            // Blend in
            yield return Blend(blendDuration, false);
            // Delay
            float delay = fullDuration - blendDuration * 2;
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            // Blend out
            yield return Blend(blendDuration, true);
            // Clean up
            DisconnectAction();
            DisconnectSpeech();
            this.currentActionBlendCoroutine = null;
        }
        private IEnumerator Blend(float blendDuration, bool invert) {
            blendT = 0f;
            while (blendT < 1f) {
                blendT += Time.deltaTime / blendDuration;
                weight = Mathf.Lerp(0f, AvatarAnimationSystem.FULLWEIGHT, blendT);
                weight = (!invert) ? weight : 1 - weight;
                UpdateActionWeights(weight);
                yield return null;
            }
            weight = (!invert) ? 1f : 0f;
            UpdateActionWeights(weight);
        }
        private void UpdateActionWeights(float weight) {
            //this.layerAnimationMixer.SetInputWeight(
            //    AvatarAnimationSystem.ANIMATION_LAYER_MOVEMENT_INPUT, 1f - weight
            //);
            if (this.actionPlayable.IsValid())
                this.layerAnimationMixer.SetInputWeight(
                    AvatarAnimationSystem.ANIMATION_LAYER_ACTION_INPUT, weight
                );
            if (this.coreAudioMixer.IsValid() && this.speechAudioPlayable.IsValid())
                this.coreAudioMixer.SetInputWeight(
                    AvatarAnimationSystem.AUDIO_CORE_SPEECH_INPUT, weight
                );
        }
        #endregion
        ///////////////////////////////////////////////////////////////////////
        #region MOVEMENT_AUDIO
        public void PlayFootstep(Footstep footstep) {
            switch (footstep) {
                case Footstep.Left:
                    this.leftFootScrtiptPlayable.GetBehaviour().PlaySoundEffect();
                    break;
                case Footstep.Right:
                    this.rightFootScrtiptPlayable.GetBehaviour().PlaySoundEffect();
                    break;
            }
        }
        #endregion
    }
}
