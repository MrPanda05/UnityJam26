using UnityEngine;

namespace JamUnity.LePlayer
{
    public class PlayerAnimPlayer : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private SpriteRenderer _sprite;


        public void Flip(bool value)
        {
            _sprite.flipX = value;
        }
        public void SetWalkingAnim()
        {
            _animator.SetBool("IsWalking", true);
            _animator.SetBool("IsIddle", false);
        }
        public void SetIddleAnim()
        {
            _animator.SetBool("IsWalking", false);
            _animator.SetBool("IsIddle", true);
        }
    }
}
