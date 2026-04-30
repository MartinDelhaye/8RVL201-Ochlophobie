using UnityEngine;

namespace Ochlophobia.Environment
{
    /// Porte commandée uniquement par DoorButton.OpenDoor().
    /// Attacher sur le pivot de la porte (l'axe de rotation).
    /// Ajoute automatiquement un BoxCollider si la porte n'en a pas,
    /// ce qui empêche le joueur de traverser quand elle est fermée.
    public class AutoDoor : MonoBehaviour
    {
        [SerializeField] private float openAngle  = 90f;
        [SerializeField] private float speed      = 3f;
        [SerializeField] private float closeDelay = 3f;

        [Header("Collider de blocage")]
        [Tooltip("Ajoute un BoxCollider si aucun n'est présent sur ce GO")]
        [SerializeField] private bool  addColliderIfMissing = true;
        [Tooltip("Taille du collider en espace local (largeur, hauteur, épaisseur)")]
        [SerializeField] private Vector3 colliderSize   = new Vector3(1.0f, 2.4f, 0.08f);
        [Tooltip("Décalage du centre par rapport au pivot")]
        [SerializeField] private Vector3 colliderCenter = new Vector3(0.5f, 1.2f, 0f);

        private float _closedAngle;
        private float _targetAngle;
        private float _currentAngle;
        private float _closeTimer;
        private bool  _isOpen;

        private void Awake()
        {
            if (addColliderIfMissing && GetComponent<Collider>() == null)
            {
                var box    = gameObject.AddComponent<BoxCollider>();
                box.size   = colliderSize;
                box.center = colliderCenter;
            }
        }

        private void Start()
        {
            _closedAngle  = transform.localEulerAngles.y;
            _targetAngle  = _closedAngle;
            _currentAngle = _closedAngle;
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

        public void OpenDoor()
        {
            _targetAngle = _closedAngle + openAngle;
            _closeTimer  = closeDelay;
            _isOpen      = true;
        }
    }
}
