using System;
using System.Collections;
using System.Collections.Generic;
using MiniMart.Core;
using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.Player;
using MiniMart.Workers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniMart.Managers
{
    public class SaveManager : MonoBehaviour
    {
        private const string SaveKey = "MiniMart.SaveData";
        private const string ContinueEligibleKey = "MiniMart.ContinueEligible";
        private static bool pendingLoad;

        public static SaveManager Instance { get; private set; }
        public static bool HasSaveData => HasUsableSaveData();
        public static bool IsLoadPending => pendingLoad;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
            pendingLoad = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstanceBeforeSceneLoad()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject saveObject = new GameObject("SaveManager");
            saveObject.AddComponent<SaveManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            TrySaveCurrentGame();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                TrySaveCurrentGame();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                TrySaveCurrentGame();
            }
        }

        public static void RequestContinueGame()
        {
            if (!HasSaveData)
            {
                pendingLoad = false;
                return;
            }

            pendingLoad = true;
        }

        public void PrepareForNewGame()
        {
            pendingLoad = false;
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.SetInt(ContinueEligibleKey, 1);
            PlayerPrefs.Save();
        }

        public void ClearSaveData()
        {
            pendingLoad = false;
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(ContinueEligibleKey);
            PlayerPrefs.Save();
        }

        public void SaveGame()
        {
            if (!CanCaptureCurrentScene())
            {
                return;
            }

            MiniMartSaveData data = BuildSaveData();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public bool LoadGame()
        {
            if (!HasSaveData)
            {
                return false;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            MiniMartSaveData data = JsonUtility.FromJson<MiniMartSaveData>(json);
            if (data == null)
            {
                return false;
            }

            RestoreSaveData(data);
            return true;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!pendingLoad)
            {
                return;
            }

            if (!string.Equals(scene.name, "Game", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            StartCoroutine(LoadAfterSceneReady());
        }

        private void TrySaveCurrentGame()
        {
            if (!CanCaptureCurrentScene())
            {
                return;
            }

            SaveGame();
        }

        private IEnumerator LoadAfterSceneReady()
        {
            yield return null;
            pendingLoad = false;

            if (LoadGame())
            {
                yield break;
            }

            DayCycleManager dayCycleManager = FindFirstObjectByType<DayCycleManager>();
            dayCycleManager?.BeginPreparationDay();
        }

        private static bool CanCaptureCurrentScene()
        {
            return GameManager.Instance != null &&
                   EconomyManager.Instance != null &&
                   FindFirstObjectByType<DayCycleManager>() != null;
        }

        private static bool HasUsableSaveData()
        {
            if (PlayerPrefs.GetInt(ContinueEligibleKey, 0) != 1)
            {
                return false;
            }

            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return false;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            MiniMartSaveData data = JsonUtility.FromJson<MiniMartSaveData>(json);
            return data != null && IsMeaningfulProgress(data);
        }

        private static bool IsMeaningfulProgress(MiniMartSaveData data)
        {
            if (data == null)
            {
                return false;
            }

            return data.currentDay > 0 ||
                   data.expansionLevel > 0 ||
                   data.stockerCount > 0 ||
                   data.cashierCount > 0 ||
                   data.dailySales > 0 ||
                   data.dailyCustomersServed > 0 ||
                   data.lifetimeSales > 0 ||
                   data.lifetimeCustomers > 0 ||
                   data.totalGoalsCompleted > 0 ||
                   data.storageEntries.Count > 0 ||
                   data.buildSlots.Count > 0 ||
                   data.dailySoldProducts.Count > 0 ||
                   data.tutorialOpenedOrderTerminal ||
                   data.tutorialPlacedOrder ||
                   data.tutorialStockedShelf ||
                   data.tutorialCompletedCheckout;
        }

        private MiniMartSaveData BuildSaveData()
        {
            DayCycleManager dayCycleManager = FindFirstObjectByType<DayCycleManager>();
            OrderManager orderManager = FindFirstObjectByType<OrderManager>();
            StoreExpansionManager expansionManager = FindFirstObjectByType<StoreExpansionManager>();
            PlacementManager placementManager = FindFirstObjectByType<PlacementManager>();
            ShadowWorkerManager shadowWorkerManager = FindFirstObjectByType<ShadowWorkerManager>();
            TrashCan trashCan = FindFirstObjectByType<TrashCan>();
            PlayerInteractor playerInteractor = FindFirstObjectByType<PlayerInteractor>();
            Tutorial.TutorialManager tutorialManager = Tutorial.TutorialManager.Instance;
            StoreProgressionManager progressionManager = StoreProgressionManager.Instance;

            MiniMartSaveData data = new MiniMartSaveData
            {
                currentDay = GameManager.Instance != null ? GameManager.Instance.CurrentDay : 0,
                normalizedTime = dayCycleManager != null ? dayCycleManager.NormalizedTime : 0f,
                dayWasRunning = dayCycleManager != null && dayCycleManager.IsRunning,
                dayWasClosingTime = dayCycleManager != null && dayCycleManager.IsClosingTime,
                currentMoney = EconomyManager.Instance != null ? EconomyManager.Instance.CurrentMoney : 0,
                expansionLevel = expansionManager != null ? expansionManager.CurrentExpansionLevel : 0,
                stockerCount = shadowWorkerManager != null ? shadowWorkerManager.StockerCount : 0,
                cashierCount = shadowWorkerManager != null ? shadowWorkerManager.CashierCount : 0,
                dailySales = EconomyManager.Instance != null ? EconomyManager.Instance.DailySales : 0,
                dailyCosts = EconomyManager.Instance != null ? EconomyManager.Instance.DailyCosts : 0,
                dailyBottleCount = EconomyManager.Instance != null ? EconomyManager.Instance.DailyBottleCount : 0,
                dailyBottleReturnIncome = EconomyManager.Instance != null ? EconomyManager.Instance.DailyBottleReturnIncome : 0,
                dailyGoalCompletedCount = EconomyManager.Instance != null ? EconomyManager.Instance.DailyGoalCompletedCount : 0,
                dailyGoalBonusIncome = EconomyManager.Instance != null ? EconomyManager.Instance.DailyGoalBonusIncome : 0,
                dailyCustomersServed = EconomyManager.Instance != null ? EconomyManager.Instance.DailyCustomersServed : 0,
                trashCanBottleCount = trashCan != null ? trashCan.CurrentBottleCount : 0,
                trashCanPassiveTimerSeconds = trashCan != null ? trashCan.PassiveBottleTimerSeconds : 0f,
                heldProductId = playerInteractor != null && playerInteractor.HeldProduct != null ? playerInteractor.HeldProduct.productId : string.Empty,
                lifetimeSales = progressionManager != null ? progressionManager.LifetimeSales : 0,
                lifetimeCustomers = progressionManager != null ? progressionManager.LifetimeCustomers : 0,
                totalGoalsCompleted = progressionManager != null ? progressionManager.TotalGoalsCompleted : 0,
                tutorialOpenedOrderTerminal = tutorialManager != null && tutorialManager.HasOpenedOrderTerminal,
                tutorialPlacedOrder = tutorialManager != null && tutorialManager.HasPlacedOrder,
                tutorialStockedShelf = tutorialManager != null && tutorialManager.HasStockedShelf,
                tutorialCompletedCheckout = tutorialManager != null && tutorialManager.HasCompletedCheckout,
            };

            if (EconomyManager.Instance != null)
            {
                Dictionary<ProductData, int> soldProducts = EconomyManager.Instance.GetDailyUnitsSoldSnapshot();
                foreach (KeyValuePair<ProductData, int> entry in soldProducts)
                {
                    if (entry.Key == null || entry.Value <= 0)
                    {
                        continue;
                    }

                    data.dailySoldProducts.Add(new ProductSalesSaveData
                    {
                        productId = entry.Key.productId,
                        amount = entry.Value
                    });
                }
            }

            if (orderManager != null)
            {
                Dictionary<ProductData, int> storageSnapshot = orderManager.GetStorageSnapshot();
                foreach (KeyValuePair<ProductData, int> entry in storageSnapshot)
                {
                    if (entry.Key == null || entry.Value <= 0)
                    {
                        continue;
                    }

                    data.storageEntries.Add(new StorageEntrySaveData
                    {
                        productId = entry.Key.productId,
                        amount = entry.Value
                    });
                }
            }

            if (placementManager != null)
            {
                BuildSlot[] buildSlots = FindObjectsByType<BuildSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < buildSlots.Length; i++)
                {
                    BuildSlot slot = buildSlots[i];
                    if (slot == null || !slot.HasPlacedFurniture || slot.PlacedFurnitureData == null)
                    {
                        continue;
                    }

                    Shelf placedShelf = slot.PlacedShelf;
                    data.buildSlots.Add(new BuildSlotSaveData
                    {
                        slotPath = GetHierarchyPath(slot.transform),
                        furnitureId = slot.PlacedFurnitureData.GetFurnitureId(),
                        productId = placedShelf != null && placedShelf.AssignedProduct != null ? placedShelf.AssignedProduct.productId : string.Empty,
                        stock = placedShelf != null ? placedShelf.CurrentStock : 0
                    });
                }
            }

            Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < shelves.Length; i++)
            {
                Shelf shelf = shelves[i];
                if (shelf == null || shelf.AssignedProduct == null)
                {
                    continue;
                }

                if (shelf.GetComponentInParent<BuildSlot>() != null)
                {
                    continue;
                }

                data.sceneShelves.Add(new ShelfSaveData
                {
                    shelfPath = GetHierarchyPath(shelf.transform),
                    productId = shelf.AssignedProduct.productId,
                    stock = shelf.CurrentStock
                });
            }

            return data;
        }

        private void RestoreSaveData(MiniMartSaveData data)
        {
            EconomyManager.Instance?.RestoreMoney(data.currentMoney);

            StoreExpansionManager expansionManager = FindFirstObjectByType<StoreExpansionManager>();
            expansionManager?.ApplyExpansionLevel(data.expansionLevel);

            OrderManager orderManager = FindFirstObjectByType<OrderManager>();
            if (orderManager != null)
            {
                orderManager.ClearStorageForRestore();
                for (int i = 0; i < data.storageEntries.Count; i++)
                {
                    StorageEntrySaveData entry = data.storageEntries[i];
                    ProductData product = FindProductById(entry.productId);
                    orderManager.RestoreStorageEntry(product, entry.amount);
                }
            }

            PlacementManager placementManager = FindFirstObjectByType<PlacementManager>();
            if (placementManager != null)
            {
                BuildSlot[] buildSlots = FindObjectsByType<BuildSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < buildSlots.Length; i++)
                {
                    buildSlots[i]?.RemoveFurniture();
                }

                for (int i = 0; i < data.buildSlots.Count; i++)
                {
                    BuildSlotSaveData slotData = data.buildSlots[i];
                    Transform slotTransform = FindByHierarchyPath(slotData.slotPath);
                    if (slotTransform == null)
                    {
                        continue;
                    }

                    BuildSlot slot = slotTransform.GetComponent<BuildSlot>();
                    PlaceableFurnitureData furniture = placementManager.FindFurnitureById(slotData.furnitureId);
                    if (slot == null || furniture == null)
                    {
                        continue;
                    }

                    slot.PlaceFurniture(furniture);
                    Shelf placedShelf = slot.PlacedShelf;
                    ProductData product = FindProductById(slotData.productId);
                    placedShelf?.RestoreState(product, slotData.stock);
                }
            }

            for (int i = 0; i < data.sceneShelves.Count; i++)
            {
                ShelfSaveData shelfData = data.sceneShelves[i];
                Transform shelfTransform = FindByHierarchyPath(shelfData.shelfPath);
                Shelf shelf = shelfTransform != null ? shelfTransform.GetComponent<Shelf>() : null;
                if (shelf == null)
                {
                    continue;
                }

                ProductData product = FindProductById(shelfData.productId);
                shelf.RestoreState(product, shelfData.stock);
            }

            ShadowWorkerManager shadowWorkerManager = FindFirstObjectByType<ShadowWorkerManager>();
            shadowWorkerManager?.RestoreWorkers(data.stockerCount, data.cashierCount);

            DayCycleManager dayCycleManager = FindFirstObjectByType<DayCycleManager>();
            dayCycleManager?.RestoreDayState(
                data.currentDay,
                data.normalizedTime,
                data.dayWasRunning,
                data.dayWasClosingTime);

            List<KeyValuePair<ProductData, int>> restoredDailySales = new List<KeyValuePair<ProductData, int>>();
            for (int i = 0; i < data.dailySoldProducts.Count; i++)
            {
                ProductSalesSaveData soldProduct = data.dailySoldProducts[i];
                ProductData product = FindProductById(soldProduct.productId);
                if (product == null || soldProduct.amount <= 0)
                {
                    continue;
                }

                restoredDailySales.Add(new KeyValuePair<ProductData, int>(product, soldProduct.amount));
            }

            EconomyManager.Instance?.RestoreDailySummary(
                data.dailySales,
                data.dailyCosts,
                data.dailyBottleCount,
                data.dailyBottleReturnIncome,
                data.dailyGoalCompletedCount,
                data.dailyGoalBonusIncome,
                data.dailyCustomersServed,
                restoredDailySales);

            TrashCan trashCan = FindFirstObjectByType<TrashCan>();
            if (trashCan != null)
            {
                trashCan.RestoreState(data.trashCanBottleCount, data.trashCanPassiveTimerSeconds);
            }

            PlayerInteractor playerInteractor = FindFirstObjectByType<PlayerInteractor>();
            ProductData heldProduct = FindProductById(data.heldProductId);
            playerInteractor?.RestoreHeldProduct(heldProduct);

            StoreProgressionManager.Instance?.RestoreProgression(
                data.lifetimeSales,
                data.lifetimeCustomers,
                data.totalGoalsCompleted);

            Tutorial.TutorialManager tutorialManager = Tutorial.TutorialManager.Instance;
            tutorialManager?.RestoreState(
                data.tutorialOpenedOrderTerminal,
                data.tutorialPlacedOrder,
                data.tutorialStockedShelf,
                data.tutorialCompletedCheckout);
        }

        private static ProductData FindProductById(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return null;
            }

            ProductData[] products = Resources.FindObjectsOfTypeAll<ProductData>();
            for (int i = 0; i < products.Length; i++)
            {
                ProductData product = products[i];
                if (product != null && product.productId == productId)
                {
                    return product;
                }
            }

            return null;
        }

        private static string GetHierarchyPath(Transform target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();
            Transform current = target;
            while (current != null)
            {
                parts.Insert(0, current.name);
                current = current.parent;
            }

            return string.Join("/", parts);
        }

        private static Transform FindByHierarchyPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            string[] parts = path.Split('/');
            if (parts.Length == 0)
            {
                return null;
            }

            GameObject root = GameObject.Find(parts[0]);
            if (root == null)
            {
                return null;
            }

            Transform current = root.transform;
            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }
    }

    [Serializable]
    internal class MiniMartSaveData
    {
        public int currentDay;
        public float normalizedTime;
        public bool dayWasRunning;
        public bool dayWasClosingTime;
        public int currentMoney;
        public int expansionLevel;
        public int stockerCount;
        public int cashierCount;
        public int dailySales;
        public int dailyCosts;
        public int dailyBottleCount;
        public int dailyBottleReturnIncome;
        public int dailyGoalCompletedCount;
        public int dailyGoalBonusIncome;
        public int dailyCustomersServed;
        public int trashCanBottleCount;
        public float trashCanPassiveTimerSeconds;
        public string heldProductId;
        public int lifetimeSales;
        public int lifetimeCustomers;
        public int totalGoalsCompleted;
        public bool tutorialOpenedOrderTerminal;
        public bool tutorialPlacedOrder;
        public bool tutorialStockedShelf;
        public bool tutorialCompletedCheckout;
        public List<StorageEntrySaveData> storageEntries = new List<StorageEntrySaveData>();
        public List<BuildSlotSaveData> buildSlots = new List<BuildSlotSaveData>();
        public List<ShelfSaveData> sceneShelves = new List<ShelfSaveData>();
        public List<ProductSalesSaveData> dailySoldProducts = new List<ProductSalesSaveData>();
    }

    [Serializable]
    internal class StorageEntrySaveData
    {
        public string productId;
        public int amount;
    }

    [Serializable]
    internal class BuildSlotSaveData
    {
        public string slotPath;
        public string furnitureId;
        public string productId;
        public int stock;
    }

    [Serializable]
    internal class ShelfSaveData
    {
        public string shelfPath;
        public string productId;
        public int stock;
    }

    [Serializable]
    internal class ProductSalesSaveData
    {
        public string productId;
        public int amount;
    }
}
