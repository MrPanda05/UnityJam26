using JamUnity.Core;
using TMPro;
using UnityEngine;

namespace JamUnity.LePlayer
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _timerText;

        public void SetLiveText(string text)
        {
            _timerText.text = text;
        }

        private void LateUpdate()
        {
            if (_timerText != null)
            {
                if (GameManager.Instance == null) return;
                SetLiveText(GameManager.Format(GameManager.Instance.ElapsedTime));
            }

        }
    }
}
