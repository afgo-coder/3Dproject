using MiniMart.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class MoneyUI : MonoBehaviour
    {
        [SerializeField] private Text moneyText;
        [SerializeField] private string prefix = "보유 금액";

        private bool _isSubscribed;

        private void OnEnable()
        {
            TryBind();
        }

        private void Start()
        {
            TryBind();
        }

        private void Update()
        {
            if (!_isSubscribed)
            {
                TryBind();
            }
        }

        private void OnDisable()
        {
            if (_isSubscribed && EconomyManager.Instance != null)
            {
                EconomyManager.Instance.MoneyChanged -= Refresh;
                _isSubscribed = false;
            }
        }

        private void TryBind()
        {
            if (_isSubscribed || EconomyManager.Instance == null)
            {
                return;
            }

            EconomyManager.Instance.MoneyChanged += Refresh;
            _isSubscribed = true;
            Refresh(EconomyManager.Instance.CurrentMoney);
        }

        private void Refresh(int value)
        {
            if (moneyText == null)
            {
                return;
            }

            moneyText.text = $"{prefix}: {value:N0}원";
        }
    }
}
