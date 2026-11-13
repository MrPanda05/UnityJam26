using System;
using UnityEngine;

namespace JamUnity.Levels
{
    public class WinFlag : MonoBehaviour
    {
        public static event Action OnGameWon;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            print("Game won!");
            OnGameWon?.Invoke();
        }
    }
}
