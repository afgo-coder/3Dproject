using MiniMart.Customer;
using MiniMart.Data;
using MiniMart.Interaction;
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
            if (dayCycle == null || !dayCycle.IsRunning || customerPrefab == null || spawnPoints.Length == 0 || _aliveCustomers >= maxCustomers)
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
            CustomerAI customer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
            CustomerTypeData type = customerTypes.Length > 0 ? customerTypes[Random.Range(0, customerTypes.Length)] : null;
            customer.Initialize(type, exitPoint, HandleCustomerExited);
            customer.SetShoppingTargets(GetRandomAvailableShelf(), _checkoutCounter);
            _aliveCustomers++;
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

        private void HandleCustomerExited(CustomerAI customer)
        {
            _aliveCustomers = Mathf.Max(0, _aliveCustomers - 1);
        }
    }
}
