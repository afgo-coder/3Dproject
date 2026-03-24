using MiniMart.Core;
using MiniMart.Interaction;
using MiniMart.Tutorial;
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
        public bool IsClosingTime { get; private set; }
        public bool IsPreparationDay => CurrentDay == 0;
        public int CurrentDay => GameManager.Instance != null ? GameManager.Instance.CurrentDay : 0;
        public float CurrentDayLengthSeconds { get; private set; }
        public float RemainingSeconds => Mathf.Max(0f, CurrentDayLengthSeconds * (1f - NormalizedTime));
        public float SpawnRateMultiplier => customerSpawnMultiplierByTime.Evaluate(NormalizedTime);

        public event System.Action<float> TimeChanged;
        public event System.Action DayEnded;

        private CustomerManager customerManager;

        private void Start()
        {
            _ = SaveManager.Instance;
            EnsureTutorialManagerExists();
            customerManager = FindFirstObjectByType<CustomerManager>();
            SubscribeClosingCallbacks();
            SubscribeTutorialCompletion();

            if (!SaveManager.IsLoadPending)
            {
                BeginPreparationDay();
            }
        }

        public void BeginPreparationDay()
        {
            TutorialManager tutorialManager = EnsureTutorialManagerExists();
            tutorialManager?.ResetTutorial();
            BeginDayInternal(0, preparationDayLengthSeconds);
            SaveManager.Instance?.SaveGame();
        }

        public void BeginNextDay()
        {
            int nextDay = CurrentDay <= 0 ? 1 : CurrentDay + 1;
            BeginDayInternal(nextDay, normalDayLengthSeconds);
            SaveManager.Instance?.SaveGame();
        }

        public void EndDayNow()
        {
            if (!IsRunning && !IsClosingTime)
            {
                return;
            }

            BeginClosingPhase();
        }

        public string GetRemainingTimeText()
        {
            int totalSeconds = Mathf.CeilToInt(RemainingSeconds);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        public void RestoreDayState(int dayNumber, float normalizedTime, bool wasRunning, bool wasClosingTime)
        {
            float length = dayNumber <= 0 ? preparationDayLengthSeconds : normalDayLengthSeconds;
            if (!wasRunning && !wasClosingTime)
            {
                RestoreEndedDayState(dayNumber, length);
                return;
            }

            BeginDayInternal(dayNumber, length);
            NormalizedTime = Mathf.Clamp01(normalizedTime);

            if (wasClosingTime)
            {
                NormalizedTime = 1f;
                IsRunning = false;
                IsClosingTime = true;
                customerManager ??= FindFirstObjectByType<CustomerManager>();
                SubscribeClosingCallbacks();
                customerManager?.BeginClosingTime();
            }

            TimeChanged?.Invoke(NormalizedTime);
        }

        private void BeginClosingPhase()
        {
            if (IsClosingTime)
            {
                return;
            }

            NormalizedTime = 1f;
            IsRunning = false;
            TimeChanged?.Invoke(NormalizedTime);
            IsClosingTime = true;
            customerManager ??= FindFirstObjectByType<CustomerManager>();
            SubscribeClosingCallbacks();

            if (customerManager != null)
            {
                customerManager.BeginClosingTime();
                return;
            }

            CompleteDayAfterCustomersExit();
        }

        private void BeginDayInternal(int dayNumber, float lengthSeconds)
        {
            CurrentDayLengthSeconds = Mathf.Max(1f, lengthSeconds);
            NormalizedTime = 0f;
            IsRunning = true;
            IsClosingTime = false;
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

            SubscribeTutorialCompletion();

            if (IsPreparationDay)
            {
                return;
            }

            NormalizedTime += Time.deltaTime / CurrentDayLengthSeconds;
            NormalizedTime = Mathf.Clamp01(NormalizedTime);
            TimeChanged?.Invoke(NormalizedTime);

            if (NormalizedTime >= 1f)
            {
                BeginClosingPhase();
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

        private static void SettleDailyGoalBonus(int day)
        {
            EconomyManager.Instance?.ApplyDailyGoalBonus(day);
        }

        private void SubscribeTutorialCompletion()
        {
            TutorialManager tutorialManager = EnsureTutorialManagerExists();
            if (tutorialManager != null)
            {
                tutorialManager.TutorialCompleted -= HandleTutorialCompleted;
                tutorialManager.TutorialCompleted += HandleTutorialCompleted;
            }
        }

        private void SubscribeClosingCallbacks()
        {
            if (customerManager == null)
            {
                return;
            }

            customerManager.AllCustomersExitedAfterClosing -= CompleteDayAfterCustomersExit;
            customerManager.AllCustomersExitedAfterClosing += CompleteDayAfterCustomersExit;
        }

        private static TutorialManager EnsureTutorialManagerExists()
        {
            if (TutorialManager.Instance != null)
            {
                return TutorialManager.Instance;
            }

            TutorialManager found = FindFirstObjectByType<TutorialManager>();
            if (found != null)
            {
                return found;
            }

            GameObject bootstrap = GameObject.Find("Bootstrap");
            if (bootstrap == null)
            {
                bootstrap = new GameObject("Bootstrap");
            }

            TutorialManager created = bootstrap.GetComponent<TutorialManager>();
            if (created == null)
            {
                created = bootstrap.AddComponent<TutorialManager>();
            }

            return created;
        }

        private void HandleTutorialCompleted()
        {
            if (!IsPreparationDay || !IsRunning)
            {
                return;
            }

            EndDayNow();
        }

        private void RestoreEndedDayState(int dayNumber, float lengthSeconds)
        {
            CurrentDayLengthSeconds = Mathf.Max(1f, lengthSeconds);
            NormalizedTime = 1f;
            IsRunning = false;
            IsClosingTime = false;
            GameManager.Instance?.BeginDay(dayNumber);
            GameManager.Instance?.EndDay();
            TimeChanged?.Invoke(NormalizedTime);
            DayEnded?.Invoke();
        }

        private void CompleteDayAfterCustomersExit()
        {
            if (!IsClosingTime)
            {
                return;
            }

            IsClosingTime = false;
            SettleBottleReturn();
            SettleDailyGoalBonus(CurrentDay);
            GameManager.Instance?.EndDay();
            SaveManager.Instance?.SaveGame();
            DayEnded?.Invoke();
        }
    }
}
