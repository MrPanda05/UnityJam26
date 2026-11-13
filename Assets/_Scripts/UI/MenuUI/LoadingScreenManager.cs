using JamUnity.LePlayer;
using System;
using UnityEngine;

namespace JamUnity.UI.MenuUI
{
    public class LoadingScreenManager : MonoBehaviour
    {
        public static LoadingScreenManager Instance {  get; private set; }
        [SerializeField]
        private GameObject _loadingScreen;

        public static event Action OnLoadingScreenStart;
        public static event Action OnLoadingScreenEnd;
        private void Awake()
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        public void Show()
        {
            OnLoadingScreenStart?.Invoke();
            _loadingScreen.SetActive(true);
        }
        public void Hide()
        {
            OnLoadingScreenEnd?.Invoke();
            _loadingScreen.SetActive(false);
        }
    }
}
