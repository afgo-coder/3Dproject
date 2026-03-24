using MiniMart.Core;
using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Managers
{
    public class PlacementManager : MonoBehaviour
    {
        public static PlacementManager Instance { get; private set; }

        [SerializeField] private PlaceableFurnitureData[] availableFurniture;

        private int selectedFurnitureIndex;

        public bool IsPlacementModeActive { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (!IsPlacementModeActive || GameManager.Instance == null || GameManager.Instance.IsModalOpen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitPlacementMode();
            }
        }

        public void EnterPlacementMode()
        {
            if (availableFurniture == null || availableFurniture.Length == 0)
            {
                UIFeedback.ShowStatus("배치 가능한 가구가 없습니다.");
                return;
            }

            IsPlacementModeActive = true;
            RefreshBuildSlots();
            UIFeedback.ShowStatus($"배치 모드 시작: {GetSelectedFurnitureName()}");
        }

        public void ExitPlacementMode()
        {
            if (!IsPlacementModeActive)
            {
                return;
            }

            IsPlacementModeActive = false;
            RefreshBuildSlots();
            UIFeedback.ShowStatus("배치 모드를 종료했습니다.");
        }

        public void SelectPreviousFurniture()
        {
            if (availableFurniture == null || availableFurniture.Length == 0)
            {
                return;
            }

            selectedFurnitureIndex = (selectedFurnitureIndex - 1 + availableFurniture.Length) % availableFurniture.Length;
            RefreshBuildSlots();
            UIFeedback.ShowStatus($"선택 가구: {GetSelectedFurnitureName()}");
        }

        public void SelectNextFurniture()
        {
            if (availableFurniture == null || availableFurniture.Length == 0)
            {
                return;
            }

            selectedFurnitureIndex = (selectedFurnitureIndex + 1) % availableFurniture.Length;
            RefreshBuildSlots();
            UIFeedback.ShowStatus($"선택 가구: {GetSelectedFurnitureName()}");
        }

        public string GetSelectedFurnitureName()
        {
            PlaceableFurnitureData data = GetSelectedFurniture();
            return data != null ? data.displayName : "가구 없음";
        }

        public int GetSelectedFurnitureCost()
        {
            PlaceableFurnitureData data = GetSelectedFurniture();
            return data != null ? data.buildCost : 0;
        }

        public string GetPlacementFailureReason(BuildSlot slot)
        {
            if (slot == null)
            {
                return "슬롯 정보가 없습니다.";
            }

            if (!IsPlacementModeActive)
            {
                return "배치 모드가 비활성 상태입니다.";
            }

            if (slot.HasPlacedFurniture)
            {
                return "이미 가구가 설치된 슬롯입니다.";
            }

            PlaceableFurnitureData selectedFurniture = GetSelectedFurniture();
            if (selectedFurniture == null || selectedFurniture.prefab == null)
            {
                return "설치할 가구가 선택되지 않았습니다.";
            }

            if (EconomyManager.Instance == null)
            {
                return "경제 매니저를 찾지 못했습니다.";
            }

            if (EconomyManager.Instance.CurrentMoney < selectedFurniture.buildCost)
            {
                return $"{selectedFurniture.buildCost:N0}원이 필요합니다.";
            }

            return string.Empty;
        }

        public PlaceableFurnitureData GetSelectedFurniture()
        {
            if (availableFurniture == null || availableFurniture.Length == 0)
            {
                return null;
            }

            selectedFurnitureIndex = Mathf.Clamp(selectedFurnitureIndex, 0, availableFurniture.Length - 1);
            return availableFurniture[selectedFurnitureIndex];
        }

        public PlaceableFurnitureData FindFurnitureById(string furnitureId)
        {
            if (availableFurniture == null || string.IsNullOrWhiteSpace(furnitureId))
            {
                return null;
            }

            for (int i = 0; i < availableFurniture.Length; i++)
            {
                PlaceableFurnitureData furniture = availableFurniture[i];
                if (furniture == null)
                {
                    continue;
                }

                if (furniture.GetFurnitureId() == furnitureId || furniture.name == furnitureId)
                {
                    return furniture;
                }
            }

            return null;
        }

        public bool TryPlaceAtSlot(BuildSlot slot)
        {
            if (!IsPlacementModeActive || slot == null)
            {
                return false;
            }

            string failureReason = GetPlacementFailureReason(slot);
            if (!string.IsNullOrWhiteSpace(failureReason))
            {
                UIFeedback.ShowStatus($"설치 불가: {failureReason}");
                return false;
            }

            PlaceableFurnitureData selectedFurniture = GetSelectedFurniture();
            EconomyManager.Instance.TrySpend(selectedFurniture.buildCost);

            slot.PlaceFurniture(selectedFurniture);
            RefreshBuildSlots();
            AudioManager.Instance?.PlayFurniturePlaced();
            UIFeedback.ShowStatus($"{selectedFurniture.displayName} 설치 완료");
            SaveManager.Instance?.SaveGame();
            ExitPlacementMode();
            return true;
        }

        public bool TryRemoveFromSlot(BuildSlot slot)
        {
            if (!IsPlacementModeActive || slot == null || !slot.HasPlacedFurniture)
            {
                return false;
            }

            string furnitureName = slot.CurrentFurnitureName;
            slot.RemoveFurniture();
            RefreshBuildSlots();
            UIFeedback.ShowStatus($"{furnitureName} 철거 완료");
            SaveManager.Instance?.SaveGame();
            return true;
        }

        private void RefreshBuildSlots()
        {
            BuildSlot[] slots = FindObjectsByType<BuildSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            PlaceableFurnitureData selectedFurniture = GetSelectedFurniture();

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].RefreshPlacementVisual(IsPlacementModeActive, selectedFurniture);
                }
            }
        }
    }
}
