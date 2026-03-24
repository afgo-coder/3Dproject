using MiniMart.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class CurrentDayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text dayText;
        [SerializeField] private Text legacyDayText;
        [SerializeField] private string prefix = "Day";

        private bool isSubscribed;

        private void Awake()
        {
            TryFindReferences();
        }

        private void OnEnable()
        {
            TryBind();
        }

        private void Start()
        {
            TryFindReferences();
            TryBind();
        }

        private void Update()
        {
            if (dayText == null && legacyDayText == null)
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
            if (isSubscribed && GameManager.Instance != null)
            {
                GameManager.Instance.DayChanged -= Refresh;
                isSubscribed = false;
            }
        }

        private void TryBind()
        {
            if (isSubscribed || GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.DayChanged += Refresh;
            isSubscribed = true;
            Refresh(GameManager.Instance.CurrentDay);
        }

        private void TryFindReferences()
        {
            UiTextUtility.TryAssignFromComponent(this, ref dayText, ref legacyDayText);
            UiTextUtility.TryAssignFromCanvasChild("DayCount", ref dayText, ref legacyDayText);
        }

        private void Refresh(int day)
        {
            string content = $"{prefix} {Mathf.Max(0, day)}";
            UiTextUtility.SetText(dayText, legacyDayText, content);
        }
    }
}
