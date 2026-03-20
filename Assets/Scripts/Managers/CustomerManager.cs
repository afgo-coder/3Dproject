using MiniMart.Core;
using MiniMart.Customer;
using MiniMart.Data;
using MiniMart.Interaction;
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
            if (dayCycle == null || !dayCycle.IsRunning || dayCycle.IsPreparationDay || customerPrefab == null || spawnPoints.Length == 0 || aliveCustomers >= maxCustomers)
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
            int effectiveLevel = (storeExpansionManager != null ? storeExpansionManager.CurrentExpansionLevel : 0) + 1;
            float intervalMultiplier = Mathf.Max(0.45f, 1f - ((effectiveLevel - 1) * spawnIntervalReductionPerExpansion));
            return (baseSpawnInterval * intervalMultiplier) / dayMultiplier;
        }

        private int GetSpawnBatchCount()
        {
            int effectiveLevel = (storeExpansionManager != null ? storeExpansionManager.CurrentExpansionLevel : 0) + 1;
            if (effectiveLevel <= 1)
            {
                return 1;
            }

            if (effectiveLevel == 2)
            {
                return Random.Range(1, Mathf.Min(2, maxSpawnBatchSize) + 1);
            }

            int minBatch = Mathf.Min(2, maxSpawnBatchSize);
            int maxBatch = Mathf.Min(3, maxSpawnBatchSize);
            return Random.Range(minBatch, maxBatch + 1);
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

            for (int i = 0; i < shelves.Length; i++)
            {
                Shelf shelf = shelves[i];
                if (shelf != null && shelf.AssignedProduct == product && shelf.CurrentStock > 0)
                {
                    return shelf;
                }
            }

            return null;
        }

        private Shelf GetRandomAvailableShelf()
        {
            Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
            if (shelves == null || shelves.Length == 0)
            {
                return null;
            }

            int startIndex = Random.Range(0, shelves.Length);
            for (int i = 0; i < shelves.Length; i++)
            {
                Shelf shelf = shelves[(startIndex + i) % shelves.Length];
                if (shelf != null && shelf.CurrentStock > 0)
                {
                    return shelf;
                }
            }

            return shelves[startIndex];
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
        }
    }
}
