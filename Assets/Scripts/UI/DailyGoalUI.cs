using MiniMart.Core;
using MiniMart.Interaction;
using MiniMart.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class DailyGoalUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private Text legacyGoalText;
        [SerializeField] private string title = "오늘 목표";
        [SerializeField] private int baseSalesTarget = 4000;
        [SerializeField] private int salesPerDay = 1500;
        [SerializeField] private int baseCustomersTarget = 2;
        [SerializeField] private int customersPerDay = 1;

        private void Awake()
        {
            TryFindReferences();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (goalText == null && legacyGoalText == null)
            {
                TryFindReferences();
            }

            if (EconomyManager.Instance == null)
            {
                return;
            }

            int day = GameManager.Instance != null ? Mathf.Max(0, GameManager.Instance.CurrentDay) : 0;
            bool shouldHide = day == 0 || (GameManager.Instance != null && GameManager.Instance.IsModalOpen);
            SetVisible(!shouldHide);

            if (shouldHide)
            {
                SetContent(string.Empty);
                return;
            }

            int salesTarget = baseSalesTarget + ((day - 1) * salesPerDay);
            int customersTarget = baseCustomersTarget + ((day - 1) * customersPerDay);
            int emptyShelves = CountEmptyAssignedShelves();

            SetContent(
                $"{title}\n" +
                $"{BuildLine(EconomyManager.Instance.DailySales >= salesTarget, $"매출 {salesTarget:N0}원 달성 ({EconomyManager.Instance.DailySales:N0}/{salesTarget:N0})")}\n" +
                $"{BuildLine(EconomyManager.Instance.DailyCustomersServed >= customersTarget, $"손님 {customersTarget}명 응대 ({EconomyManager.Instance.DailyCustomersServed}/{customersTarget})")}\n" +
                $"{BuildLine(emptyShelves == 0, $"품절 선반 0개 유지 (현재 {emptyShelves}개)")}");
        }

        private static int CountEmptyAssignedShelves()
        {
            Shelf[] shelves = Object.FindObjectsByType<Shelf>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int emptyCount = 0;
            for (int i = 0; i < shelves.Length; i++)
            {
                if (shelves[i] != null && shelves[i].AssignedProduct != null && shelves[i].CurrentStock <= 0)
                {
                    emptyCount++;
                }
            }

            return emptyCount;
        }

        private void TryFindReferences()
        {
            UiTextUtility.TryAssignFromComponent(this, ref goalText, ref legacyGoalText);
            UiTextUtility.TryAssignFromCanvasChild("GoalText", ref goalText, ref legacyGoalText);
        }

        private static string BuildLine(bool completed, string label)
        {
            return $"{(completed ? "■" : "□")} {label}";
        }

        private void SetContent(string content)
        {
            UiTextUtility.SetText(goalText, legacyGoalText, content);
        }

        private void SetVisible(bool visible)
        {
            UiTextUtility.SetVisible(goalText, legacyGoalText, visible);
        }
    }
}
