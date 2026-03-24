using MiniMart.Core;
using MiniMart.Customer;
using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.Tutorial;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Managers
{
    public class CustomerManager : MonoBehaviour
    {
        [SerializeField] private CustomerAI customerPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private CustomerTypeData[] customerTypes;
        [SerializeField] private float baseSpawnInterval = 8f;
        [SerializeField] private int maxCustomers = 5;
        [SerializeField] private float spawnIntervalReductionPerExpansion = 0.15f;
        [SerializeField] private int maxSpawnBatchSize = 3;
        [SerializeField] private TrashCan trashCan;
        [SerializeField] private ProductData bottleReturnProduct;
        [SerializeField] private float bottleReturnChance = 0.3f;

        private float spawnTimer;
        private int aliveCustomers;
        private CheckoutCounter checkoutCounter;
        private StoreExpansionManager storeExpansionManager;
        private bool isClosingTime;

        public event System.Action AllCustomersExitedAfterClosing;

        public int ActiveCustomerCount => aliveCustomers;
        public bool IsClosingTime => isClosingTime;

        private void Awake()
        {
            checkoutCounter = FindFirstObjectByType<CheckoutCounter>();
            storeExpansionManager = FindFirstObjectByType<StoreExpansionManager>();
            if (trashCan == null)
            {
                trashCan = FindFirstObjectByType<TrashCan>();
            }
        }

        private void Update()
        {
            DayCycleManager dayCycle = FindFirstObjectByType<DayCycleManager>();
            if (dayCycle == null || customerPrefab == null || spawnPoints.Length == 0)
            {
                return;
            }

            if (dayCycle.IsRunning && !dayCycle.IsClosingTime)
            {
                isClosingTime = false;
            }

            if (!dayCycle.IsRunning || isClosingTime || aliveCustomers >= maxCustomers)
            {
                return;
            }

            if (ShouldSpawnTutorialCheckoutCustomer(dayCycle))
            {
                SpawnCustomer(0);
                spawnTimer = Mathf.Max(2f, baseSpawnInterval);
                return;
            }

            if (dayCycle.IsPreparationDay)
            {
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
            {
                return;
            }

            float multiplier = Mathf.Max(0.25f, dayCycle.SpawnRateMultiplier);
            spawnTimer = GetCurrentSpawnInterval(multiplier);

            int spawnCount = GetSpawnBatchCount();
            for (int i = 0; i < spawnCount && aliveCustomers < maxCustomers; i++)
            {
                SpawnCustomer(i);
            }
        }

        private void SpawnCustomer(int batchIndex)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            CustomerTypeData type = customerTypes.Length > 0 ? customerTypes[Random.Range(0, customerTypes.Length)] : null;
            CustomerAI prefabToSpawn = type != null && type.customerPrefabOverride != null ? type.customerPrefabOverride : customerPrefab;
            if (prefabToSpawn == null)
            {
                return;
            }

            Vector3 spawnOffset = spawnPoint.right * (batchIndex * 0.6f);
            CustomerAI customer = Instantiate(prefabToSpawn, spawnPoint.position + spawnOffset, spawnPoint.rotation);
            customer.Initialize(type, exitPoint, HandleCustomerExited, trashCan, bottleReturnProduct, bottleReturnChance);

            Shelf targetShelf = GetShelfForCustomer(type);
            customer.SetShoppingTargets(targetShelf, checkoutCounter);
            UIFeedback.ShowStatus(BuildSpawnMessage(type, targetShelf));
            aliveCustomers++;
        }

        private float GetCurrentSpawnInterval(float dayMultiplier)
        {
            int currentDay = GameManager.Instance != null ? Mathf.Max(1, GameManager.Instance.CurrentDay) : 1;
            int effectiveLevel = (storeExpansionManager != null ? storeExpansionManager.CurrentExpansionLevel : 0) + 1;
            float dayBaseInterval = GetBaseSpawnIntervalForDay(currentDay);
            float intervalMultiplier = Mathf.Max(0.55f, 1f - ((effectiveLevel - 1) * spawnIntervalReductionPerExpansion));
            return (dayBaseInterval * intervalMultiplier * GetTimeSegmentSpawnMultiplier()) / dayMultiplier;
        }

        private int GetSpawnBatchCount()
        {
            int currentDay = GameManager.Instance != null ? Mathf.Max(1, GameManager.Instance.CurrentDay) : 1;
            int effectiveLevel = (storeExpansionManager != null ? storeExpansionManager.CurrentExpansionLevel : 0) + 1;
            int pressureLevel = effectiveLevel + GetAdditionalBatchPressure(currentDay);
            if (pressureLevel <= 1)
            {
                return 1;
            }

            if (pressureLevel == 2)
            {
                return Random.Range(1, Mathf.Min(2, maxSpawnBatchSize) + 1);
            }

            int minBatch = Mathf.Min(2, maxSpawnBatchSize);
            int maxBatch = Mathf.Min(3, maxSpawnBatchSize);
            return Random.Range(minBatch, maxBatch + 1);
        }

        private float GetBaseSpawnIntervalForDay(int day)
        {
            if (day <= 5)
            {
                return 8.5f;
            }

            if (day <= 13)
            {
                return 7f;
            }

            if (day <= 20)
            {
                return 5.9f;
            }

            return 5.1f;
        }

        private static int GetAdditionalBatchPressure(int day)
        {
            if (day <= 5)
            {
                return 0;
            }

            if (day <= 13)
            {
                return 1;
            }

            return 2;
        }

        private Shelf GetShelfForCustomer(CustomerTypeData customerType)
        {
            Shelf preferredShelf = GetWeightedRandomPreferredShelf(customerType);
            if (preferredShelf != null)
            {
                return preferredShelf;
            }

            return GetRandomAvailableShelf();
        }

        private Shelf GetWeightedRandomPreferredShelf(CustomerTypeData customerType)
        {
            if (customerType == null || customerType.PreferredProducts == null || customerType.PreferredProducts.Length == 0)
            {
                return null;
            }

            ProductWeight[] preferredProducts = customerType.PreferredProducts;
            Shelf[] candidateShelves = new Shelf[preferredProducts.Length];
            float[] candidateWeights = new float[preferredProducts.Length];
            int candidateCount = 0;
            float totalWeight = 0f;

            for (int i = 0; i < preferredProducts.Length; i++)
            {
                ProductWeight preferred = preferredProducts[i];
                if (preferred.product == null || preferred.weight <= 0f)
                {
                    continue;
                }

                Shelf shelf = FindShelfByProduct(preferred.product);
                if (shelf == null)
                {
                    continue;
                }

                candidateShelves[candidateCount] = shelf;
                candidateWeights[candidateCount] = preferred.weight;
                totalWeight += preferred.weight;
                candidateCount++;
            }

            if (candidateCount == 0 || totalWeight <= 0f)
            {
                return null;
            }

            float roll = Random.Range(0f, totalWeight);
            float accumulated = 0f;

            for (int i = 0; i < candidateCount; i++)
            {
                accumulated += candidateWeights[i];
                if (roll <= accumulated)
                {
                    return candidateShelves[i];
                }
            }

            return candidateShelves[candidateCount - 1];
        }

        private Shelf FindShelfByProduct(ProductData product)
        {
            Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
            if (product == null || shelves == null)
            {
                return null;
            }

            Shelf bestShelf = null;
            float bestScore = float.MinValue;
            for (int i = 0; i < shelves.Length; i++)
            {
                Shelf shelf = shelves[i];
                if (shelf != null && shelf.AssignedProduct == product && shelf.CurrentStock > 0)
                {
                    float score = GetShelfDemandScore(shelf);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestShelf = shelf;
                    }
                }
            }

            return bestShelf;
        }

        private Shelf GetRandomAvailableShelf()
        {
            Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
            if (shelves == null || shelves.Length == 0)
            {
                return null;
            }

            Shelf bestShelf = null;
            float bestScore = float.MinValue;
            for (int i = 0; i < shelves.Length; i++)
            {
                Shelf shelf = shelves[i];
                if (shelf != null && shelf.CurrentStock > 0)
                {
                    float score = GetShelfDemandScore(shelf);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestShelf = shelf;
                    }
                }
            }

            return bestShelf != null ? bestShelf : shelves[Random.Range(0, shelves.Length)];
        }

        private float GetShelfDemandScore(Shelf shelf)
        {
            if (shelf == null || shelf.AssignedProduct == null)
            {
                return 0f;
            }

            float demandMultiplier = StoreProgressionManager.Instance != null
                ? StoreProgressionManager.Instance.GetDemandMultiplier(shelf.AssignedProduct)
                : 1f;
            return demandMultiplier + Random.Range(0f, 0.15f);
        }

        private float GetTimeSegmentSpawnMultiplier()
        {
            if (StoreProgressionManager.Instance == null)
            {
                return 1f;
            }

            switch (StoreProgressionManager.Instance.GetCurrentTimeSegment())
            {
                case StoreTimeSegment.Morning:
                    return 1.1f;
                case StoreTimeSegment.Lunch:
                    return 0.82f;
                case StoreTimeSegment.Evening:
                    return 0.92f;
                case StoreTimeSegment.Night:
                    return 1.12f;
                default:
                    return 1f;
            }
        }

        private string BuildSpawnMessage(CustomerTypeData customerType, Shelf targetShelf)
        {
            string customerName = customerType != null && !string.IsNullOrWhiteSpace(customerType.typeName)
                ? customerType.typeName
                : "일반 손님";
            string productName = targetShelf != null && targetShelf.AssignedProduct != null
                ? targetShelf.AssignedProduct.productName
                : "아무 상품";

            return $"{customerName} 손님 입장 - 목표 상품: {productName}";
        }

        private void HandleCustomerExited(CustomerAI customer)
        {
            aliveCustomers = Mathf.Max(0, aliveCustomers - 1);

            if (isClosingTime && aliveCustomers == 0)
            {
                AllCustomersExitedAfterClosing?.Invoke();
            }
        }

        private bool ShouldSpawnTutorialCheckoutCustomer(DayCycleManager dayCycle)
        {
            if (dayCycle == null || !dayCycle.IsPreparationDay || aliveCustomers > 0 || isClosingTime)
            {
                return false;
            }

            TutorialManager tutorialManager = TutorialManager.Instance;
            if (tutorialManager == null)
            {
                return false;
            }

            return tutorialManager.HasOpenedOrderTerminal &&
                   tutorialManager.HasPlacedOrder &&
                   tutorialManager.HasStockedShelf &&
                   !tutorialManager.HasCompletedCheckout;
        }

        public void BeginClosingTime()
        {
            isClosingTime = true;

            if (aliveCustomers == 0)
            {
                AllCustomersExitedAfterClosing?.Invoke();
            }
        }
    }
}
