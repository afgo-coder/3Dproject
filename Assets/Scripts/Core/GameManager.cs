using System;
using UnityEngine;

namespace MiniMart.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool IsDayRunning { get; private set; }
        public bool IsModalOpen { get; private set; }
        public int CurrentDay { get; private set; }

        public event Action<int> DayChanged;
        public event Action<bool> DayRunningStateChanged;
        public event Action<bool> ModalStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CurrentDay = 0;
            ApplyCursorState(false, false);
        }

        public void BeginDay(int dayNumber)
        {
            CurrentDay = Mathf.Max(0, dayNumber);
            IsDayRunning = true;
            IsModalOpen = false;
            Time.timeScale = 1f;
            ApplyCursorState(true, false);
            DayChanged?.Invoke(CurrentDay);
            DayRunningStateChanged?.Invoke(true);
            ModalStateChanged?.Invoke(false);
        }

        public void EndDay()
        {
            IsDayRunning = false;
            IsModalOpen = false;
            Time.timeScale = 0f;
            ApplyCursorState(false, false);
            DayRunningStateChanged?.Invoke(false);
            ModalStateChanged?.Invoke(false);
        }

        public void SetModalOpen(bool isOpen)
        {
            IsModalOpen = isOpen;
            Time.timeScale = IsDayRunning && !IsModalOpen ? 1f : 0f;
            ApplyCursorState(IsDayRunning, IsModalOpen);
            ModalStateChanged?.Invoke(IsModalOpen);
        }

        private static void ApplyCursorState(bool isGameplay, bool isModalOpen)
        {
            bool lockCursor = isGameplay && !isModalOpen;
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;
        }
    }
}
