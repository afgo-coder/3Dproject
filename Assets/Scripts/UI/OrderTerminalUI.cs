using MiniMart.Core;
using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class OrderTerminalUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text selectedProductText;
        [SerializeField] private Text productCostText;
        [SerializeField] private Text expansionInfoText;
        [SerializeField] private Text placementInfoText;

        private OrderTerminal currentTerminal;
        private int selectedProductIndex;

        private void Start()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void Update()
        {
            if (currentTerminal == null || panel == null || !panel.activeSelf)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        public void Open(OrderTerminal terminal)
        {
            currentTerminal = terminal;
            selectedProductIndex = 0;

            if (panel != null)
            {
                panel.SetActive(true);
            }

            GameManager.Instance?.SetModalOpen(true);
            Refresh();
        }

        public void Close()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            currentTerminal = null;
            GameManager.Instance?.SetModalOpen(false);
        }

        public void SelectPreviousProduct()
        {
            ProductData[] products = GetProducts();
            if (products.Length == 0)
            {
                return;
            }

            selectedProductIndex = (selectedProductIndex - 1 + products.Length) % products.Length;
            Refresh();
        }

        public void SelectNextProduct()
        {
            ProductData[] products = GetProducts();
            if (products.Length == 0)
            {
                return;
            }

            selectedProductIndex = (selectedProductIndex + 1) % products.Length;
            Refresh();
        }

        public void OrderSelectedProduct()
        {
            if (currentTerminal == null)
            {
                return;
            }

            ProductData[] products = GetProducts();
            if (products.Length == 0)
            {
                UIFeedback.ShowStatus("발주 가능한 상품이 없습니다.");
                return;
            }

            ProductData product = products[Mathf.Clamp(selectedProductIndex, 0, products.Length - 1)];
            OrderManager orderManager = Object.FindFirstObjectByType<OrderManager>();
            if (orderManager == null)
            {
                UIFeedback.ShowStatus("발주 실패: OrderManager를 찾지 못했습니다.");
                return;
            }

            bool success = orderManager.PlaceOrder(product, currentTerminal.GetOrderAmount());
            UIFeedback.ShowStatus(
                success
                    ? $"{product.productName} {currentTerminal.GetOrderAmount()}개를 발주했습니다."
                    : "발주 실패: 돈이 부족하거나 창고 자리가 없습니다.");

            Refresh();
        }

        public void BuyNextExpansion()
        {
            if (currentTerminal == null)
            {
                return;
            }

            StoreExpansionManager expansionManager = currentTerminal.GetExpansionManager();
            if (expansionManager == null)
            {
                UIFeedback.ShowStatus("확장 매니저가 연결되지 않았습니다.");
                return;
            }

            expansionManager.TryBuyNextExpansion();
            Refresh();
        }

        public void SelectPreviousFurniture()
        {
            PlacementManager placementManager = GetPlacementManager();
            if (placementManager == null)
            {
                UIFeedback.ShowStatus("배치 매니저가 연결되지 않았습니다.");
                return;
            }

            placementManager.SelectPreviousFurniture();
            Refresh();
        }

        public void SelectNextFurniture()
        {
            PlacementManager placementManager = GetPlacementManager();
            if (placementManager == null)
            {
                UIFeedback.ShowStatus("배치 매니저가 연결되지 않았습니다.");
                return;
            }

            placementManager.SelectNextFurniture();
            Refresh();
        }

        public void EnterPlacementMode()
        {
            PlacementManager placementManager = GetPlacementManager();
            if (placementManager == null)
            {
                UIFeedback.ShowStatus("배치 매니저가 연결되지 않았습니다.");
                return;
            }

            placementManager.EnterPlacementMode();
            Close();
        }

        public void ExitPlacementMode()
        {
            PlacementManager placementManager = GetPlacementManager();
            if (placementManager == null)
            {
                return;
            }

            placementManager.ExitPlacementMode();
            Refresh();
        }

        private void Refresh()
        {
            RefreshProductInfo();
            RefreshExpansionInfo();
            RefreshPlacementInfo();
        }

        private void RefreshProductInfo()
        {
            if (selectedProductText == null || productCostText == null)
            {
                return;
            }

            ProductData[] products = GetProducts();
            if (products.Length == 0)
            {
                selectedProductText.text = "발주 상품 없음";
                productCostText.text = string.Empty;
                return;
            }

            ProductData product = products[Mathf.Clamp(selectedProductIndex, 0, products.Length - 1)];
            int totalCost = product.costPrice * currentTerminal.GetOrderAmount();
            selectedProductText.text = $"상품: {product.productName}";
            productCostText.text = $"수량: {currentTerminal.GetOrderAmount()}개  |  비용: {totalCost:N0}원";
        }

        private void RefreshExpansionInfo()
        {
            if (expansionInfoText == null)
            {
                return;
            }

            if (currentTerminal == null || currentTerminal.GetExpansionManager() == null)
            {
                expansionInfoText.text = "확장 정보 없음";
                return;
            }

            StoreExpansionManager expansionManager = currentTerminal.GetExpansionManager();
            expansionInfoText.text = expansionManager.HasRemainingExpansion
                ? $"다음 확장: {expansionManager.GetNextExpansionLabel()} ({expansionManager.GetNextExpansionCost():N0}원)"
                : "모든 확장 완료";
        }

        private void RefreshPlacementInfo()
        {
            if (placementInfoText == null)
            {
                return;
            }

            PlacementManager placementManager = GetPlacementManager();
            if (placementManager == null)
            {
                placementInfoText.text = "배치 정보 없음";
                return;
            }

            string modeText = placementManager.IsPlacementModeActive ? "활성" : "비활성";
            placementInfoText.text =
                $"배치 모드: {modeText}\n" +
                $"선택 가구: {placementManager.GetSelectedFurnitureName()} ({placementManager.GetSelectedFurnitureCost():N0}원)";
        }

        private ProductData[] GetProducts()
        {
            return currentTerminal != null ? currentTerminal.GetAvailableProducts() : System.Array.Empty<ProductData>();
        }

        private PlacementManager GetPlacementManager()
        {
            return currentTerminal != null ? currentTerminal.GetPlacementManager() : null;
        }
    }
}
