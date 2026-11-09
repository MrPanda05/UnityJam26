using System;
using UnityEngine;

namespace JamUnity.Commons.Components
{
    public class HurtboxComponent : MonoBehaviour
    {
        public event Action OnHitTaken;
        public void TakeHit()
        {
            print("Ouch");
            OnHitTaken?.Invoke();
        }
    }
}
