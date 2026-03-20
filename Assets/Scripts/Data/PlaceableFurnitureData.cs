using MiniMart.Interaction;
using UnityEngine;

namespace MiniMart.Data
{
    [CreateAssetMenu(menuName = "MiniMart/Placeable Furniture", fileName = "PlaceableFurnitureData")]
    public class PlaceableFurnitureData : ScriptableObject
    {
        public string furnitureId;
        public string displayName = "새 가구";
        public int buildCost = 5000;
        public GameObject prefab;
        public Sprite icon;
        public float localYOffset = -0.5f;
        public Vector3 localEulerOffset;

        public Shelf GetShelfPrefab()
        {
            return prefab != null ? prefab.GetComponent<Shelf>() : null;
        }
    }
}
