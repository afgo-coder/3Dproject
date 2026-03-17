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

        private float _spawnTimer;
        private int _aliveCustomers;
        private Shelf[] _shelves;
        private CheckoutCounter _checkoutCounter;

        private void Awake()
        {
            _shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
            _checkoutCounter = FindFirstObjectByType<CheckoutCounter>();
        }

        private void Update()
        {
            DayCycleManager dayCycle = FindFirstObjectByType<DayCycleManager>();
            if (dayCycle == null || !dayCycle.IsRunning || dayCycle.IsPreparationDay || customerPrefab == null || spawnPoints.Length == 0 || _aliveCustomers >= maxCustomers)
            {
                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f)
            {
                return;
            }

            float multiplier = Mathf.Max(0.25f, dayCycle.SpawnRateMultiplier);
            _spawnTimer = baseSpawnInterval / multiplier;
            SpawnCustomer();
        }

        private void SpawnCustomer()
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            CustomerTypeData type = customerTypes.Length > 0 ? customerTypes[Random.Range(0, customerTypes.Length)] : null;
            CustomerAI prefabToSpawn = type != null && type.customerPrefabOverride != null ? type.customerPrefabOverride : customerPrefab;
            if (prefabToSpawn == null)
            {
                return;
            }

            CustomerAI customer = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            customer.Initialize(type, exitPoint, HandleCustomerExited);

            Shelf targetShelf = GetShelfForCustomer(type);
            customer.SetShoppingTargets(targetShelf, _checkoutCounter);
            UIFeedback.ShowStatus(BuildSpawnMessage(type, targetShelf));
            _aliveCustomers++;
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
            if (product == null || _shelves == null)
            {
                return null;
            }

            for (int i = 0; i < _shelves.Length; i++)
            {
                Shelf shelf = _shelves[i];
                if (shelf != null && shelf.AssignedProduct == product && shelf.CurrentStock > 0)
                {
                    return shelf;
                }
            }

            return null;
        }

        private Shelf GetRandomAvailableShelf()
        {
            if (_shelves == null || _shelves.Length == 0)
            {
                return null;
            }

            int startIndex = Random.Range(0, _shelves.Length);
            for (int i = 0; i < _shelves.Length; i++)
            {
                Shelf shelf = _shelves[(startIndex + i) % _shelves.Length];
                if (shelf != null && shelf.CurrentStock > 0)
                {
                    return shelf;
                }
            }

            return _shelves[startIndex];
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
            _aliveCustomers = Mathf.Max(0, _aliveCustomers - 1);
        }
    }
}
