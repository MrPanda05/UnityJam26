using UnityEngine;

namespace JamUnity.Commons.Components
{
    public class HitboxComponent : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            print(collision.name);
            if(collision.TryGetComponent<HurtboxComponent>(out HurtboxComponent hurtbox))
            {
                hurtbox.TakeHit();
            }
        }
    }
}
