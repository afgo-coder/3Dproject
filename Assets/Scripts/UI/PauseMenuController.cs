using MiniMart.Core;
using MiniMart.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniMart.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private string settingsSceneName = "SettingsScene";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Awake()
        {
            TryAutoBind();

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void Update()
        {
            if (panel == null)
            {
                TryAutoBind();
            }

            if (GameManager.Instance == null)
            {
                return;
            }

            bool isPauseOpen = panel != null && panel.activeInHierarchy;
            if (!GameManager.Instance.IsDayRunning && !isPauseOpen)
            {
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (isPauseOpen)
            {
                ResumeGame();
                return;
            }

            if (GameManager.Instance.IsModalOpen)
            {
                return;
            }

            Open();
        }

        public void Open()
        {
            if (panel == null || GameManager.Instance == null || !GameManager.Instance.IsDayRunning)
            {
                return;
            }

            if (GameManager.Instance.IsModalOpen && !panel.activeInHierarchy)
            {
                GameManager.Instance.SetModalOpen(false);
            }

            if (GameManager.Instance.IsModalOpen)
            {
                return;
            }

            panel.SetActive(true);
            GameManager.Instance.SetModalOpen(true);
            AudioManager.Instance?.PlayButtonClick();
        }

        public void ResumeGame()
        {
            if (panel == null || !panel.activeInHierarchy)
            {
                return;
            }

            panel.SetActive(false);
            GameManager.Instance?.SetModalOpen(false);
            AudioManager.Instance?.PlayButtonClick();
        }

        public void OpenSettings()
        {
            AudioManager.Instance?.PlayButtonClick();
            SettingsMenuController.SetReturnScene(SceneManager.GetActiveScene().name);
            SceneManager.LoadScene(settingsSceneName);
        }

        public void SaveAndReturnToMainMenu()
        {
            AudioManager.Instance?.PlayButtonClick();
            SaveManager.Instance?.SaveGame();
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void TryAutoBind()
        {
            if (panel != null)
            {
                return;
            }

            Transform pauseTransform = transform.Find("PausePanel");
            if (pauseTransform != null)
            {
                panel = pauseTransform.gameObject;
                return;
            }

            GameObject fallback = GameObject.Find("PausePanel");
            if (fallback != null)
            {
                panel = fallback;
            }
        }

        public void SaveAndQuit()
        {
            AudioManager.Instance?.PlayButtonClick();
            SaveManager.Instance?.SaveGame();
            Time.timeScale = 1f;
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
