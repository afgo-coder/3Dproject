using MiniMart.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class DayResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text summaryText;
        [SerializeField] private DayCycleManager dayCycleManager;

        private void OnEnable()
        {
            if (dayCycleManager != null)
            {
                dayCycleManager.DayEnded += ShowSummary;
            }
        }

        private void OnDisable()
        {
            if (dayCycleManager != null)
            {
                dayCycleManager.DayEnded -= ShowSummary;
            }
        }

        private void ShowSummary()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (summaryText != null && EconomyManager.Instance != null)
            {
                summaryText.text =
                    $"Sales: {EconomyManager.Instance.DailySales:N0}\n" +
                    $"Costs: {EconomyManager.Instance.DailyCosts:N0}\n" +
                    $"Profit: {EconomyManager.Instance.DailyProfit:N0}";
            }
        }
    }
}
