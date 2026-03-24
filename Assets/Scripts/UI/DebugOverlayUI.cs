using MiniMart.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class DebugOverlayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text interactionPromptText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Text legacyInteractionPromptText;
        [SerializeField] private Text legacyStatusText;
        [SerializeField] private float statusDuration = 2.5f;

        private float statusTimer;

        private void Awake()
        {
            TryFindReferences();
            SetInteractionPrompt(string.Empty);
            ClearStatus();
        }

        private void Update()
        {
            if ((interactionPromptText == null && legacyInteractionPromptText == null) ||
                (statusText == null && legacyStatusText == null))
            {
                TryFindReferences();
            }

            bool isModalOpen = GameManager.Instance != null && GameManager.Instance.IsModalOpen;
            SetInteractionVisible(!isModalOpen && !string.IsNullOrWhiteSpace(GetCurrentInteractionText()));
            SetStatusVisible(!isModalOpen && !string.IsNullOrWhiteSpace(GetCurrentStatusText()));

            if (statusTimer <= 0f)
            {
                return;
            }

            statusTimer -= Time.unscaledDeltaTime;
            if (statusTimer <= 0f)
            {
                ClearStatus();
            }
        }

        public void SetInteractionPrompt(string message)
        {
            SetInteractionContent(message);
            SetInteractionVisible(
                !string.IsNullOrWhiteSpace(message) &&
                (GameManager.Instance == null || !GameManager.Instance.IsModalOpen));
        }

        public void ShowStatus(string message)
        {
            Debug.Log(message);
            SetStatusContent(message);
            SetStatusVisible(
                !string.IsNullOrWhiteSpace(message) &&
                (GameManager.Instance == null || !GameManager.Instance.IsModalOpen));
            statusTimer = statusDuration;
        }

        private void TryFindReferences()
        {
            if ((interactionPromptText == null && legacyInteractionPromptText == null) ||
                (statusText == null && legacyStatusText == null))
            {
                UiTextUtility.TryAssignFromCanvasChild("InteractionPromptText", ref interactionPromptText, ref legacyInteractionPromptText);
                UiTextUtility.TryAssignFromCanvasChild("StatusText", ref statusText, ref legacyStatusText);
            }
        }

        private void ClearStatus()
        {
            SetStatusContent(string.Empty);
            SetStatusVisible(false);
        }

        private void SetInteractionContent(string content)
        {
            UiTextUtility.TryAssignFromComponent(this, ref interactionPromptText, ref legacyInteractionPromptText);
            UiTextUtility.SetText(interactionPromptText, legacyInteractionPromptText, content);
        }

        private void SetStatusContent(string content)
        {
            UiTextUtility.SetText(statusText, legacyStatusText, content);
        }

        private string GetCurrentInteractionText()
        {
            return UiTextUtility.GetText(interactionPromptText, legacyInteractionPromptText);
        }

        private string GetCurrentStatusText()
        {
            return UiTextUtility.GetText(statusText, legacyStatusText);
        }

        private void SetInteractionVisible(bool visible)
        {
            UiTextUtility.SetVisible(interactionPromptText, legacyInteractionPromptText, visible);
        }

        private void SetStatusVisible(bool visible)
        {
            UiTextUtility.SetVisible(statusText, legacyStatusText, visible);
        }
    }
}
