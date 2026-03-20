using MiniMart.Core;
using MiniMart.Interaction;
using UnityEngine;

namespace MiniMart.Managers
{
    public class DayCycleManager : MonoBehaviour
    {
        [SerializeField] private float preparationDayLengthSeconds = 30f;
        [SerializeField] private float normalDayLengthSeconds = 420f;
        [SerializeField] private AnimationCurve customerSpawnMultiplierByTime = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);
        [SerializeField] private KeyCode debugEndDayKey = KeyCode.F8;
        [SerializeField] private bool allowDebugEndDayKey = true;

        public float NormalizedTime { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPreparationDay => CurrentDay == 0;
        public int CurrentDay => GameManager.Instance != null ? GameManager.Instance.CurrentDay : 0;
        public float CurrentDayLengthSeconds { get; private set; }
        public float RemainingSeconds => Mathf.Max(0f, CurrentDayLengthSeconds * (1f - NormalizedTime));
        public float SpawnRateMultiplier => customerSpawnMultiplierByTime.Evaluate(NormalizedTime);

        public event System.Action<float> TimeChanged;
        public event System.Action DayEnded;

        private void Start()
        {
            BeginPreparationDay();
        }

        public void BeginPreparationDay()
        {
            BeginDayInternal(0, preparationDayLengthSeconds);
        }

        public void BeginNextDay()
        {
            int nextDay = CurrentDay <= 0 ? 1 : CurrentDay + 1;
            BeginDayInternal(nextDay, normalDayLengthSeconds);
        }

        public void EndDayNow()
        {
            if (!IsRunning)
            {
                return;
            }

            NormalizedTime = 1f;
            IsRunning = false;
            SettleBottleReturn();
            GameManager.Instance?.EndDay();
            TimeChanged?.Invoke(NormalizedTime);
            DayEnded?.Invoke();
        }

        public string GetRemainingTimeText()
        {
            int totalSeconds = Mathf.CeilToInt(RemainingSeconds);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        private void BeginDayInternal(int dayNumber, float lengthSeconds)
        {
            CurrentDayLengthSeconds = Mathf.Max(1f, lengthSeconds);
            NormalizedTime = 0f;
            IsRunning = true;
            EconomyManager.Instance?.ResetDailySummary();
            GameManager.Instance?.BeginDay(dayNumber);
            TimeChanged?.Invoke(NormalizedTime);
        }

        private void Update()
        {
            if (allowDebugEndDayKey && Input.GetKeyDown(debugEndDayKey))
            {
                EndDayNow();
                return;
            }

            if (!IsRunning)
            {
                return;
            }

            NormalizedTime += Time.deltaTime / CurrentDayLengthSeconds;
            NormalizedTime = Mathf.Clamp01(NormalizedTime);
            TimeChanged?.Invoke(NormalizedTime);

            if (NormalizedTime >= 1f)
            {
                IsRunning = false;
                SettleBottleReturn();
                GameManager.Instance?.EndDay();
                DayEnded?.Invoke();
            }
        }

        private static void SettleBottleReturn()
        {
            TrashCan trashCan = FindFirstObjectByType<TrashCan>();
            if (trashCan != null)
            {
                trashCan.SettleDailyBottleReturn();
            }
        }
    }
}
