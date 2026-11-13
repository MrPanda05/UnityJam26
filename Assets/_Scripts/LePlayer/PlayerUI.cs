using System;
using UnityEngine;

namespace JamUnity.LePlayer
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private Player _player;
        [SerializeField]
        private HealthUI _healthUI;
        private void OnEnable()
        {
            _player.OnDeath += HandleDeath;
            _healthUI.SetLiveText(_player.CurrentLives.ToString());

        }
        private void OnDisable()
        {
            _player.OnDeath -= HandleDeath;
        }
        private void Start()
        {
            _healthUI.SetLiveText(_player.CurrentLives.ToString());
        }

        private void HandleDeath()
        {
            _healthUI.SetLiveText(_player.CurrentLives.ToString());
        }
    }
}
