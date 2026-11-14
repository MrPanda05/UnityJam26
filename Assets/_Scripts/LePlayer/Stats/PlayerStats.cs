using UnityEngine;

namespace JamUnity.LePlayer.Stats
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        public float groundedMaxSpeed;
        public float airMaxSpeed;
        public float GroundedAcel => groundedMaxSpeed/timeToMaxSpeedGround;
        public float GroundedDecel => groundedMaxSpeed/ timeToStopGround;
        public float AirAcel => airMaxSpeed / timeToMaxSpeedAir;
        public float AirDecel => airMaxSpeed / timeToStopAir;

        public float timeToMaxSpeedGround;
        public float timeToStopGround;

        public float timeToMaxSpeedAir;
        public float timeToStopAir;
        public float maxSlopAngle;
        [Header("Jump")]
        public int maxJumps;
        public int maxFramesToCoyote;
        public int maxFrameToBufferJump;
        public float jumpHeight;
        public float jumpTimeToPeak;
        public float jumpTimeToGround;
        public float jumpHeightVariable;
        public float terminalVelocity;
        [Header("Wall Jump")]
        [SerializeField] 
        public float wallJumpHorizontalSpeed = 12f;
        [SerializeField] 
        public float wallJumpLockTime = 0.15f;
        [Header("Enemy related")]
        public float stompKnockback;
        /// <summary>
        /// This will only be used to avoid dying while stomping
        /// </summary>
        [Header("Other")]
        public int maxFramesToDie = 5;
        
        public float JumpVelocity => ((2.0f * jumpHeight) / jumpTimeToPeak);
        public float JumpGravity => ((-2.0f * jumpHeight) / (jumpTimeToPeak * jumpTimeToPeak));
        public float FallGravity => ((-2.0f * jumpHeight) / (jumpTimeToGround * jumpTimeToGround));
        public float VariableGravity => -(JumpVelocity * JumpVelocity) / (2f * jumpHeightVariable);
    }
}
