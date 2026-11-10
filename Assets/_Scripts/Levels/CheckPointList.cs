using JamUnity.Enemies;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JamUnity.Levels
{
    public class CheckPointList : MonoBehaviour
    {
        [SerializeField]
        private List<CheckPoint> _checkPoints = new();

        [SerializeField]
        private Transform _defaultRespawnPosition;

        [SerializeField]
        private Transform _currentRespawnPosition;

        public event Action OnCheckPointHit;
        
    private void Awake()
        {
            GetAllCurrentEnemies();
        }
        private void GetAllCurrentEnemies()
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<CheckPoint>(out CheckPoint checkPoint))
                {
                    _checkPoints.Add(checkPoint);
                    checkPoint.OnCheckPointReached += HandleOnReach;
                }
            }
        }

        private void HandleOnReach(CheckPoint instance)
        {
            _currentRespawnPosition = instance.transform;
            OnCheckPointHit?.Invoke();
        }
        public Transform GetCurrentRespawnPosition()
        {
            if( _currentRespawnPosition == null)
            {
                return _defaultRespawnPosition;
            }
            return _currentRespawnPosition;
        }
    }
}
