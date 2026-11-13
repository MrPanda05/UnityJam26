using System;
using UnityEngine;

namespace JamUnity.Commons.Components
{
    [Serializable]
    public enum HitType
    {
        None,
        Enemy,
        Player,
        KillBox
    }
    public class HitboxComponent : MonoBehaviour
    {
        [SerializeField]
        private HitType hitType;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.TryGetComponent<HurtboxComponent>(out HurtboxComponent hurtbox))
            {
                hurtbox.TakeHit(hitType);
            }
        }
    }
}
