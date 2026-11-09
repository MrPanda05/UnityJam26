using UnityEngine;

namespace JamUnity.Enemies.Brains
{
    public class PatrolBrain : MonoBehaviour, IBrain
    {
        public bool IsThinking { get; private set; }

        [field: SerializeField]
        public int MaxThinkingTime { get; private set; }
        private int _thinkCount = 0;

        private Vector3 _initialGlobalPos;
        [field: SerializeField]
        public BasicEnemy MyEnemy { get; private set; }

        /// <summary>
        /// Offsets from the starting position (e.g. -3 and 5). Order doesn't matter.
        /// </summary>
        [SerializeField]
        private Vector2 _patrolRange = new Vector2(-3f, 3f);
        [Header("AI configs")]
        [SerializeField]
        private AvoidAbyssComponent _abyssDetector;
        [SerializeField]
        private bool _xAxis = true;

        /// <summary>
        /// If true, enemy will only patrol within the patrol range
        /// </summary>
        [SerializeField]
        private bool _smart = false;

        private int _currentDir = 1;

        private void Awake()
        {
            _initialGlobalPos = transform.position;
        }

        public void StartThinking() => IsThinking = true;
        public void StopThinking() => IsThinking = false;

        public void Think()
        {
            if (!IsThinking) return;

            // Wall check (every MaxThinkingTime frames)
            if (_thinkCount > MaxThinkingTime)
            {
                var count = MyEnemy.GetCurrentContactPoints();
                for (int i = 0; i < count; i++)
                {
                    if (Mathf.Abs(MyEnemy.ContactPoints[i].normal.x) > 0.9f)
                    {
                        _currentDir *= -1;
                        break;
                    }
                }
                if(_abyssDetector != null)
                {
                    if (_abyssDetector.ShouldInvert())
                    {
                        _currentDir *= -1;
                    }
                }
                _thinkCount = 0;
            }

            // "Virtual waypoint" check
            if (_smart)
            {
                GetBounds(out float min, out float max);
                float pos = _xAxis ? transform.position.x : transform.position.y;

                if (pos >= max && _currentDir > 0)
                    _currentDir = -1;
                else if (pos <= min && _currentDir < 0)
                    _currentDir = 1;
            }

            MyEnemy.HandleGravity();
            MyEnemy.MoveHorizontly(_currentDir);
            MyEnemy.ApplyMovement();
            _thinkCount++;
        }

        private void GetBounds(out float min, out float max)
        {
            float origin = _xAxis ? _initialGlobalPos.x : _initialGlobalPos.y;
            min = origin + Mathf.Min(_patrolRange.x, _patrolRange.y);
            max = origin + Mathf.Max(_patrolRange.x, _patrolRange.y);
        }

        private void OnDrawGizmos()
        {
            // In edit mode Awake hasn't run, so use the current position
            Vector3 origin = Application.isPlaying ? _initialGlobalPos : transform.position;
            float lo = Mathf.Min(_patrolRange.x, _patrolRange.y);
            float hi = Mathf.Max(_patrolRange.x, _patrolRange.y);
            Vector3 axis = _xAxis ? Vector3.right : Vector3.up;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin + axis * lo, origin + axis * hi);
            Gizmos.DrawWireSphere(origin + axis * lo, 0.15f);
            Gizmos.DrawWireSphere(origin + axis * hi, 0.15f);
        }
    }
}