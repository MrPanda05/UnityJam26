using JamUnity.Core;
using UnityEngine;

namespace JamUnity.Commons.Components
{
    public class PlayMusic : MonoBehaviour
    {
        [SerializeField]
        private SoundPlayer _music;

        [SerializeField]
        private bool _playOnStart = false;

        private void Start()
        {
            if (_playOnStart)
            {
                PlayMusicThis();
            }
        }

        public void PlayMusicThis()
        {
            MusicPlayerManager.Instance.PlayMusic(_music);
        }
    }
}
