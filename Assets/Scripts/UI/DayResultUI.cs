using MiniMart.Core;
using MiniMart.Data;
using MiniMart.Managers;
using MiniMart.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class DayResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private Text legacySummaryText;
        [SerializeField] private DayCycleManager dayCycleManager;

        private void Awake()
        {
            TryFindReferences();
        }

        private void Start()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (dayCycleManager == null)
            {
                dayCycleManager = FindFirstObjectByType<DayCycleManager>();
            }

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

            if (dayCycleManager == null)
            {
                dayCycleManager = FindFirstObjectByType<DayCycleManager>();
            }

            dayCycleManager?.BeginNextDay();
        }

        public void RefreshSummary()
        {
            TryFindReferences();

            if (EconomyManager.Instance == null)
            {
                return;
            }

            ProductData bestProduct = EconomyManager.Instance.GetBestSellingProduct(out int soldCount);
            string bestProductLine = bestProduct != null
                ? $"베스트 상품: {bestProduct.productName} ({soldCount}개 판매)"
                : "베스트 상품: 판매 기록 없음";

            string content = GameManager.Instance != null && GameManager.Instance.CurrentDay == 0
                ? BuildTutorialSummary(bestProductLine)
                : BuildNormalSummary(bestProductLine);

            UiTextUtility.SetText(summaryText, legacySummaryText, content);
        }

        private static string BuildTutorialSummary(string bestProductLine)
        {
            TutorialManager tutorialManager = TutorialManager.Instance;
            int completedCount = 0;
            if (tutorialManager != null)
            {
                if (tutorialManager.HasOpenedOrderTerminal) completedCount++;
                if (tutorialManager.HasPlacedOrder) completedCount++;
                if (tutorialManager.HasStockedShelf) completedCount++;
                if (tutorialManager.HasCompletedCheckout) completedCount++;
            }

            return
                "튜토리얼 완료!\n" +
                "기본 운영 흐름을 모두 익혔습니다.\n\n" +
                $"완료한 단계: {completedCount}/4\n" +
                $"응대한 손님: {EconomyManager.Instance.DailyCustomersServed}명\n" +
                $"{bestProductLine}\n" +
                $"현재 보유 금액: {EconomyManager.Instance.CurrentMoney:N0}원\n\n" +
                "다음 날부터 본격적인 편의점 운영이 시작됩니다.";
        }

        private static string BuildNormalSummary(string bestProductLine)
        {
            string dayLine = GameManager.Instance != null ? $"Day {GameManager.Instance.CurrentDay}\n" : string.Empty;
            return
                $"{dayLine}" +
                $"오늘 매출: {EconomyManager.Instance.DailySales:N0}원\n" +
                $"오늘 비용: {EconomyManager.Instance.DailyCosts:N0}원\n" +
                $"응대한 손님: {EconomyManager.Instance.DailyCustomersServed}명\n" +
                $"공병 회수: {EconomyManager.Instance.DailyBottleCount}개 (+{EconomyManager.Instance.DailyBottleReturnIncome:N0}원)\n" +
                $"목표 보너스: {EconomyManager.Instance.DailyGoalCompletedCount}개 달성 (+{EconomyManager.Instance.DailyGoalBonusIncome:N0}원)\n" +
                $"오늘 순이익: {EconomyManager.Instance.DailyProfit:N0}원\n" +
                $"{bestProductLine}\n" +
                $"현재 보유 금액: {EconomyManager.Instance.CurrentMoney:N0}원";
        }

        private void TryFindReferences()
        {
            if (panel == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    Transform panelTransform = canvas.transform.Find("Result");
                    if (panelTransform != null)
                    {
                        panel = panelTransform.gameObject;
                    }
                }
            }

            if (summaryText == null && legacySummaryText == null && panel != null)
            {
                UiTextUtility.TryAssignFromChild(panel.transform, "SummaryText", ref summaryText, ref legacySummaryText);
                UiTextUtility.TryAssignFromChild(panel.transform, "Text (Legacy)", ref summaryText, ref legacySummaryText);
            }
        }
    }
}
