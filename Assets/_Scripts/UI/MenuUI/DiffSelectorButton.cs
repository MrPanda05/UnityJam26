using JamUnity.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JamUnity.UI.MenuUI
{
    public class DiffSelectorButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Diff _diffSetting;
        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private string _sceneToLoad = "Level1";
        [SerializeField]
        private string _description = "Description";

        public event Action<string> OnMouseHover;
        public event Action OnMouseExit;

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

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseHover?.Invoke(_description);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseExit?.Invoke();
        }
    }
}
