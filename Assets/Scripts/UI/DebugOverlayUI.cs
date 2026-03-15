using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class DebugOverlayUI : MonoBehaviour
    {
        [SerializeField] private Text interactionPromptText;
        [SerializeField] private Text statusText;
        [SerializeField] private float statusDuration = 2.5f;

        private float _statusTimer;

        private void Awake()
        {
            SetInteractionPrompt(string.Empty);
            ClearStatus();
        }

        private void Update()
        {
            if (_statusTimer <= 0f)
            {
                return;
            }

            _statusTimer -= Time.unscaledDeltaTime;
            if (_statusTimer <= 0f)
            {
                ClearStatus();
            }
        }

        public void SetInteractionPrompt(string message)
        {
            if (interactionPromptText == null)
            {
                return;
            }

            interactionPromptText.text = message;
            interactionPromptText.enabled = !string.IsNullOrWhiteSpace(message);
        }

        public void ShowStatus(string message)
        {
            Debug.Log(message);

            if (statusText == null)
            {
                return;
            }

            statusText.text = message;
            statusText.enabled = !string.IsNullOrWhiteSpace(message);
            _statusTimer = statusDuration;
        }

        private void ClearStatus()
        {
            if (statusText == null)
            {
                return;
            }

            statusText.text = string.Empty;
            statusText.enabled = false;
        }
    }
}
