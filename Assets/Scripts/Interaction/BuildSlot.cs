using MiniMart.Data;
using MiniMart.Managers;
using MiniMart.UI;
using UnityEngine;

namespace MiniMart.Interaction
{
    public class BuildSlot : Interactable
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private Transform previewRoot;
        [SerializeField] private GameObject buildAreaVisual;
        [SerializeField] private Material previewMaterial;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private GameObject placedObject;
        [SerializeField] private PlaceableFurnitureData placedFurnitureData;

        private GameObject previewObject;

        public bool HasPlacedFurniture => placedObject != null;
        public string CurrentFurnitureName => placedFurnitureData != null ? placedFurnitureData.displayName : "설치된 가구";
        public PlaceableFurnitureData PlacedFurnitureData => placedFurnitureData;
        public Shelf PlacedShelf => placedObject != null ? placedObject.GetComponent<Shelf>() : null;

        private void Awake()
        {
            if (interactionCollider == null)
            {
                interactionCollider = GetComponent<Collider>();
            }
        }

        private void Start()
        {
            PlacementManager placementManager = PlacementManager.Instance;
            RefreshPlacementVisual(
                placementManager != null && placementManager.IsPlacementModeActive,
                placementManager != null ? placementManager.GetSelectedFurniture() : null);
        }

        private void OnEnable()
        {
            PlacementManager placementManager = PlacementManager.Instance;
            RefreshPlacementVisual(
                placementManager != null && placementManager.IsPlacementModeActive,
                placementManager != null ? placementManager.GetSelectedFurniture() : null);
        }

        public override string GetInteractionPrompt()
        {
            PlacementManager placementManager = PlacementManager.Instance;
            if (placementManager == null)
            {
                return "배치 매니저 없음";
            }

            if (!placementManager.IsPlacementModeActive)
            {
                return "[E] 배치 모드를 먼저 시작하세요";
            }

            if (HasPlacedFurniture)
            {
                return $"[E] {CurrentFurnitureName} 철거";
            }

            string failureReason = placementManager.GetPlacementFailureReason(this);
            if (!string.IsNullOrWhiteSpace(failureReason))
            {
                return $"[E] 설치 불가 ({failureReason})";
            }

            return $"[E] {placementManager.GetSelectedFurnitureName()} 설치 ({placementManager.GetSelectedFurnitureCost():N0}원)";
        }

        public override void Interact(GameObject interactor)
        {
            PlacementManager placementManager = PlacementManager.Instance;
            if (placementManager == null)
            {
                UIFeedback.ShowStatus("배치 매니저가 존재하지 않습니다.");
                return;
            }

            if (!placementManager.IsPlacementModeActive)
            {
                UIFeedback.ShowStatus("먼저 운영 패널에서 배치 모드를 시작하세요.");
                return;
            }

            if (HasPlacedFurniture)
            {
                placementManager.TryRemoveFromSlot(this);
                return;
            }

            string failureReason = placementManager.GetPlacementFailureReason(this);
            if (!string.IsNullOrWhiteSpace(failureReason))
            {
                UIFeedback.ShowStatus($"설치 불가: {failureReason}");
                return;
            }

            placementManager.TryPlaceAtSlot(this);
        }

        public void PlaceFurniture(PlaceableFurnitureData furnitureData)
        {
            if (furnitureData == null || furnitureData.prefab == null)
            {
                return;
            }

            RemoveFurniture();

            Transform parent = contentRoot != null ? contentRoot : transform;
            placedObject = Object.Instantiate(furnitureData.prefab, parent);
            ApplyPlacementTransform(placedObject.transform, furnitureData);
            placedFurnitureData = furnitureData;

            RefreshPlacementVisual(
                PlacementManager.Instance != null && PlacementManager.Instance.IsPlacementModeActive,
                PlacementManager.Instance != null ? PlacementManager.Instance.GetSelectedFurniture() : null);
        }

        public void RemoveFurniture()
        {
            if (placedObject != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(placedObject);
                }
                else
                {
                    Object.DestroyImmediate(placedObject);
                }
            }

            placedObject = null;
            placedFurnitureData = null;
        }

        public void RefreshPlacementVisual(bool placementModeActive, PlaceableFurnitureData selectedFurniture)
        {
            if (interactionCollider != null)
            {
                interactionCollider.enabled = placementModeActive;
            }

            if (buildAreaVisual != null)
            {
                buildAreaVisual.SetActive(placementModeActive && !HasPlacedFurniture);
            }

            if (!placementModeActive || HasPlacedFurniture || selectedFurniture == null || selectedFurniture.prefab == null)
            {
                DestroyPreview();
                return;
            }

            EnsurePreview(selectedFurniture);
        }

        private void EnsurePreview(PlaceableFurnitureData selectedFurniture)
        {
            if (previewObject != null)
            {
                PreviewMarker existingMarker = previewObject.GetComponent<PreviewMarker>();
                if (existingMarker != null && existingMarker.SourceFurniture == selectedFurniture)
                {
                    return;
                }

                DestroyPreview();
            }

            Transform parent = previewRoot != null ? previewRoot : (contentRoot != null ? contentRoot : transform);
            previewObject = Object.Instantiate(selectedFurniture.prefab, parent);
            ApplyPlacementTransform(previewObject.transform, selectedFurniture);
            previewObject.name = $"{selectedFurniture.displayName}_Preview";
            previewObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            PreviewMarker marker = previewObject.AddComponent<PreviewMarker>();
            marker.SourceFurniture = selectedFurniture;

            foreach (Transform child in previewObject.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }

            foreach (Collider collider in previewObject.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }

            foreach (MonoBehaviour behaviour in previewObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != null && behaviour != marker)
                {
                    behaviour.enabled = false;
                }
            }

            if (previewMaterial != null)
            {
                foreach (Renderer renderer in previewObject.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] materials = renderer.sharedMaterials;
                    Material[] previewMaterials = new Material[materials.Length];
                    for (int i = 0; i < previewMaterials.Length; i++)
                    {
                        previewMaterials[i] = previewMaterial;
                    }

                    renderer.sharedMaterials = previewMaterials;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }
        }

        private void DestroyPreview()
        {
            if (previewObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(previewObject);
            }
            else
            {
                Object.DestroyImmediate(previewObject);
            }

            previewObject = null;
        }

        private static void ApplyPlacementTransform(Transform target, PlaceableFurnitureData furnitureData)
        {
            if (target == null || furnitureData == null || furnitureData.prefab == null)
            {
                return;
            }

            Transform prefabTransform = furnitureData.prefab.transform;
            target.localPosition = new Vector3(
                prefabTransform.localPosition.x,
                furnitureData.localYOffset,
                prefabTransform.localPosition.z);
            target.localRotation = prefabTransform.localRotation * Quaternion.Euler(furnitureData.localEulerOffset);
            target.localScale = prefabTransform.localScale;
        }

        private sealed class PreviewMarker : MonoBehaviour
        {
            public PlaceableFurnitureData SourceFurniture { get; set; }
        }
    }
}
