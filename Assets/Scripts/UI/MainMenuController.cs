using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniMart.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string settingsSceneName = "SettingsScene";

        public void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        public void OpenSettings()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(settingsSceneName);
        }

        public void QuitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
