using UnityEngine;

namespace JamUnity.UI.MenuUI
{
    public class QuitButton : MonoBehaviour
    {
        private void Start()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                this.gameObject.SetActive(false);
            }
        }
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
