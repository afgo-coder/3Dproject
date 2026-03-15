using UnityEngine;

namespace MiniMart.Interaction
{
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] private string displayName = "Interactable";

        public string DisplayName => displayName;

        public virtual string GetInteractionPrompt()
        {
            return $"[E] {displayName}";
        }

        public abstract void Interact(GameObject interactor);
    }
}
