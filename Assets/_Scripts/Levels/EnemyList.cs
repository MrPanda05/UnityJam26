using JamUnity.Enemies;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JamUnity.Levels
{
    /// <summary>
    /// Only cares about Non disposable enemies
    /// </summary>
    public class EnemyList : MonoBehaviour
    {
        [SerializeField]
        private List<BasicEnemy> _allActiveEnemies = new();
        [SerializeField]
        private List<BasicEnemy> _allDeadEnemies = new();

        private void Awake()
        {
            GetAllCurrentEnemies();
        }
        [ContextMenu("Respawn Dead Enemies")]
        public void RespawnDeadEnemies()
        {
            if(_allDeadEnemies.Count <= 0) return;
            foreach (BasicEnemy item in _allDeadEnemies)
            {
                item.Respawn();
                //_allActiveEnemies.Add(item);
            }
            _allDeadEnemies.Clear();
        }
        public void ClearDeadEnemies()
        {
            foreach (var item in _allDeadEnemies)
            {
                Destroy(item.gameObject);
            }
            _allDeadEnemies.Clear();
        }

        private void GetAllCurrentEnemies()
        {
            foreach (Transform child in transform)
            {
                if(child.TryGetComponent<BasicEnemy>(out BasicEnemy enemy))
                {
                    _allActiveEnemies.Add(enemy);
                    if (!enemy.IsDisposable)
                    {
                        enemy.OnDeath += HandleInstanceDeath;
                        enemy.OnRespawn += HandleRespawn;
                    }
                }
            }
        }

        private void HandleRespawn(BasicEnemy instance)
        {
            //_allDeadEnemies.Remove(instance);
            _allActiveEnemies.Add(instance);
        }

        private void HandleInstanceDeath(BasicEnemy instance)
        {
            _allActiveEnemies.Remove(instance);
            _allDeadEnemies.Add(instance);
        }
    }
}
