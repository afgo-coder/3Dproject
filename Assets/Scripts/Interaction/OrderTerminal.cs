using MiniMart.Data;
using MiniMart.Managers;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class OrderTerminal : Interactable
    {
        [SerializeField] private ProductData defaultProduct;
        [SerializeField] private int orderAmount = 5;

        public override string GetInteractionPrompt()
        {
            if (defaultProduct == null)
            {
                return "[E] 발주 단말기 (상품 미설정)";
            }

            return $"[E] {defaultProduct.productName} {orderAmount}개 발주";
        }

        public override void Interact(GameObject interactor)
        {
            if (defaultProduct == null)
            {
                UIFeedback.ShowStatus("발주 실패: 기본 상품이 지정되지 않았습니다.");
                return;
            }

            OrderManager orderManager = FindFirstObjectByType<OrderManager>();
            if (orderManager == null)
            {
                UIFeedback.ShowStatus("발주 실패: OrderManager를 찾지 못했습니다.");
                return;
            }

            bool success = orderManager.PlaceOrder(defaultProduct, orderAmount);
            string productName = string.IsNullOrWhiteSpace(defaultProduct.productName) ? defaultProduct.name : defaultProduct.productName;
            UIFeedback.ShowStatus(
                success
                    ? $"{productName} {orderAmount}개를 발주했습니다."
                    : "발주 실패: 돈이 부족하거나 설정이 올바르지 않습니다.");
        }
    }
}
