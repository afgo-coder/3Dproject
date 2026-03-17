using MiniMart.Core;
using MiniMart.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class DayTimerUI : MonoBehaviour
    {
        [SerializeField] private Text timerText;
        [SerializeField] private DayCycleManager dayCycleManager;
        [SerializeField] private string prefix = "남은 시간";

        private void Update()
        {
            if (timerText == null || dayCycleManager == null)
            {
                return;
            }

            bool isVisible = GameManager.Instance == null || GameManager.Instance.IsDayRunning;
            timerText.enabled = isVisible;
            if (!isVisible)
            {
                return;
            }

            timerText.text = $"{prefix}: {dayCycleManager.GetRemainingTimeText()}";
        }
    }
}
