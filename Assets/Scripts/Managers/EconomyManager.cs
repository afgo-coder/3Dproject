using System;
using System.Collections.Generic;
using MiniMart.Data;
using UnityEngine;

namespace MiniMart.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private int startingMoney = 30000;

        private readonly Dictionary<ProductData, int> dailyUnitsSold = new Dictionary<ProductData, int>();
        private bool dailyBottleSettlementApplied;

        public int CurrentMoney { get; private set; }
        public int DailySales { get; private set; }
        public int DailyCosts { get; private set; }
        public int DailyBottleCount { get; private set; }
        public int DailyBottleReturnIncome { get; private set; }
        public int DailyProfit => DailySales + DailyBottleReturnIncome - DailyCosts;

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

        public void RecordProductSale(ProductData product, int amount = 1)
        {
            if (product == null || amount <= 0)
            {
                return;
            }

            if (dailyUnitsSold.ContainsKey(product))
            {
                dailyUnitsSold[product] += amount;
            }
            else
            {
                dailyUnitsSold.Add(product, amount);
            }

            DailySummaryChanged?.Invoke();
        }

        public void ResetDailySummary()
        {
            DailySales = 0;
            DailyCosts = 0;
            DailyBottleCount = 0;
            DailyBottleReturnIncome = 0;
            dailyBottleSettlementApplied = false;
            dailyUnitsSold.Clear();
            DailySummaryChanged?.Invoke();
        }

        public void ApplyBottleReturnSettlement(int bottleCount, int bottleValue)
        {
            if (dailyBottleSettlementApplied)
            {
                return;
            }

            DailyBottleCount = Mathf.Max(0, bottleCount);
            DailyBottleReturnIncome = DailyBottleCount * Mathf.Max(0, bottleValue);
            CurrentMoney += DailyBottleReturnIncome;
            dailyBottleSettlementApplied = true;
            MoneyChanged?.Invoke(CurrentMoney);
            DailySummaryChanged?.Invoke();
        }

        public ProductData GetBestSellingProduct(out int soldCount)
        {
            soldCount = 0;
            ProductData bestProduct = null;

            foreach (KeyValuePair<ProductData, int> entry in dailyUnitsSold)
            {
                if (entry.Value > soldCount)
                {
                    bestProduct = entry.Key;
                    soldCount = entry.Value;
                }
            }

            return bestProduct;
        }
    }
}
