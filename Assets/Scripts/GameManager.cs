using UnityEngine;
using UnityEngine.Events;

namespace Ochlophobia
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Timer")]
        [SerializeField] private float duration = 60f;

        [Header("Events")]
        public UnityEvent onGameStart;
        public UnityEvent onGameWin;
        public UnityEvent onGameLose;

        public float TimeLeft { get; private set; }
        public bool IsRunning { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartGame()
        {
            TimeLeft = duration;
            IsRunning = true;
            onGameStart.Invoke();
        }

        private void Update()
        {
            if (!IsRunning) return;

            TimeLeft -= Time.deltaTime;

            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                IsRunning = false;
                onGameLose.Invoke();
            }
        }

        /// <summary>
        /// Appelle cette méthode depuis le trigger du quai quand le joueur monte dans le train.
        /// </summary>
        public void PlayerReachedTrain()
        {
            if (!IsRunning) return;
            IsRunning = false;
            onGameWin.Invoke();
        }
    }
}
