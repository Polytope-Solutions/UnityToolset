using UnityEngine;
using UnityEngine.Playables;

namespace PolytopeSolutions.Toolset.Animations.Avatar {
    public class AvatarAnimationSoundEffectBehaviour : PlayableBehaviour {
        [SerializeField] private AudioClip[] audioClips;
        [SerializeField] private ExposedReference<AudioSource> audioSourceReference;
        private AudioSource audioSource;

        private AudioClip RandomAudioClip => this.audioClips[Random.Range(0, this.audioClips.Length)];

        public void Init(AudioClip[] audioClips, AudioSource audioSource) {
            this.audioClips = audioClips;
            this.audioSourceReference = new ExposedReference<AudioSource> {
                defaultValue = audioSource
            }; 
        }

        public override void OnGraphStart(Playable playable) {
            this.audioSource = this.audioSourceReference.Resolve(playable.GetGraph().GetResolver());
        }

        public void PlaySoundEffect() {
            if (this.audioClips == null || this.audioClips.Length == 0 || this.audioSource == null)
                return;
            this.audioSource.PlayOneShot(this.RandomAudioClip);
        }
    }
}
