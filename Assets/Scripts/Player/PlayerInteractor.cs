using MiniMart.Data;
using MiniMart.Interaction;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private LayerMask interactionMask = Physics.DefaultRaycastLayers;

        public ProductData HeldProduct { get; private set; }

        private Interactable _currentInteractable;

        private void Update()
        {
            UpdateFocus();

            if (Input.GetKeyDown(KeyCode.E))
            {
                TryInteract();
            }
        }

        public bool TryHoldProduct(ProductData product)
        {
            if (product == null || HeldProduct != null)
            {
                return false;
            }

            HeldProduct = product;
            string productName = string.IsNullOrWhiteSpace(product.productName) ? product.name : product.productName;
            UIFeedback.ShowStatus($"{productName}을(를) 들었습니다.");
            return true;
        }

        public bool TryConsumeHeldItem()
        {
            if (HeldProduct == null)
            {
                return false;
            }

            string productName = string.IsNullOrWhiteSpace(HeldProduct.productName) ? HeldProduct.name : HeldProduct.productName;
            HeldProduct = null;
            UIFeedback.ShowStatus($"{productName}을(를) 내려놓았습니다.");
            return true;
        }

        private void UpdateFocus()
        {
            if (playerCamera == null)
            {
                _currentInteractable = null;
                UIFeedback.SetInteractionPrompt("PlayerInteractor에 카메라가 연결되지 않았습니다.");
                return;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionMask))
            {
                _currentInteractable = hit.collider.GetComponentInParent<Interactable>();
                UIFeedback.SetInteractionPrompt(
                    _currentInteractable != null
                        ? _currentInteractable.GetInteractionPrompt()
                        : string.Empty);
                return;
            }

            _currentInteractable = null;
            UIFeedback.SetInteractionPrompt(string.Empty);
        }

        private void TryInteract()
        {
            if (_currentInteractable == null)
            {
                UIFeedback.ShowStatus("상호작용할 대상이 없습니다.");
                return;
            }

            Debug.Log($"상호작용: {_currentInteractable.DisplayName}");
            _currentInteractable.Interact(gameObject);
        }
    }
}
