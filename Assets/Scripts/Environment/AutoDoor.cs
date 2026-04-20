using UnityEngine;

namespace Ochlophobia.Environment
{
    /// Automatic door that swings open when the player gets close, then closes after a delay.
    /// Attach to the door pivot GameObject. Works by rotating the door around its local Y axis.
    public class AutoDoor : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float triggerDistance = 2.5f;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float speed = 3f;
        [SerializeField] private float closeDelay = 1.5f;

        private float _closedAngle;
        private float _targetAngle;
        private float _currentAngle;
        private float _closeTimer;
        private bool _isOpen;

        private void Start()
        {
            _closedAngle = transform.localEulerAngles.y;
            _targetAngle = _closedAngle;
            _currentAngle = _closedAngle;

            if (player == null)
            {
                var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null) player = xrOrigin.transform;
            }
        }

        private void Update()
        {
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.position);
            bool playerNear = dist <= triggerDistance;

            if (playerNear)
            {
                _targetAngle = _closedAngle + openAngle;
                _closeTimer = closeDelay;
                _isOpen = true;
            }
            else if (_isOpen)
            {
                _closeTimer -= Time.deltaTime;
                if (_closeTimer <= 0f)
                {
                    _targetAngle = _closedAngle;
                    if (Mathf.Abs(_currentAngle - _closedAngle) < 0.5f)
                        _isOpen = false;
                }
            }

            _currentAngle = Mathf.LerpAngle(_currentAngle, _targetAngle, Time.deltaTime * speed);
            var euler = transform.localEulerAngles;
            euler.y = _currentAngle;
            transform.localEulerAngles = euler;
        }
    }
}
