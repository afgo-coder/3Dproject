using MiniMart.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class MoneyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private Text legacyMoneyText;
        [SerializeField] private string prefix = "보유 금액";

        private bool isSubscribed;

        private void Awake()
        {
            TryFindReferences();
        }

        private void OnEnable()
        {
            TryFindReferences();
            TryBind();
        }

        private void Start()
        {
            TryFindReferences();
            TryBind();
        }

        private void Update()
        {
            if (moneyText == null && legacyMoneyText == null)
            {
                TryFindReferences();
            }

            if (!isSubscribed)
            {
                TryBind();
            }
        }

        private void OnDisable()
        {
            if (isSubscribed && EconomyManager.Instance != null)
            {
                EconomyManager.Instance.MoneyChanged -= Refresh;
                isSubscribed = false;
            }
        }

        private void TryBind()
        {
            if (isSubscribed || EconomyManager.Instance == null)
            {
                return;
            }

            EconomyManager.Instance.MoneyChanged += Refresh;
            isSubscribed = true;
            Refresh(EconomyManager.Instance.CurrentMoney);
        }

        private void TryFindReferences()
        {
            UiTextUtility.TryAssignFromComponent(this, ref moneyText, ref legacyMoneyText);
            UiTextUtility.TryAssignFromCanvasChild("MoneyText", ref moneyText, ref legacyMoneyText);
        }

        private void Refresh(int value)
        {
            string content = $"{prefix}: {value:N0}원";
            UiTextUtility.SetText(moneyText, legacyMoneyText, content);
        }
    }
}
