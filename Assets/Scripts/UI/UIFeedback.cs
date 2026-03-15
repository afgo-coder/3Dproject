using UnityEngine;

namespace MiniMart.UI
{
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
}
