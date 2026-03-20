using System;
using System.Collections.Generic;
using MiniMart.Data;
using MiniMart.Interaction;
using UnityEngine;
using UnityEngine.AI;

namespace MiniMart.Customer
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerAI : MonoBehaviour
    {
        private const string SpeedParameter = "Speed";

        private enum CustomerState
        {
            Browsing,
            GoingToShelf,
            GoingToCheckout,
            GoingToTrashCan,
            Leaving
        }

        [SerializeField] private Shelf targetShelf;
        [SerializeField] private CheckoutCounter checkoutCounter;

        private readonly List<ProductData> basket = new List<ProductData>();
        private NavMeshAgent agent;
        private Animator animator;
        private CustomerTypeData customerType;
        private ProductData targetProduct;
        private Transform exitPoint;
        private Action<CustomerAI> onExited;
        private TrashCan trashCan;
        private ProductData bottleReturnProduct;
        private float bottleReturnChance;
        private CustomerState state;
        private float browseTimer;
        private bool hasQueuedAtCheckout;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
        }

        public void Initialize(
            CustomerTypeData customerTypeData,
            Transform exitPointTransform,
            Action<CustomerAI> onExitedCallback,
            TrashCan assignedTrashCan,
            ProductData assignedBottleReturnProduct,
            float assignedBottleReturnChance)
        {
            customerType = customerTypeData;
            exitPoint = exitPointTransform;
            onExited = onExitedCallback;
            trashCan = assignedTrashCan;
            bottleReturnProduct = assignedBottleReturnProduct;
            bottleReturnChance = assignedBottleReturnChance;
            hasQueuedAtCheckout = false;

            if (customerType != null)
            {
                agent.speed = customerType.walkSpeed;
                browseTimer = customerType.browseDuration;
            }
            else
            {
                browseTimer = 2f;
            }

            state = CustomerState.Browsing;
        }

        private void Update()
        {
            UpdateAnimation();

            switch (state)
            {
                case CustomerState.Browsing:
                    UpdateBrowsing();
                    break;
                case CustomerState.GoingToShelf:
                    UpdateGoingToShelf();
                    break;
                case CustomerState.GoingToCheckout:
                    UpdateGoingToCheckout();
                    break;
                case CustomerState.GoingToTrashCan:
                    UpdateGoingToTrashCan();
                    break;
                case CustomerState.Leaving:
                    UpdateLeaving();
                    break;
            }
        }

        public void SetShoppingTargets(Shelf shelf, CheckoutCounter counter)
        {
            targetShelf = shelf;
            checkoutCounter = counter;
            targetProduct = shelf != null ? shelf.AssignedProduct : null;
        }

        public void ReceiveProduct(ProductData product)
        {
            if (product != null)
            {
                basket.Add(product);
            }
        }

        public int GetBasketPrice()
        {
            int total = 0;
            for (int i = 0; i < basket.Count; i++)
            {
                total += basket[i].salePrice;
            }

            return total;
        }

        public List<ProductData> GetBasketProducts()
        {
            return new List<ProductData>(basket);
        }

        public void CompleteCheckout()
        {
            if (ShouldVisitTrashCan())
            {
                agent.SetDestination(trashCan.GetDropPointPosition());
                state = CustomerState.GoingToTrashCan;
                return;
            }

            BeginLeaving();
        }

        private void UpdateBrowsing()
        {
            browseTimer -= Time.deltaTime;
            if (browseTimer > 0f)
            {
                return;
            }

            if (targetShelf != null)
            {
                agent.SetDestination(targetShelf.transform.position);
                state = CustomerState.GoingToShelf;
                return;
            }

            BeginLeaving();
        }

        private void UpdateGoingToShelf()
        {
            if (agent.pathPending || agent.remainingDistance > 1.25f)
            {
                return;
            }

            if (targetShelf != null)
            {
                targetShelf.TryTakeOne(this);
            }

            if (checkoutCounter != null)
            {
                agent.SetDestination(checkoutCounter.GetCustomerStandPosition());
                state = CustomerState.GoingToCheckout;
            }
            else
            {
                BeginLeaving();
            }
        }

        private void UpdateGoingToCheckout()
        {
            if (agent.pathPending || agent.remainingDistance > 0.25f)
            {
                return;
            }

            if (!hasQueuedAtCheckout && checkoutCounter != null && checkoutCounter.IsCustomerAtCounter(this))
            {
                checkoutCounter.NotifyCustomerArrived(this);
                hasQueuedAtCheckout = true;
            }
        }

        private void UpdateGoingToTrashCan()
        {
            if (agent.pathPending || agent.remainingDistance > 0.6f)
            {
                return;
            }

            trashCan?.AddBottleFromCustomer();
            BeginLeaving();
        }

        private void UpdateLeaving()
        {
            if (agent.pathPending || agent.remainingDistance > 1.25f)
            {
                return;
            }

            onExited?.Invoke(this);
            Destroy(gameObject);
        }

        private void BeginLeaving()
        {
            if (exitPoint != null)
            {
                agent.SetDestination(exitPoint.position);
            }

            state = CustomerState.Leaving;
        }

        private void UpdateAnimation()
        {
            if (animator == null)
            {
                return;
            }

            float speed = agent.velocity.magnitude;
            animator.SetFloat(SpeedParameter, speed);
        }

        public CustomerTypeData CustomerType => customerType;
        public ProductData TargetProduct => targetProduct;

        private bool ShouldVisitTrashCan()
        {
            if (trashCan == null || bottleReturnProduct == null || basket.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < basket.Count; i++)
            {
                if (basket[i] == bottleReturnProduct)
                {
                    return UnityEngine.Random.value <= bottleReturnChance;
                }
            }

            return false;
        }
    }
}
