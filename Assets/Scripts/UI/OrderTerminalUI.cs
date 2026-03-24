using System;
using MiniMart.Core;
using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class OrderTerminalUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text selectedProductText;
        [SerializeField] private Text legacySelectedProductText;
        [SerializeField] private TMP_Text productCostText;
        [SerializeField] private Text legacyProductCostText;
        [SerializeField] private TMP_Text expansionInfoText;
        [SerializeField] private Text legacyExpansionInfoText;
        [SerializeField] private TMP_Text placementInfoText;
        [SerializeField] private Text legacyPlacementInfoText;
        [SerializeField] private TMP_Text shadowWorkerInfoText;
        [SerializeField] private Text legacyShadowWorkerInfoText;
        [SerializeField] private TMP_FontAsset preferredFontAsset;

        private OrderTerminal currentTerminal;
        private int selectedProductIndex;

        private void Awake()
        {
            TryAutoBind();
        }

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
            TryAutoBind();
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
            OrderManager orderManager = UnityEngine.Object.FindFirstObjectByType<OrderManager>();
            if (orderManager == null)
            {
                UIFeedback.ShowStatus("발주에 실패했습니다. OrderManager를 찾을 수 없습니다.");
                return;
            }

            bool success = orderManager.PlaceOrder(product, currentTerminal.GetOrderAmount());
            UIFeedback.ShowStatus(
                success
                    ? $"{product.productName} {currentTerminal.GetOrderAmount()}개를 발주했습니다."
                    : "발주에 실패했습니다. 창고 공간을 확인해 주세요.");

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

        public void HireStockerShadow()
        {
            ShadowWorkerManager shadowWorkerManager = GetShadowWorkerManager();
            if (shadowWorkerManager == null)
            {
                UIFeedback.ShowStatus("분신 매니저가 연결되지 않았습니다.");
                return;
            }

            shadowWorkerManager.TryHireStockerShadow();
            Refresh();
        }

        public void HireCashierShadow()
        {
            ShadowWorkerManager shadowWorkerManager = GetShadowWorkerManager();
            if (shadowWorkerManager == null)
            {
                UIFeedback.ShowStatus("분신 매니저가 연결되지 않았습니다.");
                return;
            }

            shadowWorkerManager.TryHireCashierShadow();
            Refresh();
        }

        private void Refresh()
        {
            ApplyUnifiedPanelFont();
            RefreshProductInfo();
            RefreshExpansionInfo();
            RefreshPlacementInfo();
            RefreshShadowWorkerInfo();
        }

        private void RefreshProductInfo()
        {
            ProductData[] products = GetProducts();
            if (products.Length == 0)
            {
                SetText(selectedProductText, legacySelectedProductText, "상품 없음");
                SetText(productCostText, legacyProductCostText, string.Empty);
                return;
            }

            ProductData product = products[Mathf.Clamp(selectedProductIndex, 0, products.Length - 1)];
            int totalCost = product.costPrice * currentTerminal.GetOrderAmount();
            string featuredTag = StoreProgressionManager.Instance != null &&
                                 StoreProgressionManager.Instance.IsPromotionUnlocked &&
                                 product.category == StoreProgressionManager.Instance.FeaturedCategory
                ? " [추천]"
                : string.Empty;
            SetText(selectedProductText, legacySelectedProductText, $"상품: {product.productName}{featuredTag}");
            SetText(productCostText, legacyProductCostText, $"수량: {currentTerminal.GetOrderAmount()}개 | 비용: {totalCost:N0}원");
        }

        private void RefreshExpansionInfo()
        {
            if (currentTerminal == null || currentTerminal.GetExpansionManager() == null)
            {
                SetText(expansionInfoText, legacyExpansionInfoText, "확장 정보 없음");
                return;
            }

            StoreExpansionManager expansionManager = currentTerminal.GetExpansionManager();
            SetText(
                expansionInfoText,
                legacyExpansionInfoText,
                expansionManager.HasRemainingExpansion
                    ? $"다음 확장: {expansionManager.GetNextExpansionLabel()} ({expansionManager.GetNextExpansionCost():N0}원)"
                    : "모든 확장을 완료했습니다.");
        }

        private void RefreshPlacementInfo()
        {
            PlacementManager placementManager = GetPlacementManager();
            if (placementManager == null)
            {
                SetText(placementInfoText, legacyPlacementInfoText, "배치 정보 없음");
                return;
            }

            string modeText = placementManager.IsPlacementModeActive ? "활성" : "비활성";
            SetText(
                placementInfoText,
                legacyPlacementInfoText,
                $"배치 모드: {modeText}\n선택 가구: {placementManager.GetSelectedFurnitureName()} ({placementManager.GetSelectedFurnitureCost():N0}원)");
        }

        private void RefreshShadowWorkerInfo()
        {
            ShadowWorkerManager shadowWorkerManager = GetShadowWorkerManager();
            if (shadowWorkerManager == null)
            {
                SetText(shadowWorkerInfoText, legacyShadowWorkerInfoText, "분신 정보 없음");
                return;
            }

            SetText(
                shadowWorkerInfoText,
                legacyShadowWorkerInfoText,
                $"운반 분신: {shadowWorkerManager.StockerCount}/{shadowWorkerManager.MaxStockers}명 ({shadowWorkerManager.StockerCost:N0}원)\n계산 분신: {shadowWorkerManager.CashierCount}/{shadowWorkerManager.MaxCashiers}명 ({shadowWorkerManager.CashierCost:N0}원)");
        }

        private ProductData[] GetProducts()
        {
            return currentTerminal != null ? currentTerminal.GetAvailableProducts() : Array.Empty<ProductData>();
        }

        private PlacementManager GetPlacementManager()
        {
            return currentTerminal != null ? currentTerminal.GetPlacementManager() : null;
        }

        private ShadowWorkerManager GetShadowWorkerManager()
        {
            return currentTerminal != null ? currentTerminal.GetShadowWorkerManager() : null;
        }

        private void TryAutoBind()
        {
            if (panel == null)
            {
                panel = gameObject;
            }

            AutoBind(ref selectedProductText, ref legacySelectedProductText, "SelectedProductText");
            AutoBind(ref productCostText, ref legacyProductCostText, "ProductCostText");
            AutoBind(ref expansionInfoText, ref legacyExpansionInfoText, "ExpansionInfoText");
            AutoBind(ref placementInfoText, ref legacyPlacementInfoText, "PlacementInfoText");
            AutoBind(ref shadowWorkerInfoText, ref legacyShadowWorkerInfoText, "ShadowWorkerInfoText");

            if (shadowWorkerInfoText == null && legacyShadowWorkerInfoText == null)
            {
                AutoBind(ref shadowWorkerInfoText, ref legacyShadowWorkerInfoText, "ShadowWorker");
            }

            if (preferredFontAsset == null)
            {
                preferredFontAsset = FindPreferredFontAsset();
            }

            ApplyUnifiedPanelFont();
        }

        private TMP_FontAsset FindPreferredFontAsset()
        {
            if (selectedProductText != null && selectedProductText.font != null)
            {
                return selectedProductText.font;
            }

            TMP_Text[] sceneTexts = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < sceneTexts.Length; i++)
            {
                TMP_Text text = sceneTexts[i];
                if (text == null || text.font == null)
                {
                    continue;
                }

                if (text.font.name.Contains("RiaSans", StringComparison.OrdinalIgnoreCase))
                {
                    return text.font;
                }
            }

            return TMP_Settings.defaultFontAsset;
        }

        private void ApplyUnifiedPanelFont()
        {
            if (panel == null)
            {
                return;
            }

            TMP_FontAsset fontAsset = preferredFontAsset != null ? preferredFontAsset : FindPreferredFontAsset();
            if (fontAsset == null)
            {
                return;
            }

            TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (text == null)
                {
                    continue;
                }

                text.font = fontAsset;
                if (string.IsNullOrWhiteSpace(text.text) || string.Equals(text.text, "New Text", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }
        }

        private void AutoBind(ref TMP_Text tmpText, ref Text legacyText, string childName)
        {
            if (panel == null)
            {
                return;
            }

            Transform child = panel.transform.Find(childName);
            if (child == null)
            {
                return;
            }

            if (tmpText == null)
            {
                tmpText = child.GetComponent<TMP_Text>();
            }

            if (legacyText == null)
            {
                legacyText = child.GetComponent<Text>();
            }
        }

        private static void SetText(TMP_Text tmpText, Text legacyText, string content)
        {
            if (tmpText != null)
            {
                tmpText.text = content;
            }

            if (legacyText != null)
            {
                legacyText.text = content;
            }
        }
    }
}
