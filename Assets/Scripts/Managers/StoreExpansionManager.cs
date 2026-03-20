using System;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Managers
{
    [Serializable]
    public class StoreExpansionStep
    {
        public string label = "확장";
        public int cost = 10000;
        public GameObject sectionRoot;
        public GameObject blockerToDisable;
    }

    public class StoreExpansionManager : MonoBehaviour
    {
        [SerializeField] private int startingExpansionLevel = 0;
        [SerializeField] private StoreExpansionStep[] expansionSteps;

        private int _currentExpansionIndex;

        public bool HasRemainingExpansion => expansionSteps != null && _currentExpansionIndex < expansionSteps.Length;
        public int CurrentExpansionLevel => _currentExpansionIndex;

        private void Start()
        {
            _currentExpansionIndex = Mathf.Clamp(startingExpansionLevel, 0, expansionSteps != null ? expansionSteps.Length : 0);

            if (expansionSteps == null)
            {
                return;
            }

            for (int i = 0; i < expansionSteps.Length; i++)
            {
                if (expansionSteps[i].sectionRoot != null)
                {
                    expansionSteps[i].sectionRoot.SetActive(i < _currentExpansionIndex);
                }

                if (expansionSteps[i].blockerToDisable != null)
                {
                    expansionSteps[i].blockerToDisable.SetActive(i >= _currentExpansionIndex);
                }
            }
        }

        public bool TryBuyNextExpansion()
        {
            if (!HasRemainingExpansion)
            {
                UIFeedback.ShowStatus("이미 모든 매장 확장을 완료했습니다.");
                return false;
            }

            StoreExpansionStep step = expansionSteps[_currentExpansionIndex];
            if (!EconomyManager.Instance || !EconomyManager.Instance.TrySpend(step.cost))
            {
                UIFeedback.ShowStatus($"확장 실패: {step.cost:N0}원이 필요합니다.");
                return false;
            }

            if (step.sectionRoot != null)
            {
                step.sectionRoot.SetActive(true);
            }

            if (step.blockerToDisable != null)
            {
                step.blockerToDisable.SetActive(false);
            }

            UIFeedback.ShowStatus($"매장 확장 완료: {step.label}");
            _currentExpansionIndex++;
            return true;
        }

        public string GetNextExpansionLabel()
        {
            return HasRemainingExpansion ? expansionSteps[_currentExpansionIndex].label : "확장 없음";
        }

        public int GetNextExpansionCost()
        {
            return HasRemainingExpansion ? expansionSteps[_currentExpansionIndex].cost : 0;
        }
    }
}
