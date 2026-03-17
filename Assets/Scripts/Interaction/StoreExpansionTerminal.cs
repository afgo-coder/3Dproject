using MiniMart.Managers;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class StoreExpansionTerminal : Interactable
    {
        [SerializeField] private StoreExpansionManager expansionManager;

        public override string GetInteractionPrompt()
        {
            if (expansionManager == null)
            {
                return "[E] 확장 단말기 (매니저 미연결)";
            }

            return expansionManager.HasRemainingExpansion
                ? $"[E] {expansionManager.GetNextExpansionLabel()} 확장 ({expansionManager.GetNextExpansionCost():N0}원)"
                : "[E] 모든 확장 완료";
        }

        public override void Interact(GameObject interactor)
        {
            if (expansionManager == null)
            {
                UIFeedback.ShowStatus("확장 단말기가 연결되지 않았습니다.");
                return;
            }

            expansionManager.TryBuyNextExpansion();
        }
    }
}
