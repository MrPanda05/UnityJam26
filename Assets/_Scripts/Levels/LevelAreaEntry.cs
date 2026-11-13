using System;
using Unity.Cinemachine;
using UnityEngine;

namespace JamUnity.Levels
{
    public class LevelAreaEntry : MonoBehaviour
    {
        private Collider2D _collider;
        [SerializeField]
        private CurrentLevelHandler _handler;
        [SerializeField]
        private bool _setPosOnEnter = false;
        private bool _hasEntered = false;
        public static event Action<Collider2D> OnLevelEntry;
        [SerializeField]
        private CinemachineCamera _cam;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            _cam.Priority = 1000;
            _cam.Follow = collision.gameObject.transform;
            _handler.IsPlayerOnCurrentLevel = true;
            OnLevelEntry?.Invoke(_collider);
            if (_setPosOnEnter && !_hasEntered)
            {
                _handler.SetPlayerPosToDefault();
            }
            _hasEntered = true;
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            _cam.Priority = -1;
            _handler.IsPlayerOnCurrentLevel = false;
        }
    }
}
