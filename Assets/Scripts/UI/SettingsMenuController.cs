using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniMart.UI
{
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        public void BackToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
