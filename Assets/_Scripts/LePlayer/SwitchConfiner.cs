using JamUnity.Levels;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace JamUnity.LePlayer
{
    public class SwitchConfiner : MonoBehaviour
    {
        private CinemachineConfiner2D _confiner;

        private void Awake()
        {
            _confiner = GetComponent<CinemachineConfiner2D>();
            LevelAreaEntry.OnLevelEntry += change;
        }

        private void change(Collider2D d)
        {
            _confiner.BoundingShape2D = d;
        }
    }
}
