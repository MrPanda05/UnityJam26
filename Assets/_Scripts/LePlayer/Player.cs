using JamUnity.Commons;
using JamUnity.Commons.Components;
using JamUnity.Core;
using JamUnity.Enemies;
using JamUnity.Events;
using JamUnity.LePlayer.Stats;
using JamUnity.Levels;
using JamUnity.UI.MenuUI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JamUnity.LePlayer
{
    public enum PlayerStates
    {
        Alive,
        Dead,
        Paused,
        Other
    }
    public class Player : MonoBehaviour, IDiffRule
    {
        [SerializeField]
        private PlayerAnimPlayer _playerAnim;
        [field: SerializeField]
        public PlayerStats Stats { get; private set; }
        //Components
        private Rigidbody2D _rig;
        private Collider2D _collider;
        [SerializeField]
        private PlayerFeet _myFeet;
        [SerializeField]
        private HurtboxComponent _myHurtbox;
        [field: SerializeField]
        public PlayerStates CurrentState { get; private set; }
        //Colisions
        private ContactPoint2D[] _contacts = new ContactPoint2D[10];
        private ContactFilter2D _ceilingFilter;
        private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[4];
        private const float CeilingSkin = 0.02f;
        [SerializeField]
        private InputActionAsset _playerActions;
        
        private float _wallJumpLockTimer;

        [SerializeField]
        private LayerMask _oneWayLayer, _enemyLayer;
        [Header("Events")]
        [SerializeField]
        private RespawnEventChannel _respawnChannel;
        [Header("Particles")]
        [SerializeField]
        private ParticleSystem _doubleJumpParticles;
        [SerializeField]
        private ParticleSystem _deathParticles;
        [SerializeField]
        private TrailRenderer _trailRenderer;
        [Header("Sound effects")]
        [SerializeField]
        private SoundPlayer _groundJumpSFX;
        [SerializeField]
        private SoundPlayer _airJumpSFX;
        [SerializeField]
        private SoundPlayer _wallJumpSFX;
        [SerializeField]
        private SoundPlayer _stompSFX;
        [SerializeField]
        private SoundPlayer _deathSFX;

        //Inputs
        private InputAction _move;
        private InputAction _jump;
        private InputAction _down;
        private InputAction _restart;

        private Vector2 _moveInput;
        private bool _jumpRequested;
        private bool _jumpPressed;

        private int _playerLayer;
        private bool _ignoring;
        private int _bosta;

        private bool _isInside;

        //Movement
        private Vector2 _velocity;
        public Vector2 CurrentVelocity => _velocity;
        private bool _isGrounded;
        private bool _wasOnGround;
        private bool _isOnOneWay;
        private bool _hitCelling;
        private bool _isOnWall;
        private bool _countCoyote = false;
        private bool _countBufferJump = false;
        private bool _canCoyote = false;
        private bool _canBuffer = false;
        private bool _countDeathTimer = false;
        private bool _tookDamageFromEnemy = false;
        private int _jumpCount = 0;
        private int _frameCountForCoyote = 0;
        private int _frameCountForJumpBuffer = 0;
        private int _frameCountForDeath = 0;
        private Vector2 _curretnWallNormal;
        private int _currentLives = 7;
        public int CurrentLives => _currentLives;

        public static event Action OnRestart;
        public event Action OnDeath;
        //When life is zero
        public static event Action OnFullDeath;


        private void OnDrawGizmos()
        {
            if (_collider == null) return;
            Gizmos.color = _isInside ? Color.green : Color.red;
            Gizmos.DrawWireCube(_collider.bounds.center, _collider.bounds.size);
        }

        private void Awake()
        {
            LoadingScreenManager.OnLoadingScreenStart += HandleLoadingScreenStart;
            LoadingScreenManager.OnLoadingScreenEnd += HandleLoadingScreenEnd;
            _rig = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _move = _playerActions.FindAction("Move");
            _jump = _playerActions.FindAction("Jump");
            _down = _playerActions.FindAction("Down");
            _restart = _playerActions.FindAction("Restart");
            _playerLayer = gameObject.layer;
            _bosta = LayerMask.NameToLayer("OneWay");
            _ceilingFilter = new ContactFilter2D();
            _ceilingFilter.useTriggers = false;
            _ceilingFilter.SetLayerMask(~(_oneWayLayer | _enemyLayer)); // one-ways never count as a ceiling
            _respawnChannel.OnRespawn += HandleRespawn;
            WinFlag.OnGameWon += HandleGameWon;
            HandleDiff();


        }

        private void HandleGameWon()
        {
            _velocity = Vector2.zero;
            _rig.linearVelocity = Vector2.zero;
            CurrentState = PlayerStates.Other;
        }

        private void HandleLoadingScreenEnd()
        {
            CurrentState = PlayerStates.Alive;
        }

        private void HandleLoadingScreenStart()
        {
            CurrentState = PlayerStates.Paused;
        }

        private void HandleRespawn(Transform spawn)
        {
            transform.position = spawn.position;
            CurrentState = PlayerStates.Alive;
        }
        private void OnDestroy()
        {
            _respawnChannel.OnRespawn -= HandleRespawn;
            WinFlag.OnGameWon -= HandleGameWon;
            LoadingScreenManager.OnLoadingScreenStart -= HandleLoadingScreenStart;
            LoadingScreenManager.OnLoadingScreenEnd -= HandleLoadingScreenEnd;

        }
        private void Start()
        {
            _playerActions.Enable();
            if(GameManager.Instance != null)
            {
                GameManager.Instance.StartTimer();
            }
        }

        private void OnEnable()
        {
            _down.canceled += OnDownButtonCanceled;
            _myFeet.OnEnemyHeadHit += OnEnemyStomp;
            _myHurtbox.OnHitTaken += HandleHit;
        }

        private void HandleHit(HitType type)
        {
            if (CurrentState != PlayerStates.Alive) return;
            if(type == HitType.KillBox)
            {
                CurrentState = PlayerStates.Dead;
                _velocity = Vector2.zero;
                _rig.linearVelocity = Vector2.zero;
                StartCoroutine(DeathRoutine());
                return;
            }
            _countDeathTimer = true;
        }

        private void OnEnemyStomp(GameObject head)
        {
            if(_countDeathTimer && (_frameCountForDeath <= Stats.maxFramesToDie))
            {
                print("Saved due to stomp, would die otherwise");
                _countDeathTimer = false;
                _frameCountForDeath = 0;

            }
            AddUpwordForce(Stats.stompKnockback);
            //Regenerates a air jump
            if (_jumpCount > 0) _jumpCount--;
            if(head.transform.parent.gameObject.TryGetComponent<BasicEnemy>(out BasicEnemy enemy))
            {
                enemy.Kill();
            }
        }

        private void OnDisable()
        {
            _down.canceled -= OnDownButtonCanceled;
            _myFeet.OnEnemyHeadHit -= OnEnemyStomp;
            _myHurtbox.OnHitTaken -= HandleHit;

        }

        private void Update()
        {
            if (_restart.WasPressedThisFrame())
            {
                OnRestart?.Invoke();
            }
            switch (CurrentState)
            {
                case PlayerStates.Alive:
                GatherInputs();
                    break;
            }
        }
        public void AddUpwordForce(float force)
        {
            _velocity.y = force;
        }
        private void GatherInputs()
        {
            
            _moveInput = _move.ReadValue<Vector2>().normalized;
            if (_jump.WasPressedThisFrame())
            {
                _jumpRequested = true;
            }
            _jumpPressed = _jump.IsPressed();
        }

        private void HandleOneWay()
        {
            bool shouldIgnore = _down.IsPressed() || (_ignoring && _isInside);
            if (shouldIgnore == _ignoring)
            {
                //Just confirming that it is still ignoring
                Bounds b = _collider.bounds;
                _isInside = Physics2D.OverlapBox(b.center, b.size, 0f, _oneWayLayer);
            }
            _ignoring = shouldIgnore;
            Physics2D.IgnoreLayerCollision(_playerLayer, _bosta, shouldIgnore);
        }

        private void FixedUpdate()
        {
            switch (CurrentState)
            {
                case PlayerStates.Alive:
                    _wasOnGround = _isGrounded;
                    UpdateDeathTimer();
                    HandleSpecialJumps();
                    CheckColisions();
                    HandleJump();
                    HandleDirection();
                    HandleGravity();
                    CheckCeilingAhead();
                    ApplyMovement();
                    HandleOneWay();
                    break;
            }
        }

        private void UpdateDeathTimer()
        {
            if (_countDeathTimer)
            {
                _frameCountForDeath++;
                if(_frameCountForDeath > Stats.maxFramesToDie)
                {
                    //Edit thing here
                    CurrentState = PlayerStates.Dead;
                    _frameCountForDeath = 0;
                    _countDeathTimer = false;
                    _velocity = Vector2.zero;
                    _rig.linearVelocity = Vector2.zero;
                    StartCoroutine(DeathRoutine());
                }
            }
            else
            {
                _frameCountForDeath = 0;
            }
        }

        private IEnumerator DeathRoutine()
        {
            _currentLives--;
            _velocity = Vector2.zero;
            _rig.linearVelocity = Vector2.zero;
            _playerAnim.Hide(false);
            SoundPlayerManager.Instance.PlaySound(_deathSFX);
            var instance = Instantiate(_deathParticles, transform.position, Quaternion.identity);
            instance.transform.SetParent(gameObject.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.position = transform.position;
            _trailRenderer.enabled = false;
            yield return new WaitForSeconds(.8f);
            if (_currentLives <= 0)
            {
                OnFullDeath?.Invoke();
                yield return null;
            }
            OnDeath?.Invoke();
            yield return new WaitForSeconds(.2f);
            _playerAnim.Hide(true);
            _trailRenderer.enabled = true;
            OnRestart?.Invoke();

        }

        private void HandleSpecialJumps()
        {
            if (_wasOnGround && !_countCoyote)
            {
                _countCoyote = true;
                _frameCountForCoyote = 0;
            }
            if (_countCoyote)
            {
                _frameCountForCoyote++;
                if (_frameCountForCoyote > Stats.maxFramesToCoyote)
                {
                    _countCoyote = false;
                    _canCoyote = false;
                    _frameCountForCoyote = 0;
                }
                else
                {
                    _canCoyote = true;
                }
            }
            if (_countBufferJump)
            {
                _frameCountForJumpBuffer++;
                if (_frameCountForJumpBuffer > Stats.maxFrameToBufferJump)
                {
                    _countBufferJump = false;
                    _canBuffer = false;
                    _frameCountForJumpBuffer = 0;
                }
                else
                {
                    _canBuffer = true;
                }
            }
        }

        private float GetGravity()
        {
            //Raising
            if (_velocity.y > 0)
            {
                //Early release
                if (!_jumpPressed)
                {
                    return Stats.VariableGravity;
                }
                return Stats.JumpGravity;
            }
            return Stats.FallGravity;
        }

        private void CheckColisions()
        {
            _isGrounded = false;
            _hitCelling = false;
            _isOnOneWay = false;
            _isOnWall = false;
            _curretnWallNormal = Vector2.zero;
            int contactCount = _rig.GetContacts(_contacts);
            for (int i = 0; i < contactCount; i++)
            {
                Vector2 n = _contacts[i].normal;
                bool contactIsOneWay = _contacts[i].collider.gameObject.layer == _bosta;

                if (Vector2.Angle(Vector2.up, n) <= Stats.maxSlopAngle)
                {
                    _isGrounded = true;
                    _isOnOneWay = contactIsOneWay;
                }
                if (Vector2.Dot(n, Vector2.down) > 0.9f && !contactIsOneWay)
                    _hitCelling = true;
                if (Mathf.Abs(n.x) > 0.9f)
                {
                    _curretnWallNormal = n;
                    _isOnWall = true;
                }
            }
            if (_velocity.y > 0f) _isGrounded = false;
        }

        private void ApplyMovement()
        {
            if (_hitCelling)
            {
                _velocity.y = 0;
            }
            _rig.linearVelocity = _velocity;
        }

        private void HandleGravity()
        {
            if (_isGrounded && _velocity.y <= 0f)
            {
                _velocity.y = 0f;
                return;
            }
            _velocity.y += GetGravity() * Time.fixedDeltaTime;
            _velocity.y = Mathf.Clamp(_velocity.y, -Stats.terminalVelocity, Stats.terminalVelocity);
        }

        private void HandleJump()
        {

            if (_isGrounded)
            {
                _jumpCount = 0;
                if (_canBuffer)
                {
                    _jumpRequested = false;
                    SoundPlayerManager.Instance.PlaySound(_groundJumpSFX);
                    Jump(true);
                    //print("Buffer jump");
                    return;
                }
                if (_jumpRequested)
                {
                    _jumpRequested = false;
                    Jump(true);
                    SoundPlayerManager.Instance.PlaySound(_groundJumpSFX);

                    //print("Normal ground jump");
                }
                return;
            }
            //Coyote jump, count as ground jump
            if (_jumpRequested && _canCoyote)
            {
                _jumpRequested = false;
                Jump(true);
                SoundPlayerManager.Instance.PlaySound(_groundJumpSFX);

                //print("Coyote jump");
                return;
            }
            if (_jumpRequested)
            {
                //Being on wall resets jump count
                if(_isOnWall && !_isGrounded)
                {
                    _jumpRequested = false;
                    Jump(false, true);
                    SoundPlayerManager.Instance.PlaySound(_wallJumpSFX);

                    //print("WallJump");
                    return;

                }
                if ((_jumpCount < Stats.maxJumps) && !_isOnWall)
                {
                    _jumpRequested = false;
                    Jump();
                    SoundPlayerManager.Instance.PlaySound(_airJumpSFX);

                    var instance = Instantiate(_doubleJumpParticles, _myFeet.transform.position, Quaternion.identity);
                    instance.transform.SetParent(gameObject.transform, false);
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.position = _myFeet.transform.position;
                    //print("Normal air jump");
                }
                //Start jump buffer count
                else
                {
                    _countBufferJump = true;
                    _jumpRequested = false;
                }
            }
        }

        private void Jump(bool groundedJump = false, bool wallJump = false)
        {
            if (!groundedJump)
                _jumpCount++;

            if (wallJump)
            {
                _velocity.x = Mathf.Sign(_curretnWallNormal.x) * Stats.wallJumpHorizontalSpeed;
                _wallJumpLockTimer = Stats.wallJumpLockTime;
                _jumpCount--;
            }

            _velocity.y = Stats.JumpVelocity;
        }
        private void CheckCeilingAhead()
        {
            if (_velocity.y <= 0f) return;

            float dist = _velocity.y * Time.fixedDeltaTime + CeilingSkin;
            int count = _collider.Cast(Vector2.up, _ceilingFilter, _ceilingHits, dist);
            for (int i = 0; i < count; i++)
            {
                if (_ceilingHits[i].normal.y < -0.9f)
                {
                    _velocity.y = 0f;
                    _hitCelling = true;
                    return;
                }
            }
        }
        private void HandleDirection()
        {
            if (_velocity.x < 0)
            {
                _playerAnim.Flip(true);
            }
            if (_velocity.x > 0)
            {
                _playerAnim.Flip(false);

            }
            if (Mathf.Abs(_velocity.x) > 0.05f)
                _playerAnim.SetWalkingAnim();
            else
                _playerAnim.SetIddleAnim();
            if (_wallJumpLockTimer > 0f)
            {
                _wallJumpLockTimer -= Time.fixedDeltaTime;
                return; // keep the impulse untouched
            }
            float dirX = 0;
            float rate = 0;
            if (_isGrounded)
            {
                dirX = _moveInput.x * Stats.groundedMaxSpeed;
                rate = Mathf.Abs(_moveInput.x) > 0.01f ? Stats.GroundedAcel : Stats.GroundedDecel;
            }
            if (!_isGrounded)
            {
                dirX = _moveInput.x * Stats.airMaxSpeed;
                rate = Mathf.Abs(_moveInput.x) > 0.01f ? Stats.AirAcel : Stats.AirDecel;
            }
            _velocity.x = Mathf.MoveTowards(_velocity.x, dirX, rate * Time.fixedDeltaTime);
        }

        private void OnDownButtonCanceled(InputAction.CallbackContext context)
        {
            Bounds b = _collider.bounds;
            _isInside = Physics2D.OverlapBox(b.center, b.size, 0f, _oneWayLayer);
            if (_isInside)
                print("I'm inside a one way");
            else
                print("Not inside one way");
        }
        public void HandleDiff()
        {
            if (GameManager.Instance == null) return;
            switch (GameManager.Instance.CurrentGameDiff)
            {
                case Diff.Easy:
                    _currentLives = 9;
                    break;
                case Diff.Normal:
                    _currentLives = 7;
                    break;
                case Diff.Hard:
                    _currentLives = 4;
                    break;
            }
        }

        #region Debug - Jump Reach Preview
        // Everything in this region is debug only and does not touch the gameplay code above.
        // It SIMULATES a jump using the same order of operations as FixedUpdate
        // (Jump -> HandleDirection -> HandleGravity -> physics step) and the values in Stats.
        //
        // Select the Player in the Scene view to see the result:
        //   - Blue    : TAP jump  (jump released right away -> VariableGravity -> minimum)
        //   - Magenta : HOLD jump (held the whole way up    -> JumpGravity    -> maximum)
        //   - Solid line    = walking jump starting from standstill
        //   - Dashed-ish    = walking jump starting already at max speed
        //   - Vertical jump (no walking) is drawn in the same colours, straight up.
        // Right click the component header -> "Debug/Print Jump Reach" to log the numbers.

        [Header("Debug - Jump Reach Preview")]
        [SerializeField] private bool _debugDrawJumpReach = true;
        [SerializeField] private bool _debugDrawVertical = true;
        [SerializeField] private bool _debugDrawWalking = true;
        [SerializeField] private bool _debugWalkRight = true;
        [SerializeField, Min(0.001f)] private float _debugStepsPerFixed = 1f; // kept at 1 to match FixedUpdate
        [SerializeField] private Color _debugMinColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color _debugMaxColor = new Color(1f, 0.2f, 0.9f, 1f);

        private struct DebugJumpResult
        {
            public float Height;           // highest point above the start
            public float Distance;         // horizontal distance when back at start height
            public float AirTime;          // seconds in the air
            public System.Collections.Generic.List<Vector2> Path;
        }

        // holdJump:   true = jump button held the whole ascent, false = single tap
        // walking:    true = holding left/right from the moment of the jump
        // startSpeedFraction: 0 = start from standstill, 1 = start already at max speed
        private DebugJumpResult DebugSimulateJump(bool holdJump, bool walking, float startSpeedFraction)
        {
            DebugJumpResult result = new DebugJumpResult
            {
                Path = new System.Collections.Generic.List<Vector2>()
            };
            if (Stats == null) return result;

            float dt = Time.fixedDeltaTime / Mathf.Max(0.001f, _debugStepsPerFixed);
            Vector2 pos = Vector2.zero;
            Vector2 vel = new Vector2(walking ? Stats.groundedMaxSpeed * startSpeedFraction : 0f, Stats.JumpVelocity);
            float targetX = walking ? Stats.groundedMaxSpeed : 0f;
            float rate = walking ? Stats.GroundedAcel : Stats.GroundedDecel;

            result.Path.Add(pos);
            const int maxSteps = 20000;
            for (int i = 0; i < maxSteps; i++)
            {
                // HandleDirection
                vel.x = Mathf.MoveTowards(vel.x, targetX, rate * dt);

                // HandleGravity (GetGravity uses the velocity BEFORE it is updated)
                float g;
                if (vel.y > 0f) g = holdJump ? Stats.JumpGravity : Stats.VariableGravity;
                else g = Stats.FallGravity;
                vel.y += g * dt;
                vel.y = Mathf.Clamp(vel.y, -Stats.terminalVelocity, Stats.terminalVelocity);

                // Physics step
                Vector2 prev = pos;
                pos += vel * dt;
                result.AirTime += dt;

                if (pos.y > result.Height) result.Height = pos.y;

                // Landed back on the start height: interpolate the exact crossing
                if (pos.y <= 0f && vel.y <= 0f)
                {
                    float t = prev.y / (prev.y - pos.y);
                    pos = Vector2.Lerp(prev, pos, Mathf.Clamp01(t));
                    pos.y = 0f;
                    result.Path.Add(pos);
                    break;
                }
                result.Path.Add(pos);
            }

            result.Distance = pos.x;
            return result;
        }

        private void OnDrawGizmosSelected()
        {
            if (!_debugDrawJumpReach || Stats == null) return;
            if (_curretnWallNormal != Vector2.zero)
            {
                Vector3 origins = _collider != null ? _collider.bounds.center : transform.position;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(origins, origins + (Vector3)_curretnWallNormal);
            }
            if (_moveInput.x < 0)
            {
                _debugWalkRight = false;
            }
            if(_moveInput.x > 0)
            {
                _debugWalkRight = true;
            }
            Vector2 origin = _collider != null
                ? new Vector2(_collider.bounds.center.x, _collider.bounds.min.y)
                : (Vector2)transform.position;
            float dirSign = _debugWalkRight ? 1f : -1f;

            if (_debugDrawVertical)
            {
                DebugDrawResult(DebugSimulateJump(false, false, 0f), origin, 1f, _debugMinColor, "Tap (vertical)");
                DebugDrawResult(DebugSimulateJump(true, false, 0f), origin, 1f, _debugMaxColor, "Hold (vertical)");
            }

            if (_debugDrawWalking)
            {
                DebugDrawResult(DebugSimulateJump(false, true, 0f), origin, dirSign, _debugMinColor, "Tap + walk (from stop)");
                DebugDrawResult(DebugSimulateJump(true, true, 0f), origin, dirSign, _debugMaxColor, "Hold + walk (from stop)");

                Color minFast = _debugMinColor; minFast.a = 0.45f;
                Color maxFast = _debugMaxColor; maxFast.a = 0.45f;
                DebugDrawResult(DebugSimulateJump(false, true, 1f), origin, dirSign, minFast, "Tap + walk (full speed)");
                DebugDrawResult(DebugSimulateJump(true, true, 1f), origin, dirSign, maxFast, "Hold + walk (full speed)");
            }
        }

        private void DebugDrawResult(DebugJumpResult r, Vector2 origin, float dirSign, Color color, string label)
        {
            if (r.Path == null || r.Path.Count < 2) return;

            Gizmos.color = color;
            for (int i = 1; i < r.Path.Count; i++)
            {
                Vector2 a = origin + new Vector2(r.Path[i - 1].x * dirSign, r.Path[i - 1].y);
                Vector2 b = origin + new Vector2(r.Path[i].x * dirSign, r.Path[i].y);
                Gizmos.DrawLine(a, b);
            }

            // Landing marker + apex marker
            Vector2 land = origin + new Vector2(r.Distance * dirSign, 0f);
            Gizmos.DrawWireSphere(land, 0.08f);

            Vector2 apex = origin;
            float best = float.MinValue;
            for (int i = 0; i < r.Path.Count; i++)
            {
                if (r.Path[i].y > best)
                {
                    best = r.Path[i].y;
                    apex = origin + new Vector2(r.Path[i].x * dirSign, r.Path[i].y);
                }
            }
            Gizmos.DrawWireCube(apex, Vector3.one * 0.1f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(
                apex + Vector2.up * 0.15f,
                $"{label}\nH: {r.Height:0.00}  D: {r.Distance:0.00}  T: {r.AirTime:0.00}s");
#endif
        }

        [ContextMenu("Debug/Print Jump Reach")]
        private void DebugPrintJumpReach()
        {
            if (Stats == null)
            {
                Debug.LogWarning("[JumpReach] Stats is not assigned.");
                return;
            }

            DebugJumpResult vMin = DebugSimulateJump(false, false, 0f);
            DebugJumpResult vMax = DebugSimulateJump(true, false, 0f);
            DebugJumpResult wMin = DebugSimulateJump(false, true, 0f);
            DebugJumpResult wMax = DebugSimulateJump(true, true, 0f);
            DebugJumpResult fMin = DebugSimulateJump(false, true, 1f);
            DebugJumpResult fMax = DebugSimulateJump(true, true, 1f);

            Debug.Log(
                "[JumpReach] (world units, single ground jump)\n" +
                $"Vertical            -> Min H: {vMin.Height:0.00}  | Max H: {vMax.Height:0.00}\n" +
                $"Walk (from stop)    -> Min H: {wMin.Height:0.00} D: {wMin.Distance:0.00} T: {wMin.AirTime:0.00}s  | " +
                $"Max H: {wMax.Height:0.00} D: {wMax.Distance:0.00} T: {wMax.AirTime:0.00}s\n" +
                $"Walk (full speed)   -> Min H: {fMin.Height:0.00} D: {fMin.Distance:0.00} T: {fMin.AirTime:0.00}s  | " +
                $"Max H: {fMax.Height:0.00} D: {fMax.Distance:0.00} T: {fMax.AirTime:0.00}s",
                this);
        }

        #endregion
    }
}