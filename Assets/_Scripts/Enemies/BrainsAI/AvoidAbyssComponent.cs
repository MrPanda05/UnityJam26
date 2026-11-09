using UnityEngine;

namespace JamUnity.Enemies.Brains
{
    public class AvoidAbyssComponent : MonoBehaviour
    {
        [SerializeField] private Transform _feet1, _feet2;
        [SerializeField] private float _range = 0.5f;
        [SerializeField] private LayerMask _mask;

        public bool Foot1HasGround { get; private set; }
        public bool Foot2HasGround { get; private set; }

        /// <summary>True if either foot has no ground below it.</summary>
        public bool IsAbyss => !Foot1HasGround || !Foot2HasGround;

        public bool ShouldInvert()
        {
            if (_feet1 == null || _feet2 == null) return false;

            Foot1HasGround = Physics2D.Raycast(_feet1.position, Vector2.down, _range, _mask).collider != null;
            Foot2HasGround = Physics2D.Raycast(_feet2.position, Vector2.down, _range, _mask).collider != null;

            return IsAbyss;


        }

        private void OnDrawGizmos()
        {
            if (_feet1 == null || _feet2 == null) return;

            Gizmos.color = Foot1HasGround ? Color.green : Color.red;
            Gizmos.DrawLine(_feet1.position, _feet1.position + Vector3.down * _range);
            Gizmos.color = Foot2HasGround ? Color.green : Color.red;
            Gizmos.DrawLine(_feet2.position, _feet2.position + Vector3.down * _range);
        }
    }
}