using MiniMart.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string settingsSceneName = "SettingsScene";
        [SerializeField] private Button continueButton;

        private void Start()
        {
            if (continueButton != null)
            {
                continueButton.interactable = SaveManager.HasSaveData;
            }
        }

        public void StartGame()
        {
            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            SaveManager.Instance?.PrepareForNewGame();
            SceneManager.LoadScene(gameSceneName);
        }

        public void ContinueGame()
        {
            if (!SaveManager.HasSaveData)
            {
                if (continueButton != null)
                {
                    continueButton.interactable = false;
                }

                return;
            }

            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            SaveManager.RequestContinueGame();
            SceneManager.LoadScene(gameSceneName);
        }

        public void OpenSettings()
        {
            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene(settingsSceneName);
        }

        public void QuitGame()
        {
            AudioManager.Instance?.PlayButtonClick();
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
