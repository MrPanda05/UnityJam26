using JamUnity.Core;
using TMPro;
using UnityEngine;

namespace JamUnity.UI.MenuUI
{
    public class FinalTime : MonoBehaviour
    {
        private TMP_Text _timeText;
        private void Awake()
        {
            _timeText = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            _timeText.text = $"Final Time: {GameManager.Format(GameManager.Instance.FinalTime)}";
        }
    }
}
