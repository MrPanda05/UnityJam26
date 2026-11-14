using JamUnity.Commons;
using UnityEngine;

namespace JamUnity.Core
{
    public class SoundPlayerManager : MonoBehaviour
    {
        public static SoundPlayerManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        public void PlaySound(SoundPlayer sound)
        {
            if (sound == null) return;
            var instance = Instantiate(sound);
            instance.PlaySound();
        }
    }
}
