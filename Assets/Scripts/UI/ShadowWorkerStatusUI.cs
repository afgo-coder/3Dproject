using MiniMart.Core;
using MiniMart.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class ShadowWorkerStatusUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Text legacyStatusText;
        [SerializeField] private ShadowWorkerManager shadowWorkerManager;
        [SerializeField] private string title = "분신 상태";

        private void Awake()
        {
            TryFindReferences();
        }

        private void Update()
        {
            if (statusText == null && legacyStatusText == null)
            {
                TryFindReferences();
            }

            if (shadowWorkerManager == null)
            {
                shadowWorkerManager = Object.FindFirstObjectByType<ShadowWorkerManager>();
            }

            bool shouldHide = shadowWorkerManager == null || (GameManager.Instance != null && GameManager.Instance.IsModalOpen);
            SetVisible(!shouldHide);

            if (shouldHide)
            {
                SetContent(string.Empty);
                return;
            }

            SetContent(
                $"{title}\n" +
                $"진열: {shadowWorkerManager.StockerCount}/{shadowWorkerManager.MaxStockers}\n" +
                $"계산: {shadowWorkerManager.CashierCount}/{shadowWorkerManager.MaxCashiers}");
        }

        private void TryFindReferences()
        {
            UiTextUtility.TryAssignFromComponent(this, ref statusText, ref legacyStatusText);
            UiTextUtility.TryAssignFromCanvasChild("WorkerStatusText", ref statusText, ref legacyStatusText);
        }

        private void SetContent(string content)
        {
            UiTextUtility.SetText(statusText, legacyStatusText, content);
        }

        private void SetVisible(bool visible)
        {
            UiTextUtility.SetVisible(statusText, legacyStatusText, visible);
        }
    }
}
