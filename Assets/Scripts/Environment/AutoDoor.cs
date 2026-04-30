using UnityEngine;

namespace Ochlophobia.Environment
{
    /// Porte pivotante contrôlée par un bouton (DoorButton).
    /// Attach to the door pivot GameObject.
    public class AutoDoor : MonoBehaviour
    {
        [SerializeField] private float openAngle  = 90f;
        [SerializeField] private float speed      = 3f;
        [SerializeField] private float closeDelay = 3f;

        private float _closedAngle;
        private float _targetAngle;
        private float _currentAngle;
        private float _closeTimer;
        private bool  _isOpen;

        private void Start()
        {
            _closedAngle  = transform.localEulerAngles.y;
            _targetAngle  = _closedAngle;
            _currentAngle = _closedAngle;
        }

        /// Appelé par DoorButton quand le joueur appuie.
        public void OpenDoor()
        {
            _targetAngle = _closedAngle + openAngle;
            _closeTimer  = closeDelay;
            _isOpen      = true;
        }

        private void Update()
        {
            if (_isOpen)
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
