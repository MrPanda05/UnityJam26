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

        private void Awake()
        {
            Player.OnRestart += HandleRestart;
            _checkPointList.OnCheckPointHit += HandleHitCheckPoint;
        }

        private void HandleHitCheckPoint()
        {
            _enemyList.ClearDeadEnemies();
        }

        private void HandleRestart()
        {
            var respawnPos = _checkPointList.GetCurrentRespawnPosition();
            _respawnEvent.RaiseEvent(respawnPos);
            _enemyList.RespawnDeadEnemies();
        }
    }
}
