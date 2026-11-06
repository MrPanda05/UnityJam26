using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JamUnity.LePlayer
{

    public class Player : MonoBehaviour
    {
        private Rigidbody2D _rig;

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
        private bool _wasOnGround = false;
        private int _jumpCount = 0;
        private int _coyoteFrameCount = 0;
        private int _bufferFrameCount = 0;
        private bool _hasGroundJump = false;
        private bool _hasAirJump = false;
        private bool _countCoyoteFrames = false;
        private bool _countBufferFrames = false;

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
        private bool IsGrounded()
        {
            //If i'm moving up, i'm not grounded
            if (_velocity.y > 0) return false;
            return Physics2D.BoxCast(transform.position, _boxSize, 0, -transform.up, _castDistance, _masks);
        }
        private void Jump(bool grounded = true)
        {
            //Count jump only for air jumps
            if (!grounded)
            {
                _jumpCount++;
                _hasAirJump = true;
            }
            else
            {
                _hasGroundJump = true;
            }
            _velocity.y = _jumpVelocity;
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
            _velocity = _rig.linearVelocity;
            _wasOnGround = _isGrounded;
            _isGrounded = IsGrounded();
            //Horizontal
            float targetDirX = _moveInput.x * _maxSpeed;
            float rate = Mathf.Abs(_moveInput.x) > 0.01f ? _acel : _dcel;
            _velocity.x = Mathf.MoveTowards(_velocity.x, _moveInput.x * _maxSpeed, rate * Time.fixedDeltaTime);

            //Jump cases timers, outside any influence of air or ground
            if (_countBufferFrames)
            {
                if(_bufferFrameCount >= _maxFramesToBufferJump)
                {
                    print("Stopped buffer jump timer, fail to trigger it");
                    _bufferFrameCount = 0;
                    _countBufferFrames = false;
                    return;
                }
                _bufferFrameCount++;
            }
            if (_countCoyoteFrames)
            {
                if(_coyoteFrameCount >= _maxFramesToCoyoteJump)
                {
                    print("Stopped coyote timer, fail to use it");
                    _coyoteFrameCount = 0;
                    _countCoyoteFrames = false;
                    return;
                }
                _coyoteFrameCount++;
            }
            //air bone
            if (!_isGrounded)
            {
                _velocity.y += GetGravity() * Time.fixedDeltaTime;
                _velocity.y = Mathf.Clamp(_velocity.y, 0, _terminalVelocity);
                if (_wasOnGround && !_hasGroundJump)
                {
                    _countCoyoteFrames = true;
                    print("Starting coyote timer");
                }
                if (_jumpJustPressed)
                {
                    //Coyote time jump, counts as grounded
                    if(_countCoyoteFrames && _coyoteFrameCount < _maxFramesToCoyoteJump)
                    {
                        _jumpJustPressed = false;
                        _coyoteFrameCount = 0;
                        _countCoyoteFrames = false;
                        print("Coyote jump");
                        Jump();
                    }

                    //Normal air jump
                    if(_jumpCount < _maxJumps)
                    {
                        _jumpJustPressed = false;
                        print("Normal air jump");
                        Jump(false);
                    }
                    else
                    {
                        //Buffer jump
                        _countBufferFrames = true;
                        _jumpJustPressed = false;
                        print("Start to buffer jump timer");
                    }
                    
                }
            }
            //Ground
            if(_isGrounded)
            {
                _hasGroundJump = false;
                _hasAirJump = false;
                _velocity.y = 0;
                if (_jumpJustPressed && !_countBufferFrames)
                {
                    //clears jump request
                    _jumpJustPressed = false;
                    print("Normal ground jump");
                    Jump();
                }
                if (_countBufferFrames && _bufferFrameCount < _maxFramesToBufferJump)
                {
                    _jumpJustPressed = false;
                    print("Buffered jump");
                    Jump();
                }
            }
            _rig.linearVelocity = _velocity;
        }
    }
}
