using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace JamUnity.Commons
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        private Vector2 _randomPitch;
        [SerializeField]
        private bool _useRandomPitch = false;

        public AudioSource Audio => _audioSource;

        public void PlaySound()
        {
            if (_useRandomPitch)
            {
                _audioSource.pitch += Random.Range(_randomPitch.x, _randomPitch.y);
            }
            _audioSource.Play();
            StartCoroutine(WaitForAudioEnd());
        }

        private IEnumerator WaitForAudioEnd()
        {
            yield return new WaitWhile(() => _audioSource.isPlaying);
            OnAudioFinished();
        }

        private void OnAudioFinished()
        {
            Destroy(gameObject);
        }
    }
}
