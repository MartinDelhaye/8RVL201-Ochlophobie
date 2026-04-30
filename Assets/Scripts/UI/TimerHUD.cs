using UnityEngine;
using TMPro;
using Unity.XR.CoreUtils;

namespace Ochlophobia.UI
{
    /// Compte à rebours flottant. Se positionne automatiquement en haut à droite du champ de vision.
    /// S'affiche dès que GameManager.IsRunning passe à true.
    public class TimerHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerLabel;

        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color urgentColor = Color.red;
        [SerializeField] private float urgentThreshold = 15f;

        [Header("Positionnement")]
        [SerializeField] private float distanceFromCamera = 1.8f;
        [SerializeField] private Vector2 viewOffset = new Vector2(0.35f, 0.1f);

        private CanvasGroup _group;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
        }

        private void Start()
        {
            PositionInFrontOfPlayer();
        }

        private void PositionInFrontOfPlayer()
        {
            Transform cam = null;
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
                cam = xrOrigin.Camera.transform;
            else if (Camera.main != null)
                cam = Camera.main.transform;
            if (cam == null) return;

            Vector3 forward = cam.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, -forward).normalized;
            transform.position = cam.position
                + forward * distanceFromCamera
                + right * viewOffset.x
                + Vector3.up * viewOffset.y;
            transform.rotation = Quaternion.LookRotation(forward);
        }

        private void Update()
        {
            if (GameManager.Instance == null || timerLabel == null) return;

            bool running = GameManager.Instance.IsRunning;
            _group.alpha = running ? 1f : 0f;
            if (!running) return;

            float t = GameManager.Instance.TimeLeft;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            timerLabel.text = $"{minutes}:{seconds:00}";
            timerLabel.color = t <= urgentThreshold ? urgentColor : normalColor;
        }
    }
}
