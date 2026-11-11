using JamUnity.Events;
using JamUnity.LePlayer;
using System;
using UnityEngine;

namespace JamUnity.Levels
{
    public class CurrentLevelHandler : MonoBehaviour
    {
        [SerializeField]
        private EnemyList _enemyList;
        [SerializeField]
        private CheckPointList _checkPointList;
        [SerializeField]
        private RespawnEventChannel _respawnEvent;

        [field:SerializeField]
        public bool IsPlayerOnCurrentLevel { get; set; } = false;

        private void Awake()
        {
            Player.OnRestart += HandleRestart;
            _checkPointList.OnCheckPointHit += HandleHitCheckPoint;
        }
        private void OnDestroy()
        {
            Player.OnRestart -= HandleRestart;
            _checkPointList.OnCheckPointHit -= HandleHitCheckPoint;
            
        }

        private void HandleHitCheckPoint()
        {
            _enemyList.ClearDeadEnemies();
        }

        public void SetPlayerPosToDefault()
        {
            _respawnEvent.RaiseEvent(_checkPointList.GetDefaultPos());
        }

        private void HandleRestart()
        {
            if (!IsPlayerOnCurrentLevel) return;
            var respawnPos = _checkPointList.GetCurrentRespawnPosition();
            _respawnEvent.RaiseEvent(respawnPos);
            _enemyList.RespawnDeadEnemies();
        }
    }
}
