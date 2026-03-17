using MiniMart.Core;
using UnityEngine;

namespace MiniMart.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private Transform cameraRoot;

        private CharacterController _characterController;
        private float _pitch;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (GameManager.Instance != null && (!GameManager.Instance.IsDayRunning || GameManager.Instance.IsModalOpen))
            {
                return;
            }

            HandleLook();
            HandleMovement();
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            transform.Rotate(Vector3.up * mouseX);
            _pitch = Mathf.Clamp(_pitch - mouseY, -80f, 80f);

            if (cameraRoot != null)
            {
                cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 move = (transform.forward * vertical + transform.right * horizontal).normalized;
            _characterController.SimpleMove(move * moveSpeed);
        }
    }
}
