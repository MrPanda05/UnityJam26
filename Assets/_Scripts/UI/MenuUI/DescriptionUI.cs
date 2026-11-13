using System;
using TMPro;
using UnityEngine;

namespace JamUnity.UI.MenuUI
{
    public class DescriptionUI : MonoBehaviour
    {
        [SerializeField]
        private DiffSelectorButton[] _buttons;
        [SerializeField]
        private TMP_Text _descText;

        private void Start()
        {
            foreach (var button in _buttons)
            {
                button.OnMouseHover += SetDescription;
                button.OnMouseExit += ResetDescription;
            }
        }

        private void ResetDescription()
        {
            _descText.text = string.Empty;
        }

        private void SetDescription(string obj)
        {
            _descText.text = obj;
        }

        private void OnDestroy()
        {
            foreach (var button in _buttons)
            {
                button.OnMouseHover -= SetDescription;
                button.OnMouseExit -= ResetDescription;
            }
        }
    }
}
