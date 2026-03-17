using System.Collections.Generic;
using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Managers
{
    public class OrderManager : MonoBehaviour
    {
        [SerializeField] private Transform storageSpawnPoint;
        [SerializeField] private StorageBox storageBoxPrefab;
        [SerializeField] private Vector2Int spawnGridSize = new Vector2Int(3, 3);
        [SerializeField] private float spawnSpacing = 1.2f;
        [SerializeField] private Vector3 spawnCheckHalfExtents = new Vector3(0.4f, 0.5f, 0.4f);
        [SerializeField] private LayerMask spawnBlockingMask = Physics.DefaultRaycastLayers;

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
                if (TryFindSpawnPosition(out Vector3 spawnPosition))
                {
                    StorageBox box = Instantiate(storageBoxPrefab, spawnPosition, storageSpawnPoint.rotation);
                    box.Initialize(product, amount);
                }
                else
                {
                    UIFeedback.ShowStatus("창고가 가득 차서 새 발주 박스를 놓을 자리가 없습니다.");
                }
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

        private bool TryFindSpawnPosition(out Vector3 spawnPosition)
        {
            spawnPosition = storageSpawnPoint.position;

            int width = Mathf.Max(1, spawnGridSize.x);
            int height = Mathf.Max(1, spawnGridSize.y);
            int centerX = width / 2;
            int centerY = height / 2;

            for (int radius = 0; radius < Mathf.Max(width, height); radius++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (Mathf.Abs(x - centerX) != radius && Mathf.Abs(y - centerY) != radius)
                        {
                            continue;
                        }

                        Vector3 candidate = storageSpawnPoint.position
                            + storageSpawnPoint.right * ((x - centerX) * spawnSpacing)
                            + storageSpawnPoint.forward * ((y - centerY) * spawnSpacing);

                        if (IsSpawnPositionFree(candidate))
                        {
                            spawnPosition = candidate;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsSpawnPositionFree(Vector3 position)
        {
            Collider[] hits = Physics.OverlapBox(
                position,
                spawnCheckHalfExtents,
                storageSpawnPoint.rotation,
                spawnBlockingMask,
                QueryTriggerInteraction.Ignore);

            return hits.Length == 0;
        }

        private void OnDrawGizmosSelected()
        {
            if (storageSpawnPoint == null)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);

            int width = Mathf.Max(1, spawnGridSize.x);
            int height = Mathf.Max(1, spawnGridSize.y);
            int centerX = width / 2;
            int centerY = height / 2;

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(storageSpawnPoint.position, storageSpawnPoint.rotation, Vector3.one);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 localPosition = new Vector3((x - centerX) * spawnSpacing, 0f, (y - centerY) * spawnSpacing);
                    Gizmos.DrawWireCube(localPosition, spawnCheckHalfExtents * 2f);
                }
            }

            Gizmos.matrix = previousMatrix;
        }
    }
}
