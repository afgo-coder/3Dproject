using System;
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
        public ProductWeight[] preferredProducts;
    }
}
