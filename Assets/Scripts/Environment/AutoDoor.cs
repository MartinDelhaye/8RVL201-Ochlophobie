using UnityEngine;
using UnityEngine.AI;
using Unity.XR.CoreUtils;

namespace Ochlophobia.Environment
{
    /// Porte automatique qui s'ouvre quand le joueur ou un PNJ s'approche.
    /// Attache ce script sur le pivot de la porte (l'axe de rotation).
    public class AutoDoor : MonoBehaviour
    {
        [SerializeField] private float triggerDistance = 2.5f;
        [SerializeField] private float openAngle      = 90f;
        [SerializeField] private float speed          = 3f;
        [SerializeField] private float closeDelay     = 1.5f;

        private float _closedAngle;
        private float _targetAngle;
        private float _currentAngle;
        private float _closeTimer;
        private bool  _isOpen;

        private Transform _player;

        // Cache partagé entre toutes les portes pour éviter FindObjectsOfType chaque frame
        private static NavMeshAgent[] _agentCache;
        private static float          _cacheTimestamp;
        private const  float          CacheInterval = 3f;

        private void Start()
        {
            _closedAngle  = transform.localEulerAngles.y;
            _targetAngle  = _closedAngle;
            _currentAngle = _closedAngle;

            // Cherche le joueur par tag puis par XROrigin en fallback
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
                _player = playerGO.transform;
            else
            {
                var origin = FindObjectOfType<XROrigin>();
                if (origin != null) _player = origin.transform;
            }
        }

        private void Update()
        {
            if (IsAnythingNear())
            {
                _targetAngle = _closedAngle + openAngle;
                _closeTimer  = closeDelay;
                _isOpen      = true;
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

        private bool IsAnythingNear()
        {
            // Joueur
            if (_player != null &&
                Vector3.Distance(transform.position, _player.position) <= triggerDistance)
                return true;

            // PNJ : raffraîchissement du cache toutes les CacheInterval secondes
            if (Time.time - _cacheTimestamp > CacheInterval)
            {
                _agentCache     = FindObjectsOfType<NavMeshAgent>();
                _cacheTimestamp = Time.time;
            }

            if (_agentCache != null)
            {
                foreach (var agent in _agentCache)
                {
                    if (agent != null &&
                        Vector3.Distance(transform.position, agent.transform.position) <= triggerDistance)
                        return true;
                }
            }

            return false;
        }
    }
}
