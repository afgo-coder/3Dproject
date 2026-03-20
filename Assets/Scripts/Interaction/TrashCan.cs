using MiniMart.Managers;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class TrashCan : Interactable
    {
        [SerializeField] private Transform dropPoint;
        [SerializeField] private float passiveBottleIntervalSeconds = 30f;
        [SerializeField] private int bottleValue = 500;

        private float passiveBottleTimer;

        public int CurrentBottleCount { get; private set; }
        public int BottleValue => bottleValue;

        private void Start()
        {
            passiveBottleTimer = passiveBottleIntervalSeconds;
        }

        private void Update()
        {
            DayCycleManager dayCycle = FindFirstObjectByType<DayCycleManager>();
            if (dayCycle == null || !dayCycle.IsRunning || dayCycle.IsPreparationDay)
            {
                return;
            }

            passiveBottleTimer -= Time.deltaTime;
            if (passiveBottleTimer > 0f)
            {
                return;
            }

            passiveBottleTimer = passiveBottleIntervalSeconds;
            AddBottle();
        }

        public override string GetInteractionPrompt()
        {
            return $"[E] 공병 회수함 ({CurrentBottleCount}개)";
        }

        public override void Interact(GameObject interactor)
        {
            UIFeedback.ShowStatus($"현재 공병 {CurrentBottleCount}개가 쌓여 있습니다.");
        }

        public Vector3 GetDropPointPosition()
        {
            return dropPoint != null ? dropPoint.position : transform.position;
        }

        public void AddBottleFromCustomer()
        {
            AddBottle();
            UIFeedback.ShowStatus("손님이 공병을 버리고 갔습니다.");
        }

        public void SettleDailyBottleReturn()
        {
            EconomyManager.Instance?.ApplyBottleReturnSettlement(CurrentBottleCount, bottleValue);
            CurrentBottleCount = 0;
        }

        private void AddBottle()
        {
            CurrentBottleCount++;
        }
    }
}
