using UnityEngine;

namespace MiniMart.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        private bool hasOpenedOrderTerminal;
        private bool hasPlacedOrder;
        private bool hasStockedShelf;
        private bool hasCompletedCheckout;

        public bool HasOpenedOrderTerminal => hasOpenedOrderTerminal;
        public bool HasPlacedOrder => hasPlacedOrder;
        public bool HasStockedShelf => hasStockedShelf;
        public bool HasCompletedCheckout => hasCompletedCheckout;
        public bool IsTutorialComplete => HasOpenedOrderTerminal && HasPlacedOrder && HasStockedShelf && HasCompletedCheckout;

        public event System.Action TutorialStateChanged;
        public event System.Action TutorialCompleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            hasOpenedOrderTerminal = false;
            hasPlacedOrder = false;
            hasStockedShelf = false;
            hasCompletedCheckout = false;
        }

        public void NotifyOrderTerminalOpened()
        {
            SetFlag(ref hasOpenedOrderTerminal);
        }

        public void NotifyOrderPlaced()
        {
            SetFlag(ref hasPlacedOrder);
        }

        public void NotifyShelfStocked()
        {
            SetFlag(ref hasStockedShelf);
        }

        public void NotifyCheckoutCompleted()
        {
            SetFlag(ref hasCompletedCheckout);
        }

        public bool ShouldShowTutorial()
        {
            return !IsTutorialComplete;
        }

        public void ResetTutorial()
        {
            hasOpenedOrderTerminal = false;
            hasPlacedOrder = false;
            hasStockedShelf = false;
            hasCompletedCheckout = false;
            TutorialStateChanged?.Invoke();
        }

        public void RestoreState(bool openedOrderTerminal, bool placedOrder, bool stockedShelf, bool completedCheckout)
        {
            hasOpenedOrderTerminal = openedOrderTerminal;
            hasPlacedOrder = placedOrder;
            hasStockedShelf = stockedShelf;
            hasCompletedCheckout = completedCheckout;
            TutorialStateChanged?.Invoke();
        }

        private void SetFlag(ref bool flag)
        {
            if (flag)
            {
                return;
            }

            flag = true;
            TutorialStateChanged?.Invoke();

            if (IsTutorialComplete)
            {
                TutorialCompleted?.Invoke();
            }
        }
    }
}
