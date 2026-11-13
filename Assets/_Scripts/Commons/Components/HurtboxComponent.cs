using System;
using UnityEngine;

namespace JamUnity.Commons.Components
{
    public class HurtboxComponent : MonoBehaviour
    {
        public event Action<HitType> OnHitTaken;
        public void TakeHit(HitType type)
        {
            OnHitTaken?.Invoke(type);
        }
    }
}
