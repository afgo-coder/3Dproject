using System.Collections.Generic;
using MiniMart.Customer;
using MiniMart.Data;
using MiniMart.Managers;
using MiniMart.Tutorial;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class CheckoutCounter : Interactable
    {
        [SerializeField] private Transform customerStandPoint;
        [SerializeField] private float checkoutRange = 0.35f;

        private readonly Queue<CustomerAI> _waitingCustomers = new Queue<CustomerAI>();
        private readonly HashSet<CustomerAI> _readyCustomers = new HashSet<CustomerAI>();
        public bool HasWaitingCustomers => _waitingCustomers.Count > 0;

        public override string GetInteractionPrompt()
        {
            return _readyCustomers.Count > 0
                ? $"[E] 계산하기 ({_readyCustomers.Count}명 대기 중)"
                : "[E] 계산대 (도착한 손님 없음)";
        }

        public override void Interact(GameObject interactor)
        {
            TryProcessNextCustomer(true);
        }

        public void NotifyCustomerArrived(CustomerAI customer)
        {
            if (customer == null || _readyCustomers.Contains(customer))
            {
                return;
            }

            if (!IsCustomerAtCounter(customer))
            {
                return;
            }

            _readyCustomers.Add(customer);
            _waitingCustomers.Enqueue(customer);
            UIFeedback.ShowStatus("손님이 계산대 앞에 도착했습니다.");
        }

        public Vector3 GetCustomerStandPosition()
        {
            return customerStandPoint != null ? customerStandPoint.position : transform.position;
        }

        public bool IsCustomerAtCounter(CustomerAI customer)
        {
            if (customer == null)
            {
                return false;
            }

            Vector3 targetPosition = GetCustomerStandPosition();
            Vector3 currentPosition = customer.transform.position;
            currentPosition.y = targetPosition.y;
            return Vector3.Distance(currentPosition, targetPosition) <= checkoutRange;
        }

        private void RemoveInvalidCustomers()
        {
            while (_waitingCustomers.Count > 0 && _waitingCustomers.Peek() == null)
            {
                _waitingCustomers.Dequeue();
            }
        }

        public bool TryProcessNextCustomer(bool showFeedback)
        {
            RemoveInvalidCustomers();
            if (_waitingCustomers.Count == 0)
            {
                if (showFeedback)
                {
                    UIFeedback.ShowStatus("계산대 앞에 도착한 손님이 없습니다.");
                }

                return false;
            }

            CustomerAI customer = _waitingCustomers.Dequeue();
            _readyCustomers.Remove(customer);
            int totalPrice = customer.GetBasketPrice();
            List<ProductData> soldProducts = customer.GetBasketProducts();

            EconomyManager.Instance?.AddSale(totalPrice);
            if (EconomyManager.Instance != null)
            {
                for (int i = 0; i < soldProducts.Count; i++)
                {
                    EconomyManager.Instance.RecordProductSale(soldProducts[i]);
                }

                EconomyManager.Instance.RecordCustomerServed();
            }

            customer.CompleteCheckout();
            TutorialManager.Instance?.NotifyCheckoutCompleted();
            AudioManager.Instance?.PlayCheckout();

            if (showFeedback)
            {
                UIFeedback.ShowStatus($"결제가 완료되었습니다. +{totalPrice:N0}원");
            }

            SaveManager.Instance?.SaveGame();
            return true;
        }
    }
}
