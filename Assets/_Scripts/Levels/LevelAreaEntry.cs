using UnityEngine;

namespace JamUnity.Levels
{
    public class LevelAreaEntry : MonoBehaviour
    {
        [SerializeField]
        private CurrentLevelHandler _handler;
        [SerializeField]
        private bool _setPosOnEnter = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            _handler.IsPlayerOnCurrentLevel = true;
            if (_setPosOnEnter)
            {
                _handler.SetPlayerPosToDefault();
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            _handler.IsPlayerOnCurrentLevel = false;
        }
    }
}
