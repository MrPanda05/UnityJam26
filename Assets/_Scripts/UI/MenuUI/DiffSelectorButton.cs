using JamUnity.Core;
using TMPro;
using UnityEngine;

namespace JamUnity.UI.MenuUI
{
    public class DiffSelectorButton : MonoBehaviour
    {
        [SerializeField]
        private Diff _diffSetting;
        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private string _sceneToLoad = "Level1";

        private void Awake()
        {
            _text.text = _diffSetting.ToString();
        }
        public async void LoadGame()
        {
            GameManager.Instance.SetDiff(_diffSetting);
            await SceneLoader.Instance.LoadSceneAsync(_sceneToLoad, true);
            await SceneLoader.Instance.UnloadSceneAsync("MainMenu");
            await SceneLoader.Instance.LoadSceneAsync("PlayerScene");
        }
    }
}
