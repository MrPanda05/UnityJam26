using System;
using UnityEngine;

namespace JamUnity.Events
{
    [CreateAssetMenu(fileName = "RespawnEventChannel", menuName = "Scriptable Objects/RespawnEventChannel")]
    public class RespawnEventChannel : ScriptableObject
    {
        public event Action<Transform> OnRespawn;
        public void RaiseEvent(Transform respawnTransform)
        {
            OnRespawn?.Invoke(respawnTransform);
        }
    }
}
