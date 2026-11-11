using TMPro;
using UnityEngine;

namespace JamUnity.LePlayer
{
    public class HealthUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _liveText;

        public void SetLiveText(string text)
        {
            _liveText.text = text;
        }
    }
}
