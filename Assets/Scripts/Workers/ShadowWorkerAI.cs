using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.Managers;
using UnityEngine;
using UnityEngine.AI;

namespace MiniMart.Workers
{
    public enum ShadowWorkerRole
    {
        Stocker,
        Cashier
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class ShadowWorkerAI : MonoBehaviour
    {
        private const string SpeedParameter = "Speed";

        private enum StockerState
        {
            Idle,
            MovingToBox,
            MovingToShelf
        }

        [SerializeField] private float stockerRefreshInterval = 1.5f;
        [SerializeField] private float cashierProcessInterval = 1f;
        [SerializeField] private float shelfReachDistance = 1.15f;
        [SerializeField] private float boxReachDistance = 1f;
        [SerializeField] private float homeReachDistance = 0.3f;

        private NavMeshAgent agent;
        private Animator animator;
        private ShadowWorkerManager manager;
        private Transform homePoint;
        private Shelf targetShelf;
        private StorageBox targetBox;
        private float actionTimer;
        private ShadowWorkerRole role;
        private StockerState stockerState;
        private ProductData carriedProduct;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
        }

        public void Initialize(ShadowWorkerRole assignedRole, ShadowWorkerManager assignedManager, Transform assignedHomePoint)
        {
            role = assignedRole;
            manager = assignedManager;
            homePoint = assignedHomePoint;
            actionTimer = 0f;

            if (homePoint != null)
            {
                agent.Warp(homePoint.position);
                agent.SetDestination(homePoint.position);
            }

            stockerState = StockerState.Idle;
            targetShelf = null;
            targetBox = null;
            carriedProduct = null;
        }

        private void Update()
        {
            UpdateAnimation();

            switch (role)
            {
                case ShadowWorkerRole.Stocker:
                    UpdateStocker();
                    break;
                case ShadowWorkerRole.Cashier:
                    UpdateCashier();
                    break;
            }
        }

        private void UpdateStocker()
        {
            actionTimer -= Time.deltaTime;

            if (targetShelf == null && carriedProduct == null && targetBox == null && actionTimer > 0f)
            {
                return;
            }

            switch (stockerState)
            {
                case StockerState.MovingToBox:
                    UpdateStockerMovingToBox();
                    break;
                case StockerState.MovingToShelf:
                    UpdateStockerMovingToShelf();
                    break;
                default:
                    TryStartRestockTask();
                    break;
            }
        }

        private void TryStartRestockTask()
        {
            targetShelf = manager != null ? manager.FindShelfToRestock() : null;
            if (targetShelf == null || targetShelf.AssignedProduct == null)
            {
                actionTimer = stockerRefreshInterval;
                ReturnToHome();
                return;
            }

            targetBox = manager.FindStorageBox(targetShelf.AssignedProduct);
            if (targetBox == null)
            {
                targetShelf = null;
                actionTimer = stockerRefreshInterval;
                ReturnToHome();
                return;
            }

            stockerState = StockerState.MovingToBox;
            agent.SetDestination(targetBox.transform.position);
        }

        private void UpdateStockerMovingToBox()
        {
            if (targetShelf == null || targetBox == null || targetShelf.AssignedProduct == null)
            {
                ResetStockerTask();
                return;
            }

            if (!targetBox.CanProvide(targetShelf.AssignedProduct))
            {
                ResetStockerTask();
                return;
            }

            if (agent.pathPending || agent.remainingDistance > boxReachDistance)
            {
                return;
            }

            if (!targetBox.TryTakeOneForWorker())
            {
                ResetStockerTask();
                return;
            }

            carriedProduct = targetShelf.AssignedProduct;
            stockerState = StockerState.MovingToShelf;
            agent.SetDestination(targetShelf.transform.position);
        }

        private void UpdateStockerMovingToShelf()
        {
            if (targetShelf == null || carriedProduct == null)
            {
                ResetStockerTask();
                return;
            }

            if (agent.pathPending || agent.remainingDistance > shelfReachDistance)
            {
                return;
            }

            if (targetShelf.AssignedProduct == carriedProduct)
            {
                targetShelf.TryRestockDirect(1);
            }

            carriedProduct = null;
            targetBox = null;

            if (targetShelf.NeedsRestock)
            {
                StorageBox nextBox = manager != null ? manager.FindStorageBox(targetShelf.AssignedProduct) : null;
                if (nextBox != null)
                {
                    targetBox = nextBox;
                    stockerState = StockerState.MovingToBox;
                    agent.SetDestination(targetBox.transform.position);
                    return;
                }
            }

            ResetStockerTask();
        }

        private void ResetStockerTask()
        {
            stockerState = StockerState.Idle;
            targetShelf = null;
            targetBox = null;
            carriedProduct = null;
            actionTimer = stockerRefreshInterval;
            ReturnToHome();
        }

        private void UpdateCashier()
        {
            if (manager == null)
            {
                return;
            }

            CheckoutCounter counter = manager.GetCheckoutCounter();
            if (counter == null)
            {
                ReturnToHome();
                return;
            }

            Vector3 standPosition = homePoint != null ? homePoint.position : counter.transform.position;
            if (!IsNearPosition(standPosition, homeReachDistance))
            {
                if (!agent.pathPending && agent.remainingDistance <= homeReachDistance)
                {
                    agent.ResetPath();
                }
                else
                {
                    agent.SetDestination(standPosition);
                }

                return;
            }

            agent.ResetPath();

            actionTimer -= Time.deltaTime;
            if (actionTimer > 0f)
            {
                return;
            }

            if (counter.HasWaitingCustomers)
            {
                counter.TryProcessNextCustomer(false);
            }

            actionTimer = cashierProcessInterval;
        }

        private void ReturnToHome()
        {
            if (homePoint != null && !IsNearPosition(homePoint.position, homeReachDistance))
            {
                agent.SetDestination(homePoint.position);
                return;
            }

            if (agent != null && !agent.pathPending)
            {
                agent.ResetPath();
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null || agent == null)
            {
                return;
            }

            animator.SetFloat(SpeedParameter, agent.velocity.magnitude);
        }

        private bool IsNearPosition(Vector3 position, float distance)
        {
            Vector3 currentPosition = transform.position;
            currentPosition.y = position.y;
            return Vector3.Distance(currentPosition, position) <= distance;
        }
    }
}
