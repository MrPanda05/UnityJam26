using UnityEngine;

namespace JamUnity.Enemies.Stats
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
    public class EnemyStats : ScriptableObject
    {
        public string enemyName;
        public float gravity;
        public float speed;
        public float jumpForce;
    }
}
