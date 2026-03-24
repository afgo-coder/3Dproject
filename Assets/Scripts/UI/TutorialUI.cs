using MiniMart.Core;
using MiniMart.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniMart.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text tutorialText;
        [SerializeField] private Text legacyTutorialText;
        [SerializeField] private string title = "튜토리얼";

        private void Awake()
        {
            TryFindReferences();
            EnsureTutorialManagerExists();
        }

        private void OnEnable()
        {
            SubscribeEvents();
            Refresh();
        }

        private void OnDisable()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.TutorialStateChanged -= Refresh;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ModalStateChanged -= HandleModalStateChanged;
            }
        }

        private void Update()
        {
            Refresh();
        }

        private void HandleModalStateChanged(bool _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (tutorialText == null && legacyTutorialText == null)
            {
                TryFindReferences();
            }

            TutorialManager tutorialManager = EnsureTutorialManagerExists();
            bool shouldHide = tutorialManager == null || (GameManager.Instance != null && GameManager.Instance.IsModalOpen);
            bool shouldShow = !shouldHide && tutorialManager.ShouldShowTutorial();

            SetVisible(shouldShow);

            if (!shouldShow)
            {
                SetContent(string.Empty);
                return;
            }

            string content =
                $"{title}\n" +
                $"{BuildLine(tutorialManager.HasOpenedOrderTerminal, "운영 패널 열기")}\n" +
                $"{BuildLine(tutorialManager.HasPlacedOrder, "상품 발주하기")}\n" +
                $"{BuildLine(tutorialManager.HasStockedShelf, "선반 채우기")}\n" +
                $"{BuildLine(tutorialManager.HasCompletedCheckout, "손님 계산 완료하기")}";

            SetContent(content);
        }

        private void TryFindReferences()
        {
            UiTextUtility.TryAssignFromComponent(this, ref tutorialText, ref legacyTutorialText);
            UiTextUtility.TryAssignFromCanvasChild("TutorialText", ref tutorialText, ref legacyTutorialText);
        }

        private TutorialManager EnsureTutorialManagerExists()
        {
            if (TutorialManager.Instance != null)
            {
                return TutorialManager.Instance;
            }

            TutorialManager found = FindFirstObjectByType<TutorialManager>();
            if (found != null)
            {
                return found;
            }

            GameObject bootstrap = GameObject.Find("Bootstrap");
            if (bootstrap == null)
            {
                bootstrap = new GameObject("Bootstrap");
            }

            TutorialManager created = bootstrap.GetComponent<TutorialManager>();
            if (created == null)
            {
                created = bootstrap.AddComponent<TutorialManager>();
            }

            return created;
        }

        private void SubscribeEvents()
        {
            TutorialManager tutorialManager = EnsureTutorialManagerExists();
            if (tutorialManager != null)
            {
                tutorialManager.TutorialStateChanged -= Refresh;
                tutorialManager.TutorialStateChanged += Refresh;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ModalStateChanged -= HandleModalStateChanged;
                GameManager.Instance.ModalStateChanged += HandleModalStateChanged;
            }
        }

        private static string BuildLine(bool completed, string label)
        {
            return $"{(completed ? "■" : "□")} {label}";
        }

        private void SetContent(string content)
        {
            UiTextUtility.SetText(tutorialText, legacyTutorialText, content);
        }

        private void SetVisible(bool visible)
        {
            UiTextUtility.SetVisible(tutorialText, legacyTutorialText, visible);
        }
    }
}
