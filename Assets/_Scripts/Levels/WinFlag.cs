using JamUnity.Commons;
using JamUnity.Core;
using System;
using UnityEngine;

namespace JamUnity.Levels
{
    public class WinFlag : MonoBehaviour
    {
        public static event Action OnGameWon;
        [SerializeField]
        private SoundPlayer _winSound;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            SoundPlayerManager.Instance.PlaySound(_winSound);
            print("Game won!");
            OnGameWon?.Invoke();
        }
    }
}
