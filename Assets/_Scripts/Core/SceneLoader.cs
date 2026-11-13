using JamUnity.UI.MenuUI;
using System;
using System.Threading.Tasks;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamUnity.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public bool IsLoading { get; private set; } = false;
        public bool LoadMain = false;

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
        public async Awaitable LoadSceneAsync(string newScene, bool setActive = false, bool showLoading = false)
        {
            if (SceneManager.GetSceneByName(newScene).isLoaded) return;
            IsLoading = true;
            try
            {
                var op = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
                if (op == null) return;
                if (showLoading)
                {
                    LoadingScreenManager.Instance.Show();
                }
                await op;
                if (setActive)
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(newScene));
                }
            }
            finally
            {
                if (showLoading)
                {
                    LoadingScreenManager.Instance.Hide();
                }
                IsLoading = false;
            }
        }
        public async Awaitable UnloadSceneAsync(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded) return;

            var op = SceneManager.UnloadSceneAsync(scene);
            if (op != null) await op;
        }

        public async Awaitable SwapAsync(string oldScene, string newScene)
        {
            await LoadSceneAsync(newScene, true);
            await UnloadSceneAsync(oldScene);
        }
        private async void Start()
        {
            if (!LoadMain) return;
            try
            {
                await LoadSceneAsync("LoadingScreen");
                await LoadSceneAsync("MainMenu");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
