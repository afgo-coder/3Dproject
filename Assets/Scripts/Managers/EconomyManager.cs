using System;
using UnityEngine;

namespace MiniMart.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private int startingMoney = 30000;

        public int CurrentMoney { get; private set; }
        public int DailySales { get; private set; }
        public int DailyCosts { get; private set; }
        public int DailyProfit => DailySales - DailyCosts;

        public event Action<int> MoneyChanged;
        public event Action DailySummaryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CurrentMoney = startingMoney;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || CurrentMoney < amount)
            {
                return false;
            }

            CurrentMoney -= amount;
            DailyCosts += amount;
            MoneyChanged?.Invoke(CurrentMoney);
            DailySummaryChanged?.Invoke();
            return true;
        }

        public void AddSale(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentMoney += amount;
            DailySales += amount;
            MoneyChanged?.Invoke(CurrentMoney);
            DailySummaryChanged?.Invoke();
        }

        public void ResetDailySummary()
        {
            DailySales = 0;
            DailyCosts = 0;
            DailySummaryChanged?.Invoke();
        }
    }
}
