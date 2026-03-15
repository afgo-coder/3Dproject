using MiniMart.Data;
using MiniMart.Managers;
using MiniMart.Player;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class StorageBox : Interactable
    {
        [SerializeField] private ProductData product;
        [SerializeField] private int remainingAmount;

        public ProductData Product => product;
        public int RemainingAmount => remainingAmount;

        public override string GetInteractionPrompt()
        {
            string productName = product == null ? "알 수 없는 상품" : product.productName;
            return $"[E] {productName} 꺼내기 ({remainingAmount}개 남음)";
        }

        public void Initialize(ProductData targetProduct, int amount)
        {
            product = targetProduct;
            remainingAmount = amount;
        }

        public override void Interact(GameObject interactor)
        {
            PlayerInteractor playerInteractor = interactor.GetComponent<PlayerInteractor>();
            if (playerInteractor == null || product == null || remainingAmount <= 0)
            {
                UIFeedback.ShowStatus("박스 상호작용에 실패했습니다.");
                return;
            }

            OrderManager orderManager = FindFirstObjectByType<OrderManager>();
            if (orderManager == null)
            {
                UIFeedback.ShowStatus("박스 처리 실패: OrderManager를 찾지 못했습니다.");
                return;
            }

            if (!orderManager.TryTakeFromStorage(product, 1))
            {
                UIFeedback.ShowStatus($"{product.productName} 창고 재고가 없습니다.");
                return;
            }

            if (playerInteractor.TryHoldProduct(product))
            {
                remainingAmount--;
                UIFeedback.ShowStatus(
                    $"{product.productName}을(를) 집었습니다. 박스 잔량: {remainingAmount}");
                if (remainingAmount <= 0)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                orderManager.AddStorageStock(product, 1);
                UIFeedback.ShowStatus("손이 가득 차 있습니다.");
            }
        }
    }
}
