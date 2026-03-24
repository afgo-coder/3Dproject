using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniMart.Managers
{
    public class AudioManager : MonoBehaviour
    {
        private const string BgmVolumeKey = "Audio.BgmVolume";
        private const string SfxVolumeKey = "Audio.SfxVolume";

        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Default Volume")]
        [SerializeField, Range(0f, 1f)] private float defaultBgmVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.8f;

        [Header("BGM")]
        [SerializeField] private AudioClip menuBgm;
        [SerializeField] private AudioClip gameplayBgm;
        [SerializeField] private string[] menuSceneNames = { "MainMenu", "SettingsScene" };

        [Header("SFX")]
        [SerializeField] private AudioClip buttonClickSfx;
        [SerializeField] private AudioClip orderPlacedSfx;
        [SerializeField] private AudioClip furniturePlacedSfx;
        [SerializeField] private AudioClip checkoutSfx;
        [SerializeField] private AudioClip expansionSfx;

        public float BgmVolume { get; private set; }
        public float SfxVolume { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureAudioSources();
            LoadVolumes();
            ApplyVolumes();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            PlaySceneBgm(SceneManager.GetActiveScene().name);
        }

        public void PlayButtonClick()
        {
            PlayOneShot(buttonClickSfx);
        }

        public void PlayOrderPlaced()
        {
            PlayOneShot(orderPlacedSfx);
        }

        public void PlayFurniturePlaced()
        {
            PlayOneShot(furniturePlacedSfx);
        }

        public void PlayCheckout()
        {
            PlayOneShot(checkoutSfx);
        }

        public void PlayExpansion()
        {
            PlayOneShot(expansionSfx);
        }

        public void SetBgmVolume(float volume)
        {
            BgmVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
            PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
            PlayerPrefs.Save();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlaySceneBgm(scene.name);
        }

        private void PlaySceneBgm(string sceneName)
        {
            if (bgmSource == null)
            {
                return;
            }

            AudioClip targetClip = IsMenuScene(sceneName) ? menuBgm : gameplayBgm;
            if (targetClip == null)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
                return;
            }

            if (bgmSource.clip == targetClip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = targetClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        private bool IsMenuScene(string sceneName)
        {
            if (menuSceneNames == null)
            {
                return false;
            }

            for (int i = 0; i < menuSceneNames.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(menuSceneNames[i]) && menuSceneNames[i] == sceneName)
                {
                    return true;
                }
            }

            return false;
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (sfxSource == null)
            {
                EnsureAudioSources();
            }

            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        private void EnsureAudioSources()
        {
            if (bgmSource == null)
            {
                GameObject bgmObject = new GameObject("BGM Source");
                bgmObject.transform.SetParent(transform, false);
                bgmSource = bgmObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
            }

            if (sfxSource == null)
            {
                GameObject sfxObject = new GameObject("SFX Source");
                sfxObject.transform.SetParent(transform, false);
                sfxSource = sfxObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
            }
        }

        private void LoadVolumes()
        {
            BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, defaultBgmVolume);
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = BgmVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = SfxVolume;
            }
        }

        private void OnValidate()
        {
            defaultBgmVolume = Mathf.Clamp01(defaultBgmVolume);
            defaultSfxVolume = Mathf.Clamp01(defaultSfxVolume);

            if (!Application.isPlaying)
            {
                return;
            }

            ApplyVolumes();
        }
    }
}
