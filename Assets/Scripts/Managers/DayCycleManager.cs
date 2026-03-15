using System;
using UnityEngine;

namespace MiniMart.Managers
{
    public class DayCycleManager : MonoBehaviour
    {
        [SerializeField] private float dayLengthSeconds = 420f;
        [SerializeField] private AnimationCurve customerSpawnMultiplierByTime = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);

        public float NormalizedTime { get; private set; }
        public bool IsRunning { get; private set; }
        public float SpawnRateMultiplier => customerSpawnMultiplierByTime.Evaluate(NormalizedTime);

        public event Action<float> TimeChanged;
        public event Action DayEnded;
        //임시 시작
        public void Start()
        {
            BeginDay();
        }

        public void BeginDay()
        {
            NormalizedTime = 0f;
            IsRunning = true;
            EconomyManager.Instance?.ResetDailySummary();
            TimeChanged?.Invoke(NormalizedTime);
        }

        private void Update()
        {
            if (!IsRunning || dayLengthSeconds <= 0f)
            {
                return;
            }

            NormalizedTime += Time.deltaTime / dayLengthSeconds;
            NormalizedTime = Mathf.Clamp01(NormalizedTime);
            TimeChanged?.Invoke(NormalizedTime);

            if (NormalizedTime >= 1f)
            {
                IsRunning = false;
                DayEnded?.Invoke();
            }
        }
    }
}
