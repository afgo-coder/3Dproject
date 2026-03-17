using MiniMart.Data;
using MiniMart.Managers;
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

            terminalUI.Open(this);
        }

        public ProductData[] GetAvailableProducts()
        {
            return availableProducts ?? System.Array.Empty<ProductData>();
        }

        public int GetOrderAmount()
        {
            return orderAmount;
        }

        public StoreExpansionManager GetExpansionManager()
        {
            return expansionManager;
        }
    }
}
