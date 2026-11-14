using JamUnity.Commons;
using JamUnity.Core;
using JamUnity.Enemies.Brains;
using JamUnity.Enemies.Stats;
using JamUnity.LePlayer;
using System;
using UnityEngine;

namespace JamUnity.Enemies
{
    public class BasicEnemy : MonoBehaviour
    {
        [field: SerializeField]
        public EnemyStats Stats { get; private set; }

        [SerializeField]
        private MonoBehaviour _brainScript;
        [field:SerializeField]
        public PlayerAnimPlayer AnimPlayer { get; private set; }
        [SerializeField]
        private ParticleSystem _deathParticles;
        [SerializeField]
        private SoundPlayer _deathSound;

        private Rigidbody2D _rig;
        private Collider2D _collider;
        public ContactPoint2D[] ContactPoints => _contacts;
        private ContactPoint2D[] _contacts = new ContactPoint2D[10];

        public IBrain MyBrain { get; private set; }

        private Vector2 _velocity;
        public bool IsGrounded { get; private set; }
        /// <summary>
        /// If true, it means that the enemy comes from a spawner
        /// </summary>
        public bool IsDisposable {  get; private set; }

        public event Action<BasicEnemy> OnDeath;
        public event Action<BasicEnemy> OnRespawn;

        public void AlertDeath()
        {
            OnDeath?.Invoke(this);
        }
        public void AlertRespawn()
        {
            OnRespawn?.Invoke(this);
        }


        public int GetCurrentContactPoints()
        {
            IsGrounded = false;
            int contactCount = _rig.GetContacts(_contacts);
            for (int i = 0; i < contactCount; i++)
            {
                Vector2 n = _contacts[i].normal;
                if (Vector2.Angle(Vector2.up, n) == 0)
                {
                    IsGrounded = true;
                }
            }
            if (_velocity.y > 0f) IsGrounded = false;
            return contactCount;
        }
        public void HandleGravity()
        {
            if (IsGrounded && _velocity.y <= 0f)
            {
                _velocity.y = 0f;
                return;
            }
            _velocity.y += Stats.gravity * Time.fixedDeltaTime;
        }

        public void MoveHorizontly(int dir)
        {
            if(_velocity.x > 0)
            {
                AnimPlayer.Flip(false);
            }
            if(_velocity.x < 0)
            {

                AnimPlayer.Flip(true);
            }
            _velocity.x = dir * Stats.speed;
        }
        public void ApplyMovement()
        {
            _rig.linearVelocity = _velocity;
        }
        
        private void Awake()
        {
            _rig = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            if (_brainScript != null)
            {
                MyBrain = _brainScript as IBrain;
            }
        }
        private void Start()
        {
            if (MyBrain != null)
            {
                MyBrain.StartThinking();
            }
        }

        private void FixedUpdate()
        {
            if (MyBrain != null)
            {
                MyBrain.Think();
            }
        }

        public virtual void Kill()
        {
            var instance = Instantiate(_deathParticles, transform.position, Quaternion.identity);
            SoundPlayerManager.Instance.PlaySound(_deathSound);
            gameObject.SetActive(false);
            AlertDeath();
        }
        public virtual void Respawn()
        {
            gameObject.SetActive(true);
            AlertRespawn();
        }
    }
}
