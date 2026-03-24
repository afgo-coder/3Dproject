using MiniMart.Core;
using MiniMart.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class DayTimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Text legacyTimerText;
        [SerializeField] private DayCycleManager dayCycleManager;
        [SerializeField] private string prefix = "남은 시간";

        private void Awake()
        {
            TryFindReferences();
        }

        private void Update()
        {
            if (dayCycleManager == null)
            {
                dayCycleManager = FindFirstObjectByType<DayCycleManager>();
            }

            if (timerText == null && legacyTimerText == null)
            {
                TryFindReferences();
            }

            if (dayCycleManager == null)
            {
                return;
            }

            bool isVisible = (GameManager.Instance == null || GameManager.Instance.IsDayRunning) &&
                             !dayCycleManager.IsPreparationDay &&
                             !dayCycleManager.IsClosingTime;
            SetVisible(isVisible);
            if (!isVisible)
            {
                return;
            }

            SetContent($"{prefix}: {dayCycleManager.GetRemainingTimeText()}");
        }

        private void TryFindReferences()
        {
            UiTextUtility.TryAssignFromComponent(this, ref timerText, ref legacyTimerText);
            UiTextUtility.TryAssignFromCanvasChild("DayTimerText", ref timerText, ref legacyTimerText);
        }

        private void SetVisible(bool visible)
        {
            UiTextUtility.SetVisible(timerText, legacyTimerText, visible);
        }

        private void SetContent(string content)
        {
            UiTextUtility.SetText(timerText, legacyTimerText, content);
        }
    }
}
