using System.Collections.Generic;
using MiniMart.Customer;
using MiniMart.Managers;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class CheckoutCounter : Interactable
    {
        private readonly Queue<CustomerAI> _waitingCustomers = new Queue<CustomerAI>();

        public override string GetInteractionPrompt()
        {
            return _waitingCustomers.Count > 0
                ? $"[E] 계산하기 ({_waitingCustomers.Count}명 대기 중)"
                : "[E] 계산대 (대기 손님 없음)";
        }

        public override void Interact(GameObject interactor)
        {
            if (_waitingCustomers.Count == 0)
            {
                UIFeedback.ShowStatus("계산대에 대기 중인 손님이 없습니다.");
                return;
            }

            CustomerAI customer = _waitingCustomers.Dequeue();
            int totalPrice = customer.GetBasketPrice();
            EconomyManager.Instance?.AddSale(totalPrice);
            customer.CompleteCheckout();
            UIFeedback.ShowStatus($"결제가 완료되었습니다. +{totalPrice:N0}원");
        }

        public void RegisterCustomer(CustomerAI customer)
        {
            if (customer != null && !_waitingCustomers.Contains(customer))
            {
                _waitingCustomers.Enqueue(customer);
                UIFeedback.ShowStatus("손님이 계산대에서 기다리고 있습니다.");
            }
        }
    }
}
