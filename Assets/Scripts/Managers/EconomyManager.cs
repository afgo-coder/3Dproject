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

        private readonly Dictionary<ProductData, int> _dailyUnitsSold = new Dictionary<ProductData, int>();

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

        public void RecordProductSale(ProductData product, int amount = 1)
        {
            if (product == null || amount <= 0)
            {
                return;
            }

            if (_dailyUnitsSold.ContainsKey(product))
            {
                _dailyUnitsSold[product] += amount;
            }
            else
            {
                _dailyUnitsSold.Add(product, amount);
            }

            DailySummaryChanged?.Invoke();
        }

        public void ResetDailySummary()
        {
            DailySales = 0;
            DailyCosts = 0;
            _dailyUnitsSold.Clear();
            DailySummaryChanged?.Invoke();
        }

        public ProductData GetBestSellingProduct(out int soldCount)
        {
            soldCount = 0;
            ProductData bestProduct = null;

            foreach (KeyValuePair<ProductData, int> entry in _dailyUnitsSold)
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
