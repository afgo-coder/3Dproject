using System.Collections.Generic;
using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.UI;
using MiniMart.Workers;
using UnityEngine;

namespace MiniMart.Managers
{
    public class ShadowWorkerManager : MonoBehaviour
    {
        [SerializeField] private ShadowWorkerAI shadowWorkerPrefab;
        [SerializeField] private CheckoutCounter checkoutCounter;
        [SerializeField] private Transform[] stockerSpawnPoints;
        [SerializeField] private Transform[] cashierSpawnPoints;
        [SerializeField] private int stockerCost = 10000;
        [SerializeField] private int cashierCost = 15000;
        [SerializeField] private int maxStockers = 1;
        [SerializeField] private int maxCashiers = 1;

        private readonly List<ShadowWorkerAI> stockers = new List<ShadowWorkerAI>();
        private readonly List<ShadowWorkerAI> cashiers = new List<ShadowWorkerAI>();

        public int StockerCost => StoreProgressionManager.Instance != null ? StoreProgressionManager.Instance.GetAdjustedWorkerCost(stockerCost) : stockerCost;
        public int CashierCost => StoreProgressionManager.Instance != null ? StoreProgressionManager.Instance.GetAdjustedWorkerCost(cashierCost) : cashierCost;
        public int StockerCount => CountAlive(stockers);
        public int CashierCount => CountAlive(cashiers);
        public int MaxStockers => GetMaxStockerCapacity();
        public int MaxCashiers => maxCashiers;

        private void Awake()
        {
            stockerCost = 12000;
            cashierCost = 18000;
        }

        public bool TryHireStockerShadow()
        {
            CleanupDeadWorkers();
            if (shadowWorkerPrefab == null)
            {
                UIFeedback.ShowStatus("분신 프리팹이 연결되지 않았습니다.");
                return false;
            }

            if (StockerCount >= MaxStockers)
            {
                UIFeedback.ShowStatus("진열 분신을 더 이상 고용할 수 없습니다.");
                return false;
            }

            int adjustedStockerCost = StoreProgressionManager.Instance != null
                ? StoreProgressionManager.Instance.GetAdjustedWorkerCost(stockerCost)
                : stockerCost;
            if (EconomyManager.Instance == null || !EconomyManager.Instance.TrySpend(adjustedStockerCost))
            {
                UIFeedback.ShowStatus($"진열 분신 고용 실패: {stockerCost:N0}원이 필요합니다.");
                return false;
            }

            ShadowWorkerAI worker = SpawnWorker(ShadowWorkerRole.Stocker, stockerSpawnPoints, stockers);
            if (worker == null)
            {
                EconomyManager.Instance.AddSale(adjustedStockerCost);
                UIFeedback.ShowStatus("진열 분신 소환 위치가 없습니다.");
                return false;
            }

            UIFeedback.ShowStatus("진열 분신을 고용했습니다.");
            SaveManager.Instance?.SaveGame();
            return true;
        }

        public bool TryHireCashierShadow()
        {
            CleanupDeadWorkers();
            if (shadowWorkerPrefab == null)
            {
                UIFeedback.ShowStatus("분신 프리팹이 연결되지 않았습니다.");
                return false;
            }

            if (CashierCount >= maxCashiers)
            {
                UIFeedback.ShowStatus("계산 분신은 더 이상 고용할 수 없습니다.");
                return false;
            }

            int adjustedCashierCost = StoreProgressionManager.Instance != null
                ? StoreProgressionManager.Instance.GetAdjustedWorkerCost(cashierCost)
                : cashierCost;
            if (EconomyManager.Instance == null || !EconomyManager.Instance.TrySpend(adjustedCashierCost))
            {
                UIFeedback.ShowStatus($"계산 분신 고용 실패: {cashierCost:N0}원이 필요합니다.");
                return false;
            }

            ShadowWorkerAI worker = SpawnWorker(ShadowWorkerRole.Cashier, cashierSpawnPoints, cashiers);
            if (worker == null)
            {
                EconomyManager.Instance.AddSale(adjustedCashierCost);
                UIFeedback.ShowStatus("계산 분신 소환 위치가 없습니다.");
                return false;
            }

            UIFeedback.ShowStatus("계산 분신을 고용했습니다.");
            SaveManager.Instance?.SaveGame();
            return true;
        }

        public void RestoreWorkers(int stockerCount, int cashierCount)
        {
            CleanupExistingWorkers();

            int stockerTarget = Mathf.Clamp(stockerCount, 0, MaxStockers);
            int cashierTarget = Mathf.Clamp(cashierCount, 0, maxCashiers);

            for (int i = 0; i < stockerTarget; i++)
            {
                SpawnWorker(ShadowWorkerRole.Stocker, stockerSpawnPoints, stockers);
            }

            for (int i = 0; i < cashierTarget; i++)
            {
                SpawnWorker(ShadowWorkerRole.Cashier, cashierSpawnPoints, cashiers);
            }
        }

        public Shelf FindShelfToRestock()
        {
            CleanupDeadWorkers();
            OrderManager orderManager = FindFirstObjectByType<OrderManager>();
            if (orderManager == null)
            {
                return null;
            }

            Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Shelf bestShelf = null;
            int mostMissingStock = 0;

            for (int i = 0; i < shelves.Length; i++)
            {
                Shelf shelf = shelves[i];
                if (shelf == null || !shelf.NeedsRestock || shelf.AssignedProduct == null)
                {
                    continue;
                }

                if (orderManager.GetStorageStock(shelf.AssignedProduct) <= 0)
                {
                    continue;
                }

                if (shelf.MissingStock > mostMissingStock)
                {
                    bestShelf = shelf;
                    mostMissingStock = shelf.MissingStock;
                }
            }

            return bestShelf;
        }

        public CheckoutCounter GetCheckoutCounter()
        {
            return checkoutCounter;
        }

        public StorageBox FindStorageBox(ProductData product)
        {
            if (product == null)
            {
                return null;
            }

            StorageBox[] boxes = FindObjectsByType<StorageBox>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            StorageBox bestBox = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < boxes.Length; i++)
            {
                StorageBox box = boxes[i];
                if (box == null || !box.CanProvide(product))
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, box.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestBox = box;
                }
            }

            return bestBox;
        }

        private ShadowWorkerAI SpawnWorker(ShadowWorkerRole role, Transform[] spawnPoints, List<ShadowWorkerAI> targetList)
        {
            Transform spawnPoint = GetSpawnPoint(spawnPoints, targetList.Count);
            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            ShadowWorkerAI worker = Instantiate(shadowWorkerPrefab, position, rotation);
            worker.Initialize(role, this, spawnPoint);
            targetList.Add(worker);
            return worker;
        }

        private Transform GetSpawnPoint(Transform[] spawnPoints, int index)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return null;
            }

            int clampedIndex = Mathf.Clamp(index, 0, spawnPoints.Length - 1);
            return spawnPoints[clampedIndex];
        }

        private void CleanupDeadWorkers()
        {
            stockers.RemoveAll(worker => worker == null);
            cashiers.RemoveAll(worker => worker == null);
        }

        private void CleanupExistingWorkers()
        {
            for (int i = 0; i < stockers.Count; i++)
            {
                if (stockers[i] == null)
                {
                    continue;
                }

                DestroyWorker(stockers[i]);
            }

            for (int i = 0; i < cashiers.Count; i++)
            {
                if (cashiers[i] == null)
                {
                    continue;
                }

                DestroyWorker(cashiers[i]);
            }

            stockers.Clear();
            cashiers.Clear();
        }

        private void DestroyWorker(ShadowWorkerAI worker)
        {
            if (Application.isPlaying)
            {
                Destroy(worker.gameObject);
            }
            else
            {
                DestroyImmediate(worker.gameObject);
            }
        }

        private int CountAlive(List<ShadowWorkerAI> workers)
        {
            workers.RemoveAll(worker => worker == null);
            return workers.Count;
        }

        private int GetMaxStockerCapacity()
        {
            StoreExpansionManager expansionManager = FindFirstObjectByType<StoreExpansionManager>();
            int expansionBonus = expansionManager != null ? expansionManager.CurrentExpansionLevel : 0;
            return Mathf.Max(1, maxStockers + expansionBonus);
        }
    }
}
