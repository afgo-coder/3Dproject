using MiniMart.Customer;
using MiniMart.Data;
using MiniMart.Managers;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class Shelf : Interactable
    {
        [SerializeField] private ProductData assignedProduct;
        [SerializeField] private int currentStock;

        public ProductData AssignedProduct => assignedProduct;
        public int CurrentStock => currentStock;
        public int MaxStock => assignedProduct != null ? assignedProduct.maxShelfCapacity : 0;

        public override string GetInteractionPrompt()
        {
            string productName = assignedProduct == null ? "빈 선반" : assignedProduct.productName;
            return $"[E] 선반: {productName} ({currentStock}/{MaxStock})";
        }

        public override void Interact(GameObject interactor)
        {
            Player.PlayerInteractor playerInteractor = interactor.GetComponent<Player.PlayerInteractor>();
            if (playerInteractor == null)
            {
                UIFeedback.ShowStatus("선반 상호작용 실패: PlayerInteractor가 없습니다.");
                return;
            }

            if (assignedProduct == null)
            {
                assignedProduct = playerInteractor.HeldProduct;
            }

            if (assignedProduct == null || playerInteractor.HeldProduct != assignedProduct)
            {
                UIFeedback.ShowStatus(
                    assignedProduct == null
                        ? "선반이 비어 있습니다. 먼저 박스에서 상품을 들어주세요."
                        : $"{assignedProduct.productName}을(를) 들고 있어야 이 선반을 채울 수 있습니다.");
                return;
            }

            if (currentStock >= MaxStock)
            {
                UIFeedback.ShowStatus("선반이 이미 가득 찼습니다.");
                return;
            }

            if (playerInteractor.TryConsumeHeldItem())
            {
                currentStock++;
                UIFeedback.ShowStatus(
                    $"{assignedProduct.productName} 진열 완료. 선반 재고: {currentStock}/{MaxStock}");
            }
        }

        public bool CanServe(ProductData product)
        {
            return assignedProduct == product && currentStock > 0;
        }

        public bool TryTakeOne(CustomerAI customer)
        {
            if (assignedProduct == null || currentStock <= 0)
            {
                return false;
            }

            currentStock--;
            customer.ReceiveProduct(assignedProduct);
            UIFeedback.ShowStatus(
                $"손님이 {assignedProduct.productName}을(를) 집었습니다. 선반 재고: {currentStock}/{MaxStock}");
            return true;
        }

        public bool TryRestockFromStorage(int amount)
        {
            if (assignedProduct == null || amount <= 0)
            {
                return false;
            }

            int freeSpace = MaxStock - currentStock;
            int moveAmount = Mathf.Min(freeSpace, amount);
            if (moveAmount <= 0)
            {
                return false;
            }

            if (!FindFirstObjectByType<OrderManager>().TryTakeFromStorage(assignedProduct, moveAmount))
            {
                return false;
            }

            currentStock += moveAmount;
            return true;
        }
    }
}
