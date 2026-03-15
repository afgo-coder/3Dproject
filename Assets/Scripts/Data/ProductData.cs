using UnityEngine;

namespace MiniMart.Data
{
    public enum ProductCategory
    {
        Drink,
        Snack,
        InstantFood,
        Meal,
        DailyGoods
    }

    [CreateAssetMenu(menuName = "MiniMart/Product Data", fileName = "ProductData")]
    public class ProductData : ScriptableObject
    {
        [Header("Identity")]
        public string productId;
        public string productName;
        public ProductCategory category;

        [Header("Economy")]
        public int salePrice = 1000;
        public int costPrice = 700;
        [Range(0.1f, 3f)] public float popularity = 1f;

        [Header("Presentation")]
        public Sprite icon;
        public GameObject worldPrefab;

        [Header("Shelf")]
        [Min(1)] public int maxShelfCapacity = 8;
    }
}
