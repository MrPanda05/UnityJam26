using System;
using UnityEngine;

namespace JamUnity.Levels
{
    public class EnableAfterLoad : MonoBehaviour
    {

        [SerializeField]
        private GameObject[] _objectsToEnable;
        [SerializeField]
        private LevelLoaderTrigger _trigger;
        [Tooltip("If true will load objects after loading, else will load after unloading")]
        [SerializeField]
        private bool _enableOnLoad = true;
        private void Start()
        {
            if(_trigger == null)
            {
                Debug.LogError("Trigger must not be null");
                return;
            }
            if (_enableOnLoad)
            {
                _trigger.OnLevelsLoaded += LoadObjects;
            }
            else
            {
                _trigger.OnLevelsUnLoaded += LoadObjects;
            }
        }

        private void LoadObjects()
        {
            foreach (var item in _objectsToEnable)
            {
                item.SetActive(true);
            }
        }
    }
}
