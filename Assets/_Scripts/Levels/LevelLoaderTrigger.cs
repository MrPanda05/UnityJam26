using JamUnity.Core;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamUnity.Levels
{
    public class LevelLoaderTrigger : MonoBehaviour
    {

        [SerializeField]
        private string[] _scenesToUnLoad;
        [SerializeField]
        private string[] _scenesToLoad;
        public event Action OnLevelsLoaded;
        public event Action OnLevelsUnLoaded;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            print("Loading next area");
            LoadScenes();
            UnLoadScenes();
        }

        private async void LoadScenes()
        {
            bool _hasAtLeastOneLevelLoaded = false;
            for (int i = 0; i < _scenesToLoad.Length; i++)
            {
                bool isSceneLoaded = false;
                for (int j = 0; j < SceneManager.sceneCount; j++)
                {
                    var scene = SceneManager.GetSceneAt(j);
                    if(scene.name == _scenesToLoad[i])
                    {
                        isSceneLoaded = true;
                        break;
                    }
                }

                if (!isSceneLoaded)
                {
                    _hasAtLeastOneLevelLoaded = true;
                    await SceneLoader.Instance.LoadSceneAsync(_scenesToLoad[i], false, true);
                }
            }
            if (_hasAtLeastOneLevelLoaded)
            {
                OnLevelsLoaded?.Invoke();
            }
        }
        private async void UnLoadScenes()
        {
            for (int i = 0; i < _scenesToUnLoad.Length; i++)
            {
                for (int j = 0; j < SceneManager.sceneCount; j++)
                {
                    var scene = SceneManager.GetSceneAt(j);
                    if (scene.name == _scenesToUnLoad[i])
                    {
                        await SceneLoader.Instance.UnloadSceneAsync(_scenesToUnLoad[i]);
                    }
                }
            }
            OnLevelsUnLoaded?.Invoke();
        }
    }
}
