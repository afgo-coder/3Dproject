using MiniMart.Core;
using MiniMart.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MiniMart.UI
{
    internal static class UiTextUtility
    {
        public static bool HasReference(TMP_Text tmpText, Text legacyText)
        {
            return tmpText != null || legacyText != null;
        }

        public static void TryAssignFromComponent(Component owner, ref TMP_Text tmpText, ref Text legacyText)
        {
            if (owner == null || HasReference(tmpText, legacyText))
            {
                return;
            }

            tmpText = owner.GetComponent<TMP_Text>();
            legacyText = owner.GetComponent<Text>();
        }

        public static void TryAssignFromCanvasChild(string childName, ref TMP_Text tmpText, ref Text legacyText)
        {
            if (HasReference(tmpText, legacyText) || string.IsNullOrWhiteSpace(childName))
            {
                return;
            }

            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null)
                {
                    continue;
                }

                TryAssignFromChild(canvas.transform, childName, ref tmpText, ref legacyText);
                if (HasReference(tmpText, legacyText))
                {
                    return;
                }
            }
        }

        public static void TryAssignFromChild(Transform parent, string childName, ref TMP_Text tmpText, ref Text legacyText)
        {
            if (parent == null || HasReference(tmpText, legacyText) || string.IsNullOrWhiteSpace(childName))
            {
                return;
            }

            Transform target = parent.Find(childName);
            if (target == null)
            {
                Transform[] descendants = parent.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < descendants.Length; i++)
                {
                    if (descendants[i] != null && descendants[i].name == childName)
                    {
                        target = descendants[i];
                        break;
                    }
                }
            }

            if (target == null)
            {
                return;
            }

            tmpText = target.GetComponent<TMP_Text>();
            legacyText = target.GetComponent<Text>();
        }

        public static void SetText(TMP_Text tmpText, Text legacyText, string content)
        {
            if (tmpText != null)
            {
                tmpText.text = content;
            }

            if (legacyText != null)
            {
                legacyText.text = content;
            }
        }

        public static void SetVisible(TMP_Text tmpText, Text legacyText, bool visible)
        {
            if (tmpText != null)
            {
                tmpText.enabled = visible;
            }

            if (legacyText != null)
            {
                legacyText.enabled = visible;
            }
        }

        public static string GetText(TMP_Text tmpText, Text legacyText)
        {
            if (tmpText != null)
            {
                return tmpText.text;
            }

            return legacyText != null ? legacyText.text : string.Empty;
        }
    }

    public static class UIFeedback
    {
        public static void ShowStatus(string message)
        {
            Debug.Log(message);
            DebugOverlayUI overlay = Object.FindFirstObjectByType<DebugOverlayUI>();
            if (overlay != null)
            {
                overlay.ShowStatus(message);
            }
        }

        public static void SetInteractionPrompt(string message)
        {
            DebugOverlayUI overlay = Object.FindFirstObjectByType<DebugOverlayUI>();
            if (overlay != null)
            {
                overlay.SetInteractionPrompt(message);
            }
        }
    }

    public class StoreProgressUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Text legacyProgressText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureDriverExists()
        {
            if (Object.FindFirstObjectByType<StoreProgressUI>() != null)
            {
                return;
            }

            GameObject driver = new GameObject("StoreProgressUI");
            Object.DontDestroyOnLoad(driver);
            driver.AddComponent<StoreProgressUI>();
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            Transform[] descendants = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < descendants.Length; i++)
            {
                Transform current = descendants[i];
                if (current != null && current.name == childName)
                {
                    return current;
                }
            }

            return null;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            TryFindReferences();
            Refresh();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            if (StoreProgressionManager.Instance != null)
            {
                StoreProgressionManager.Instance.ProgressionChanged -= Refresh;
                StoreProgressionManager.Instance.ProgressionChanged += Refresh;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.DayChanged -= HandleGameStateChanged;
                GameManager.Instance.DayChanged += HandleGameStateChanged;
                GameManager.Instance.ModalStateChanged -= HandleModalStateChanged;
                GameManager.Instance.ModalStateChanged += HandleModalStateChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (StoreProgressionManager.Instance != null)
            {
                StoreProgressionManager.Instance.ProgressionChanged -= Refresh;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.DayChanged -= HandleGameStateChanged;
                GameManager.Instance.ModalStateChanged -= HandleModalStateChanged;
            }
        }

        private void Update()
        {
            if (!UiTextUtility.HasReference(progressText, legacyProgressText))
            {
                TryFindReferences();
            }

            Refresh();
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            progressText = null;
            legacyProgressText = null;
            TryFindReferences();
            Refresh();
        }

        private void HandleGameStateChanged(int _)
        {
            Refresh();
        }

        private void HandleModalStateChanged(bool _)
        {
            Refresh();
        }

        private void TryFindReferences()
        {
            UiTextUtility.TryAssignFromCanvasChild("StoreProgressText", ref progressText, ref legacyProgressText);
        }

        private void Refresh()
        {
            if (!UiTextUtility.HasReference(progressText, legacyProgressText))
            {
                TryFindReferences();
            }

            bool shouldHide = GameManager.Instance != null && GameManager.Instance.IsModalOpen;
            UiTextUtility.SetVisible(progressText, legacyProgressText, !shouldHide);
            if (shouldHide)
            {
                return;
            }

            if (StoreProgressionManager.Instance == null)
            {
                UiTextUtility.SetText(progressText, legacyProgressText, string.Empty);
                return;
            }

            UiTextUtility.SetText(
                progressText,
                legacyProgressText,
                StoreProgressionManager.Instance.GetOperationsSummary());
        }
    }
}
