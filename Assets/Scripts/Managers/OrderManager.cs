using System.Collections.Generic;
using MiniMart.Data;
using MiniMart.Interaction;
using UnityEngine;

namespace MiniMart.Managers
{
    public class OrderManager : MonoBehaviour
    {
        [SerializeField] private Transform storageSpawnPoint;
        [SerializeField] private StorageBox storageBoxPrefab;

        private readonly Dictionary<ProductData, int> _storageInventory = new Dictionary<ProductData, int>();

        public int GetStorageStock(ProductData product)
        {
            if (product == null)
            {
                return 0;
            }

            return _storageInventory.TryGetValue(product, out int amount) ? amount : 0;
        }

        public bool PlaceOrder(ProductData product, int amount)
        {
            if (product == null || amount <= 0)
            {
                return false;
            }

            int totalCost = product.costPrice * amount;
            if (!EconomyManager.Instance || !EconomyManager.Instance.TrySpend(totalCost))
            {
                return false;
            }

            AddStorageStock(product, amount);

            if (storageBoxPrefab != null && storageSpawnPoint != null)
            {
                StorageBox box = Instantiate(storageBoxPrefab, storageSpawnPoint.position, storageSpawnPoint.rotation);
                box.Initialize(product, amount);
            }

            return true;
        }

        public void AddStorageStock(ProductData product, int amount)
        {
            if (product == null || amount <= 0)
            {
                return;
            }

            if (_storageInventory.ContainsKey(product))
            {
                _storageInventory[product] += amount;
            }
            else
            {
                _storageInventory.Add(product, amount);
            }
        }

        public bool TryTakeFromStorage(ProductData product, int amount)
        {
            if (product == null || amount <= 0 || !_storageInventory.TryGetValue(product, out int current) || current < amount)
            {
                return false;
            }

            _storageInventory[product] = current - amount;
            return true;
        }
    }
}
