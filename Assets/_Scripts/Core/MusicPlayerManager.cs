using JamUnity.Commons;
using UnityEngine;

namespace JamUnity.Core
{
    public class MusicPlayerManager : MonoBehaviour
    {
        public static MusicPlayerManager Instance { get; private set; }

        private AudioSource _musicSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _musicSource = GetComponent<AudioSource>();
        }

        public void PlayMusic(SoundPlayer sound)
        {
            if (sound == null) return;
            _musicSource.Stop();
            _musicSource.generator = sound.Audio.generator;
            _musicSource.Play();
        }
    }
}
