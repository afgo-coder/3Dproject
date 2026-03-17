using MiniMart.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class CurrentDayUI : MonoBehaviour
    {
        [SerializeField] private Text dayText;
        [SerializeField] private string prefix = "Day";

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
            if (_isSubscribed && GameManager.Instance != null)
            {
                GameManager.Instance.DayChanged -= Refresh;
                _isSubscribed = false;
            }
        }

        private void TryBind()
        {
            if (_isSubscribed || GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.DayChanged += Refresh;
            _isSubscribed = true;
            Refresh(GameManager.Instance.CurrentDay);
        }

        private void Refresh(int day)
        {
            if (dayText == null)
            {
                return;
            }

            dayText.text = $"{prefix} {Mathf.Max(0, day)}";
        }
    }
}
