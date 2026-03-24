using MiniMart.Core;
using MiniMart.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            TryAssignFromChild(canvas.transform, childName, ref tmpText, ref legacyText);
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstanceAfterSceneLoad()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            Transform target = canvas.transform.Find("StoreProgressText");
            if (target == null)
            {
                return;
            }

            if (target.GetComponent<StoreProgressUI>() == null)
            {
                target.gameObject.AddComponent<StoreProgressUI>();
            }
        }

        private void Awake()
        {
            TryFindReferences();
            Refresh();
        }

        private void OnEnable()
        {
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
            UiTextUtility.TryAssignFromComponent(this, ref progressText, ref legacyProgressText);
            UiTextUtility.TryAssignFromCanvasChild("StoreProgressText", ref progressText, ref legacyProgressText);
        }

        private void Refresh()
        {
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
