using MiniMart.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class SettingsMenuController : MonoBehaviour
    {
        private static string requestedReturnSceneName;

        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text bgmValueTmpText;
        [SerializeField] private TMP_Text sfxValueTmpText;
        [SerializeField] private Text bgmValueText;
        [SerializeField] private Text sfxValueText;

        private bool isInitializing;

        private void Start()
        {
            isInitializing = true;

            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                if (bgmSlider != null)
                {
                    bgmSlider.SetValueWithoutNotify(audioManager.BgmVolume);
                }

                if (sfxSlider != null)
                {
                    sfxSlider.SetValueWithoutNotify(audioManager.SfxVolume);
                }
            }

            RegisterSliderCallbacks();
            RefreshVolumeTexts();
            isInitializing = false;
        }

        public static void SetReturnScene(string sceneName)
        {
            requestedReturnSceneName = sceneName;
        }

        public void BackToMainMenu()
        {
            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            string targetScene = string.IsNullOrWhiteSpace(requestedReturnSceneName)
                ? mainMenuSceneName
                : requestedReturnSceneName;
            requestedReturnSceneName = null;
            SceneManager.LoadScene(targetScene);
        }

        public void OnBgmSliderChanged(float value)
        {
            if (isInitializing)
            {
                return;
            }

            AudioManager.Instance?.SetBgmVolume(value);
            RefreshVolumeTexts();
        }

        public void OnSfxSliderChanged(float value)
        {
            if (isInitializing)
            {
                return;
            }

            AudioManager.Instance?.SetSfxVolume(value);
            RefreshVolumeTexts();
        }

        private void RegisterSliderCallbacks()
        {
            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
                bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
                sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            }
        }

        private void RefreshVolumeTexts()
        {
            string bgmText = bgmSlider != null ? $"BGM: {Mathf.RoundToInt(bgmSlider.value * 100f)}%" : string.Empty;
            string sfxText = sfxSlider != null ? $"SFX: {Mathf.RoundToInt(sfxSlider.value * 100f)}%" : string.Empty;

            if (bgmValueTmpText != null)
            {
                bgmValueTmpText.text = bgmText;
            }

            if (bgmValueText != null && bgmSlider != null)
            {
                bgmValueText.text = bgmText;
            }

            if (sfxValueTmpText != null)
            {
                sfxValueTmpText.text = sfxText;
            }

            if (sfxValueText != null && sfxSlider != null)
            {
                sfxValueText.text = sfxText;
            }
        }
    }
}
