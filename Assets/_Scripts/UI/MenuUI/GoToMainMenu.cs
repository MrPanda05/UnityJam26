using JamUnity.Core;
using UnityEngine;

namespace JamUnity.UI.MenuUI
{
    public class GoToMainMenu : MonoBehaviour
    {
        public void GoToMenu()
        {
            GameManager.Instance.GoToMainMenu();
        }
    }
}
