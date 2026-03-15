using UnityEngine;

namespace MiniMart.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool IsDayRunning { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void StartDay()
        {
            IsDayRunning = true;
            Time.timeScale = 1f;
        }

        public void EndDay()
        {
            IsDayRunning = false;
            Time.timeScale = 0f;
        }
    }
}
