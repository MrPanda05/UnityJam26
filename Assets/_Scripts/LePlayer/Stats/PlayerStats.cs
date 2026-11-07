using UnityEngine;

namespace JamUnity.LePlayer.Stats
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        public float maxSpeed;
        public float Acel => maxSpeed/timeToMaxSpeed;
        public float Decel => maxSpeed/ timeToStop;
        public float timeToMaxSpeed;
        public float timeToStop;
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
        
        public float JumpVelocity => ((2.0f * jumpHeight) / jumpTimeToPeak);
        public float JumpGravity => ((-2.0f * jumpHeight) / (jumpTimeToPeak * jumpTimeToPeak));
        public float FallGravity => ((-2.0f * jumpHeight) / (jumpTimeToGround * jumpTimeToGround));
        public float VariableGravity => -(JumpVelocity * JumpVelocity) / (2f * jumpHeightVariable);
    }
}
