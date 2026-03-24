using MiniMart.Customer;
using MiniMart.Data;
using MiniMart.Managers;
using MiniMart.Tutorial;
using MiniMart.UI;
using TMPro;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class Shelf : Interactable
    {
        [SerializeField] private ProductData assignedProduct;
        [SerializeField] private int currentStock;
        [SerializeField] private TextMeshPro priceLabel;
        [SerializeField] private Vector3 priceLabelLocalPosition = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 priceLabelLocalRotation = Vector3.zero;
        [SerializeField] private float priceLabelFontSize = 3f;

        public ProductData AssignedProduct => assignedProduct;
        public int CurrentStock => currentStock;
        public int MaxStock => assignedProduct != null ? assignedProduct.maxShelfCapacity : 0;
        public int MissingStock => Mathf.Max(0, MaxStock - currentStock);
        public bool NeedsRestock => assignedProduct != null && currentStock < MaxStock;

        private void Awake()
        {
            EnsurePriceLabel();
            RefreshPriceLabel();
        }

        private void OnEnable()
        {
            EnsurePriceLabel();
            RefreshPriceLabel();
        }

        private void OnValidate()
        {
            RefreshPriceLabel();
        }

        public override string GetInteractionPrompt()
        {
            if (assignedProduct == null)
            {
                return "[E] 선반: 빈 선반";
            }

            return $"[E] 선반: {assignedProduct.productName} ({currentStock}/{MaxStock}) / 판매가 {assignedProduct.salePrice:N0}원";
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
                RefreshPriceLabel();
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
                SaveManager.Instance?.SaveGame();
                TutorialManager.Instance?.NotifyShelfStocked();
                UIFeedback.ShowStatus($"{assignedProduct.productName} 진열 완료. 선반 재고: {currentStock}/{MaxStock}");
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
            UIFeedback.ShowStatus($"손님이 {assignedProduct.productName}을(를) 집었습니다. 선반 재고: {currentStock}/{MaxStock}");
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

            OrderManager orderManager = FindFirstObjectByType<OrderManager>();
            if (orderManager == null || !orderManager.TryTakeFromStorage(assignedProduct, moveAmount))
            {
                return false;
            }

            currentStock += moveAmount;
            SaveManager.Instance?.SaveGame();
            return true;
        }

        public bool TryRestockDirect(int amount)
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

            currentStock += moveAmount;
            SaveManager.Instance?.SaveGame();
            return true;
        }

        public void RestoreState(ProductData product, int stock)
        {
            assignedProduct = product;
            currentStock = Mathf.Max(0, stock);
            RefreshPriceLabel();
        }

        private void EnsurePriceLabel()
        {
            if (priceLabel != null)
            {
                return;
            }

            Transform existing = transform.Find("AutoPriceLabel");
            if (existing != null)
            {
                priceLabel = existing.GetComponent<TextMeshPro>();
                if (priceLabel != null)
                {
                    return;
                }
            }

            GameObject labelObject = new GameObject("AutoPriceLabel");
            labelObject.transform.SetParent(transform, false);
            priceLabel = labelObject.AddComponent<TextMeshPro>();
            priceLabel.alignment = TextAlignmentOptions.Center;
            priceLabel.color = Color.white;
            priceLabel.enableWordWrapping = false;
            priceLabel.transform.localScale = Vector3.one * 0.1f;
        }

        private void RefreshPriceLabel()
        {
            if (priceLabel == null)
            {
                return;
            }

            priceLabel.transform.localPosition = priceLabelLocalPosition;
            priceLabel.transform.localEulerAngles = priceLabelLocalRotation;
            priceLabel.fontSize = priceLabelFontSize;
            priceLabel.text = assignedProduct != null ? $"{assignedProduct.salePrice:N0}원" : "가격 미정";
        }
    }
}
