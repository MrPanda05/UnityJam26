using System;
using UnityEngine;

namespace JamUnity.Levels
{
    public class CheckPoint : MonoBehaviour
    {
        public event Action<CheckPoint> OnCheckPointReached;
        [field: SerializeField]
        public bool CheckPointReached { get; private set; }
        [field: SerializeField]
        public bool Repeatable { get; private set; }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!Repeatable && CheckPointReached) return;
            CheckPointReached = true;
            OnCheckPointReached?.Invoke(this);
            if (!Repeatable)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
