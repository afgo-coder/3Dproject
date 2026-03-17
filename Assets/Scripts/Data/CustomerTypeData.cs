using System;
using System.Collections.Generic;
using MiniMart.Customer;
using UnityEngine;

namespace MiniMart.Data
{
    [Serializable]
    public struct ProductWeight
    {
        public ProductData product;
        [Min(0f)] public float weight;
    }

    [CreateAssetMenu(menuName = "MiniMart/Customer Type", fileName = "CustomerTypeData")]
    public class CustomerTypeData : ScriptableObject
    {
        public string typeName;
        [Min(0.5f)] public float walkSpeed = 2f;
        [Min(0f)] public float browseDuration = 2f;
        public CustomerAI customerPrefabOverride;
        public ProductWeight[] preferredProducts;
        public ProductWeight[] PreferredProducts => preferredProducts;

        public ProductData GetRandomPreferredProduct()
        {
            if (preferredProducts == null || preferredProducts.Length == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            for (int i = 0; i < preferredProducts.Length; i++)
            {
                if (preferredProducts[i].product != null && preferredProducts[i].weight > 0f)
                {
                    totalWeight += preferredProducts[i].weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float accumulated = 0f;

            for (int i = 0; i < preferredProducts.Length; i++)
            {
                ProductWeight entry = preferredProducts[i];
                if (entry.product == null || entry.weight <= 0f)
                {
                    continue;
                }

                accumulated += entry.weight;
                if (roll <= accumulated)
                {
                    return entry.product;
                }
            }

            return null;
        }

        public IEnumerable<ProductData> GetPreferredProductsSorted()
        {
            if (preferredProducts == null || preferredProducts.Length == 0)
            {
                yield break;
            }

            ProductWeight[] copy = (ProductWeight[])preferredProducts.Clone();
            Array.Sort(copy, (a, b) => b.weight.CompareTo(a.weight));

            for (int i = 0; i < copy.Length; i++)
            {
                if (copy[i].product != null && copy[i].weight > 0f)
                {
                    yield return copy[i].product;
                }
            }
        }
    }
}
