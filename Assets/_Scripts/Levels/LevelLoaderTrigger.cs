using JamUnity.Core;
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            print("Loading next area");
            LoadScenes();
            UnLoadScenes();
        }

        private async void LoadScenes()
        {
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
                    await SceneLoader.Instance.LoadSceneAsync(_scenesToLoad[i], false, true);
                }
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
        }
    }
}
