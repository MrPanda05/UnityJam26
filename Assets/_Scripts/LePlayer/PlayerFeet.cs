using System;
using UnityEngine;

namespace JamUnity.LePlayer
{
    public class PlayerFeet : MonoBehaviour
    {
        [SerializeField] private LayerMask _head;
        [SerializeField] private float _aboveTolerance = 0.05f;

        public event Action<GameObject> OnEnemyHeadHit;

        private Collider2D _myCollider;
        private ContactFilter2D _filter;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];

        private Vector2 _prevCenter;
        private Vector2 _currentCenter;

        private void Awake()
        {
            _myCollider = GetComponent<Collider2D>();
            _filter = new ContactFilter2D();
            _filter.SetLayerMask(_head);
            _filter.useTriggers = true; // enemy head is a trigger

            _currentCenter = _myCollider.bounds.center;
            _prevCenter = _currentCenter;
        }

        private void FixedUpdate()
        {
            _prevCenter = _currentCenter;
            _currentCenter = _myCollider.bounds.center;

            Vector2 size = _myCollider.bounds.size;
            Vector2 delta = _currentCenter - _prevCenter;
            float dist = delta.magnitude;

            // Sweep the whole feet box along the path travelled since the last step.
            // If we barely moved, do a tiny downward cast so standing-on-head still counts.
            Vector2 dir = dist > 0.0001f ? delta / dist : Vector2.down;
            float castDist = Mathf.Max(dist, 0.02f);

            int count = Physics2D.BoxCast(_prevCenter, size, 0f, dir, _filter, _hits, castDist);

            float prevBottom = _prevCenter.y - size.y * 0.5f;

            for (int i = 0; i < count; i++)
            {
                Collider2D head = _hits[i].collider;

                // Valid stomp only if our feet were above the head's center last step.
                if (prevBottom + _aboveTolerance >= head.bounds.center.y)
                {
                    OnEnemyHeadHit?.Invoke(head.gameObject);
                    break; // one stomp per physics step
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_myCollider == null) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(_prevCenter, _myCollider.bounds.size);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_currentCenter, _myCollider.bounds.size);
        }
    }
}