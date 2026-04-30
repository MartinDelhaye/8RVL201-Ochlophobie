using UnityEngine;
using Unity.XR.CoreUtils;

namespace Ochlophobia.Crowd
{
    /// À attacher sur chaque agent de la foule.
    /// Applique une force sur le Rigidbody du joueur quand l'agent est proche,
    /// simulant la pression physique de la foule.
    public class CrowdPush : MonoBehaviour
    {
        [SerializeField] private float pushRadius = 0.7f;
        [SerializeField] private float pushForce  = 3f;

        // Référence partagée entre tous les agents — initialisée une seule fois
        private static Rigidbody _playerRb;
        private static Transform _playerTransform;

        private void Start()
        {
            if (_playerRb == null)
            {
                var origin = FindObjectOfType<XROrigin>();
                if (origin != null)
                {
                    _playerTransform = origin.transform;
                    _playerRb        = origin.GetComponent<Rigidbody>();
                }
            }
        }

        private void FixedUpdate()
        {
            if (_playerRb == null) return;

            Vector3 toPlayer = _playerTransform.position - transform.position;
            toPlayer.y = 0f;

            float dist = toPlayer.magnitude;
            if (dist < 0.01f || dist > pushRadius) return;

            // Force proportionnelle à la proximité et à la vitesse de l'agent
            float intensity = (1f - dist / pushRadius) * pushForce;
            _playerRb.AddForce(toPlayer.normalized * intensity, ForceMode.Force);
        }

        private void OnDestroy()
        {
            // Ne remet à null que si c'est bien cet objet qui avait initialisé la ref
            if (_playerTransform != null && _playerRb == null)
            {
                _playerRb        = null;
                _playerTransform = null;
            }
        }
    }
}
