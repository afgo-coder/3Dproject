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
            Leaving
        }

        [SerializeField] private Shelf targetShelf;
        [SerializeField] private CheckoutCounter checkoutCounter;

        private readonly List<ProductData> _basket = new List<ProductData>();
        private NavMeshAgent _agent;
        private Animator _animator;
        private CustomerTypeData _customerType;
        private ProductData _targetProduct;
        private Transform _exitPoint;
        private Action<CustomerAI> _onExited;
        private CustomerState _state;
        private float _browseTimer;
        private bool _hasQueuedAtCheckout;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
        }

        public void Initialize(CustomerTypeData customerType, Transform exitPoint, Action<CustomerAI> onExited)
        {
            _customerType = customerType;
            _exitPoint = exitPoint;
            _onExited = onExited;
            _hasQueuedAtCheckout = false;

            if (_customerType != null)
            {
                _agent.speed = _customerType.walkSpeed;
                _browseTimer = _customerType.browseDuration;
            }
            else
            {
                _browseTimer = 2f;
            }

            _state = CustomerState.Browsing;
        }

        private void Update()
        {
            UpdateAnimation();

            switch (_state)
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
                case CustomerState.Leaving:
                    UpdateLeaving();
                    break;
            }
        }

        public void SetShoppingTargets(Shelf shelf, CheckoutCounter counter)
        {
            targetShelf = shelf;
            checkoutCounter = counter;
            _targetProduct = shelf != null ? shelf.AssignedProduct : null;
        }

        public void ReceiveProduct(ProductData product)
        {
            if (product != null)
            {
                _basket.Add(product);
            }
        }

        public int GetBasketPrice()
        {
            int total = 0;
            for (int i = 0; i < _basket.Count; i++)
            {
                total += _basket[i].salePrice;
            }

            return total;
        }

        public List<ProductData> GetBasketProducts()
        {
            return new List<ProductData>(_basket);
        }

        public void CompleteCheckout()
        {
            BeginLeaving();
        }

        private void UpdateBrowsing()
        {
            _browseTimer -= Time.deltaTime;
            if (_browseTimer > 0f)
            {
                return;
            }

            if (targetShelf != null)
            {
                _agent.SetDestination(targetShelf.transform.position);
                _state = CustomerState.GoingToShelf;
                return;
            }

            BeginLeaving();
        }

        private void UpdateGoingToShelf()
        {
            if (_agent.pathPending || _agent.remainingDistance > 1.25f)
            {
                return;
            }

            if (targetShelf != null)
            {
                targetShelf.TryTakeOne(this);
            }

            if (checkoutCounter != null)
            {
                _agent.SetDestination(checkoutCounter.GetCustomerStandPosition());
                _state = CustomerState.GoingToCheckout;
            }
            else
            {
                BeginLeaving();
            }
        }

        private void UpdateGoingToCheckout()
        {
            if (_agent.pathPending || _agent.remainingDistance > 0.25f)
            {
                return;
            }

            if (!_hasQueuedAtCheckout && checkoutCounter != null && checkoutCounter.IsCustomerAtCounter(this))
            {
                checkoutCounter.NotifyCustomerArrived(this);
                _hasQueuedAtCheckout = true;
            }
        }

        private void UpdateLeaving()
        {
            if (_agent.pathPending || _agent.remainingDistance > 1.25f)
            {
                return;
            }

            _onExited?.Invoke(this);
            Destroy(gameObject);
        }

        private void BeginLeaving()
        {
            if (_exitPoint != null)
            {
                _agent.SetDestination(_exitPoint.position);
            }

            _state = CustomerState.Leaving;
        }

        private void UpdateAnimation()
        {
            if (_animator == null)
            {
                return;
            }

            float speed = _agent.velocity.magnitude;
            _animator.SetFloat(SpeedParameter, speed);
        }

        public CustomerTypeData CustomerType => _customerType;
        public ProductData TargetProduct => _targetProduct;
    }
}
