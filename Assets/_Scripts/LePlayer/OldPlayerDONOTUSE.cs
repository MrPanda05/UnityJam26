using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JamUnity.LePlayer
{
    //do not use
    public class OldPlayerDONOTUSE : MonoBehaviour
    {
        private Rigidbody2D _rig;
        private Collider2D _col;
        private ContactPoint2D[] _contacts = new ContactPoint2D[10];

        [Header("Movement")]
        [SerializeField]
        private float _acel = 10f;
        [SerializeField]
        private float _dcel = 10f;
        [SerializeField]
        private float _maxSpeed = 5f;
        [SerializeField]
        private bool _deriveAcels = false;
        [SerializeField]
        private float _timeToMaxSpeed = 1f;
        [SerializeField]
        private float _timeToStop = 1f;
        [Header("Slide")]
        [SerializeField, Range(0f, 89f)] 
        private float _maxSlopeAngle = 60f;
        [SerializeField] 
        private int _slideIterations = 3;

        [Header("Jump")]
        [SerializeField]
        private int _maxJumps = 1;
        [SerializeField]
        private float _jumpHeight = 1;
        [SerializeField]
        private float _jumpTimeToPeak = .5f;
        [SerializeField]
        private float _jumpTimeToGround = .5f;
        [SerializeField]
        private float _variableJumpHeight = .5f;
        [SerializeField]
        private LayerMask _masks;
        [SerializeField]
        private Vector2 _boxSize;
        [SerializeField]
        private float _castDistance = 0;
        [SerializeField]
        private int _maxFramesToCoyoteJump = 5;
        [SerializeField]
        private int _maxFramesToBufferJump = 5;
        [SerializeField]
        private float _terminalVelocity = 30;


        private float _jumpVelocity;
        private float _jumpGravity;
        private float _fallGravity;
        private float _variableGravity;
        private Vector2 _moveInput;
        private Vector2 _velocity;
        private bool _jumpJustPressed;
        private bool _jumpPressed;
        [SerializeField] 
        private InputActionAsset _playerActions;
        private InputAction _move;
        private InputAction _jump;

        private bool _isGrounded = false;
        private bool _isOneWay = false;
        private bool _hitCelling = false;
        private bool _wasOnGround = false;
        private int _jumpCount = 0;
        private int _coyoteFrameCount = 0;
        private int _bufferFrameCount = 0;
        private bool _hasGroundJump = false;
        private bool _hasAirJump = false;
        private bool _countCoyoteFrames = false;
        private bool _countBufferFrames = false;
        private bool _jumpedSinceGround = false;

        #region Debug
        [Header("Debug")]
        [SerializeField] private bool _debugDrawJumps = true;
        [SerializeField] private bool _debugStartAtMaxSpeed = true;   // running jump vs. standing jump
        [SerializeField] private float _debugDirection = 1f;          // 1 = right, -1 = left
        [SerializeField, Range(0f, 1f)] private float _debugHoldTime = 0.2f; // seconds held, for the "variable" arc
        private struct JumpSim
        {
            public List<Vector2> points;
            public Vector2 apex;
            public float height;
            public float distance;
            public float airTime;
        }

        // Mirrors FixedUpdate: step 0 is the jump frame (velocity set, no gravity), then gravity each step.
        private JumpSim SimulateJump(float holdTime, float dirX)
        {
            var sim = new JumpSim { points = new List<Vector2>() };
            float dt = Time.fixedDeltaTime;
            float acel = _deriveAcels ? _maxSpeed / _timeToMaxSpeed : _acel;

            Vector2 pos = Vector2.zero;
            Vector2 vel = new Vector2(_debugStartAtMaxSpeed ? dirX * _maxSpeed : 0f, _jumpVelocity);
            sim.points.Add(pos);

            for (int i = 0; i < 1000; i++)
            {
                vel.x = Mathf.MoveTowards(vel.x, dirX * _maxSpeed, acel * dt);

                if (i > 0)
                {
                    bool held = (i * dt) < holdTime;
                    float g = vel.y > 0f ? (held ? _jumpGravity : _variableGravity) : _fallGravity;
                    vel.y += g * dt;
                }

                Vector2 prev = pos;
                pos += vel * dt;

                if (pos.y > sim.height)
                {
                    sim.height = pos.y;
                    sim.apex = pos;
                }

                if (i > 0 && pos.y <= 0f)
                {
                    // Interpolate the exact landing point on flat ground at the start height
                    float f = prev.y / (prev.y - pos.y);
                    pos = Vector2.Lerp(prev, pos, f);
                    sim.points.Add(pos);
                    sim.airTime = (i + f) * dt;
                    break;
                }
                sim.points.Add(pos);
            }

            sim.distance = pos.x;
            return sim;
        }

        private void DrawJump(JumpSim s, Vector3 origin, Color color, string label)
        {
            Gizmos.color = color;
            for (int i = 1; i < s.points.Count; i++)
                Gizmos.DrawLine(origin + (Vector3)s.points[i - 1], origin + (Vector3)s.points[i]);

#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(origin + (Vector3)s.apex + Vector3.up * 0.1f,
                $"{label}\nh={s.height:F2}  d={Mathf.Abs(s.distance):F2}  t={s.airTime:F2}s");
#endif
        }

        private void OnDrawGizmosSelected()
        {
            if (!_debugDrawJumps) return;

            ComputeJumpInfo();   // so Inspector tweaks show up without entering play mode
            float dt = Time.fixedDeltaTime;
            Vector3 o = transform.position;

            // 1) Pure vertical (no horizontal input), offset in x so the lines don't overlap
            DrawJump(SimulateJump(dt, 0f), o + Vector3.left * 0.4f, Color.red, "V tap");
            DrawJump(SimulateJump(_debugHoldTime, 0f), o, Color.yellow, "V hold");
            DrawJump(SimulateJump(999f, 0f), o + Vector3.right * 0.4f, Color.green, "V max");

            // 2) With horizontal movement: min (1-frame tap), variable (_debugHoldTime), max (full hold)
            DrawJump(SimulateJump(dt, _debugDirection), o, Color.red, "H tap");
            DrawJump(SimulateJump(_debugHoldTime, _debugDirection), o, Color.yellow, "H hold");
            DrawJump(SimulateJump(999f, _debugDirection), o, Color.green, "H max");
        }

        [ContextMenu("Print Jump Stats")]
        private void PrintJumpStats()
        {
            ComputeJumpInfo();
            float dt = Time.fixedDeltaTime;
            void Log(string n, JumpSim s) =>
                Debug.Log($"{n}: height={s.height:F2}  distance={Mathf.Abs(s.distance):F2}  airTime={s.airTime:F2}s");

            Log("Vertical tap", SimulateJump(dt, 0f));
            Log("Vertical hold", SimulateJump(_debugHoldTime, 0f));
            Log("Vertical max", SimulateJump(999f, 0f));
            Log("Horizontal tap", SimulateJump(dt, _debugDirection));
            Log("Horizontal hold", SimulateJump(_debugHoldTime, _debugDirection));
            Log("Horizontal max", SimulateJump(999f, _debugDirection));
        }

        #endregion
        private float GetGravity()
        {
            //Raising
            if(_velocity.y > 0)
            {
                //Early release
                if (!_jumpPressed)
                {
                    return _variableGravity;
                }
                return _jumpGravity;
            }
            return _fallGravity;
        }
        private void Jump(bool fromGround)
        {
            if (fromGround) _jumpedSinceGround = true;   // coyote can't give a second ground jump
            else _jumpCount++;

            _velocity.y = _jumpVelocity;
            _isGrounded = false;
            _bufferFrameCount = 0;
            _coyoteFrameCount = _maxFramesToCoyoteJump;
        }

        private void ComputeJumpInfo()
        {
            _jumpVelocity = ((2.0f * _jumpHeight) / _jumpTimeToPeak);
            _jumpGravity = ((-2.0f * _jumpHeight) / (_jumpTimeToPeak * _jumpTimeToPeak));
            _fallGravity = ((-2.0f * _jumpHeight) / (_jumpTimeToGround * _jumpTimeToGround));
            _variableGravity = -(_jumpVelocity * _jumpVelocity) / (2f * _variableJumpHeight);
        }
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position-transform.up * _castDistance, _boxSize);
        }
        private void Awake()
        {
            _rig = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            ComputeJumpInfo();
            _move = _playerActions.FindAction("Move");
            _jump = _playerActions.FindAction("Jump");
        }
        private void Update()
        {
            _moveInput = _move.ReadValue<Vector2>().normalized;
            if(_moveInput.x > 0)
            {
                _debugDirection = 1;
            }
            if (_moveInput.x < 0)
            {
                _debugDirection = -1;
            }
            //Update records the input, fixed update will clear it.
            if (_jump.WasPressedThisFrame())
            {
                _jumpJustPressed = true;
            }
            _jumpPressed = _jump.IsPressed();
            if (_deriveAcels)
            {
                _acel = _maxSpeed / _timeToMaxSpeed;
                _dcel = _maxSpeed / _timeToStop;
            }
            ComputeJumpInfo();
        }
        private void FixedUpdate()
        {
            /*
             * Character controller 2D platformer
             * 
             * Horizontal
             * - Acelerate to max speed
             * - Decelerate to zero
             * - Must feel snappy and responsive
             * - Decelerate must not be instant, but need to be fast
             * Vertical
             * - One ground jump
             * - One air jump(possible)
             * - Jump buffer
             * - Coyote time
             * - Terminal velocity
             * - Fast falling if pressing down
             * - On one ways platfomers, ignores them, can hold down to ignoret hem all
             * - Variable jump height
             */
            GatherColisions();
            HandleDirection();
            HandleGravity();
            HandleJump();
            FilterWalls();
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            bool rising = _velocity.y > 0f;
            var slide = new Rigidbody2D.SlideMovement
            {
                maxIterations = _slideIterations,
                surfaceUp = Vector2.up,
                surfaceSlideAngle = _maxSlopeAngle,
                gravity = Vector2.zero,
                surfaceAnchor = (_isGrounded && !rising) ? new Vector2(0f, -0.05f) : Vector2.zero,
            };

            Vector2 before = _rig.position;
            var result = _rig.Slide(_velocity, Time.fixedDeltaTime, slide);
            if (result.slideHit && Vector2.Dot(result.slideHit.normal, Vector2.down) > 0.9f)
                _velocity.y = Mathf.Min(_velocity.y, 0f);
        }
        private void FilterWalls()
        {
            int contactCount = _rig.GetContacts(_contacts);
            for (int i = 0; i < contactCount; i++)
            {
                Vector2 n = _contacts[i].normal;
                float angle = Vector2.Angle(Vector2.up, n);

                bool isWall = angle > _maxSlopeAngle && angle < 120f;   // not floor, not ceiling
                if (!isWall || _isOneWay) continue;

                float into = Vector2.Dot(_velocity, n);
                if (into < 0f)
                    _velocity -= n * into;
            }
        }
        private void HandleGravity()
        {
            if (_isGrounded && _velocity.y <= 0f)
            {
                _velocity.y = 0f;
                return;
            }
            _velocity.y += GetGravity() * Time.fixedDeltaTime;
            _velocity.y = Mathf.Clamp(_velocity.y, -_terminalVelocity, _terminalVelocity);
        }

        private void HandleDirection()
        {
            float targetDirX = _moveInput.x * _maxSpeed;
            float rate = Mathf.Abs(_moveInput.x) > 0.01f ? _acel : _dcel;
            _velocity.x = Mathf.MoveTowards(_velocity.x, _moveInput.x * _maxSpeed, rate * Time.fixedDeltaTime);
        }

        private void HandleJump()
        {
            if (!_jumpJustPressed) return;
            _jumpJustPressed = false;

            if (_isGrounded)
            {
                _velocity.y = _jumpVelocity;
                _isGrounded = false;
            }
            else if (_jumpCount < _maxJumps)   // air jumps
            {
                _jumpCount++;
                _velocity.y = _jumpVelocity;
            }
        }

        private void GatherColisions()
        {
            _isGrounded = false;
            _hitCelling = false;
            _isOneWay = false;

            int contactCount = _rig.GetContacts(_contacts);
            for (int i = 0; i < contactCount; i++)
            {
                Vector2 n = _contacts[i].normal;

                if (Vector2.Angle(Vector2.up, n) <= _maxSlopeAngle)
                {
                    _isGrounded = true;
                    _isOneWay = LayerMask.LayerToName(_contacts[i].collider.gameObject.layer) == "OneWay";
                }
                if (Vector2.Dot(n, Vector2.down) > 0.9f && !_isOneWay)
                    _hitCelling = true;
            }

            // Still touching the ground on the frame after a jump doesn't count as grounded
            if (_velocity.y > 0f) _isGrounded = false;   // rising = not grounded
            if (_isGrounded) _jumpCount = 0;             // landing refills air jumps
        }
    }
}
