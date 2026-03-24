using System;
using System.Collections.Generic;
using MiniMart.Data;
using MiniMart.Interaction;
using UnityEngine;

namespace MiniMart.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private int startingMoney = 30000;

        private readonly Dictionary<ProductData, int> dailyUnitsSold = new Dictionary<ProductData, int>();
        private bool dailyBottleSettlementApplied;
        private bool dailyGoalBonusApplied;

        public int CurrentMoney { get; private set; }
        public int DailySales { get; private set; }
        public int DailyCosts { get; private set; }
        public int DailyBottleCount { get; private set; }
        public int DailyBottleReturnIncome { get; private set; }
        public int DailyGoalCompletedCount { get; private set; }
        public int DailyGoalBonusIncome { get; private set; }
        public int DailyCustomersServed { get; private set; }
        public int DailyProfit => DailySales + DailyBottleReturnIncome + DailyGoalBonusIncome - DailyCosts;

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
            StoreProgressionManager.Instance?.RegisterSales(amount);
            MoneyChanged?.Invoke(CurrentMoney);
            DailySummaryChanged?.Invoke();
        }

        public void RestoreMoney(int amount)
        {
            CurrentMoney = Mathf.Max(0, amount);
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
            DailyGoalCompletedCount = 0;
            DailyGoalBonusIncome = 0;
            DailyCustomersServed = 0;
            dailyBottleSettlementApplied = false;
            dailyGoalBonusApplied = false;
            dailyUnitsSold.Clear();
            DailySummaryChanged?.Invoke();
        }

        public void RecordCustomerServed(int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }

            DailyCustomersServed += amount;
            StoreProgressionManager.Instance?.RegisterCustomersServed(amount);
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

        public void ApplyDailyGoalBonus(int day)
        {
            if (dailyGoalBonusApplied || day <= 0)
            {
                return;
            }

            DailyGoalCompletedCount = GetCompletedGoalCount(day);
            DailyGoalBonusIncome = DailyGoalCompletedCount * GetGoalBonusPerGoal(day);
            CurrentMoney += DailyGoalBonusIncome;
            dailyGoalBonusApplied = true;
            StoreProgressionManager.Instance?.RegisterGoalsCompleted(DailyGoalCompletedCount);
            MoneyChanged?.Invoke(CurrentMoney);
            DailySummaryChanged?.Invoke();
        }

        public int GetDailySalesTarget(int day)
        {
            int normalizedDay = Mathf.Max(1, day);
            if (normalizedDay <= 5)
            {
                return 4000 + ((normalizedDay - 1) * 1500);
            }

            if (normalizedDay <= 13)
            {
                return 12000 + ((normalizedDay - 6) * 2000);
            }

            if (normalizedDay <= 20)
            {
                return 30000 + ((normalizedDay - 14) * 2500);
            }

            return 50000 + ((normalizedDay - 21) * 3000);
        }

        public int GetDailyCustomersTarget(int day)
        {
            int normalizedDay = Mathf.Max(1, day);
            if (normalizedDay <= 5)
            {
                return 2 + (normalizedDay - 1);
            }

            if (normalizedDay <= 13)
            {
                return 7 + (normalizedDay - 6);
            }

            if (normalizedDay <= 20)
            {
                return 16 + (normalizedDay - 14);
            }

            return 24 + (normalizedDay - 21);
        }

        public int CountEmptyAssignedShelves()
        {
            Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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

        public int GetCompletedGoalCount(int day)
        {
            if (day <= 0)
            {
                return 0;
            }

            int completedCount = 0;
            if (DailySales >= GetDailySalesTarget(day))
            {
                completedCount++;
            }

            if (DailyCustomersServed >= GetDailyCustomersTarget(day))
            {
                completedCount++;
            }

            if (CountEmptyAssignedShelves() == 0)
            {
                completedCount++;
            }

            return completedCount;
        }

        public Dictionary<ProductData, int> GetDailyUnitsSoldSnapshot()
        {
            return new Dictionary<ProductData, int>(dailyUnitsSold);
        }

        private int GetGoalBonusPerGoal(int day)
        {
            int normalizedDay = Mathf.Max(1, day);
            if (normalizedDay <= 5)
            {
                return 1500;
            }

            if (normalizedDay <= 13)
            {
                return 1750;
            }

            if (normalizedDay <= 20)
            {
                return 2250;
            }

            return 3000;
        }

        public void RestoreDailySummary(
            int dailySales,
            int dailyCosts,
            int dailyBottleCount,
            int dailyBottleReturnIncome,
            int dailyGoalCompletedCount,
            int dailyGoalBonusIncome,
            int dailyCustomersServed,
            IEnumerable<KeyValuePair<ProductData, int>> soldProducts)
        {
            DailySales = Mathf.Max(0, dailySales);
            DailyCosts = Mathf.Max(0, dailyCosts);
            DailyBottleCount = Mathf.Max(0, dailyBottleCount);
            DailyBottleReturnIncome = Mathf.Max(0, dailyBottleReturnIncome);
            DailyGoalCompletedCount = Mathf.Max(0, dailyGoalCompletedCount);
            DailyGoalBonusIncome = Mathf.Max(0, dailyGoalBonusIncome);
            DailyCustomersServed = Mathf.Max(0, dailyCustomersServed);

            dailyUnitsSold.Clear();
            if (soldProducts != null)
            {
                foreach (KeyValuePair<ProductData, int> entry in soldProducts)
                {
                    if (entry.Key == null || entry.Value <= 0)
                    {
                        continue;
                    }

                    dailyUnitsSold[entry.Key] = entry.Value;
                }
            }

            dailyBottleSettlementApplied = DailyBottleReturnIncome > 0 || DailyBottleCount > 0;
            dailyGoalBonusApplied = DailyGoalBonusIncome > 0 || DailyGoalCompletedCount > 0;
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
