using MiniMart.Data;
using MiniMart.Managers;
using MiniMart.Tutorial;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class OrderTerminal : Interactable
    {
        [SerializeField] private ProductData[] availableProducts;
        [SerializeField] private int orderAmount = 5;
        [SerializeField] private OrderTerminalUI terminalUI;
        [SerializeField] private StoreExpansionManager expansionManager;
        [SerializeField] private PlacementManager placementManager;
        [SerializeField] private ShadowWorkerManager shadowWorkerManager;

        public override string GetInteractionPrompt()
        {
            return "[E] 운영 패널 열기";
        }

        public override void Interact(GameObject interactor)
        {
            if (terminalUI == null)
            {
                UIFeedback.ShowStatus("운영 패널이 연결되지 않았습니다.");
                return;
            }

            TutorialManager.Instance?.NotifyOrderTerminalOpened();
            terminalUI.Open(this);
        }

        public ProductData[] GetAvailableProducts()
        {
            return availableProducts ?? System.Array.Empty<ProductData>();
        }

        public int GetOrderAmount()
        {
            return orderAmount + (StoreProgressionManager.Instance != null ? StoreProgressionManager.Instance.GetAdditionalOrderAmount() : 0);
        }

        public StoreExpansionManager GetExpansionManager()
        {
            return expansionManager;
        }

        public PlacementManager GetPlacementManager()
        {
            return placementManager;
        }

        public ShadowWorkerManager GetShadowWorkerManager()
        {
            return shadowWorkerManager;
        }
    }
}
