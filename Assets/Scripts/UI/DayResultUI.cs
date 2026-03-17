using MiniMart.Core;
using MiniMart.Data;
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

        private void Start()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (dayCycleManager != null)
            {
                dayCycleManager.DayEnded += ShowSummary;
            }

            RefreshSummary();
        }

        private void OnDisable()
        {
            if (dayCycleManager != null)
            {
                dayCycleManager.DayEnded -= ShowSummary;
            }
        }

        public void ShowSummary()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            RefreshSummary();
        }

        public void OnNextDayButtonClicked()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            dayCycleManager?.BeginNextDay();
        }

        public void RefreshSummary()
        {
            if (summaryText == null || EconomyManager.Instance == null)
            {
                return;
            }

            ProductData bestProduct = EconomyManager.Instance.GetBestSellingProduct(out int soldCount);
            string bestProductLine = bestProduct != null
                ? $"베스트 상품: {bestProduct.productName} ({soldCount}개 판매)"
                : "베스트 상품: 판매 기록 없음";
            string dayLine = GameManager.Instance != null ? $"Day {GameManager.Instance.CurrentDay}\n" : string.Empty;

            summaryText.text =
                $"{dayLine}" +
                $"오늘 매출: {EconomyManager.Instance.DailySales:N0}원\n" +
                $"오늘 비용: {EconomyManager.Instance.DailyCosts:N0}원\n" +
                $"오늘 순이익: {EconomyManager.Instance.DailyProfit:N0}원\n" +
                $"{bestProductLine}\n" +
                $"현재 보유 금액: {EconomyManager.Instance.CurrentMoney:N0}원";
        }
    }
}

