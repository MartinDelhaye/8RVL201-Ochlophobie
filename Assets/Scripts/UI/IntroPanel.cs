using UnityEngine;
using Unity.XR.CoreUtils;

namespace Ochlophobia.UI
{
    /// World-space intro panel that appears in front of the player at spawn.
    /// Wire the "J'ai compris" button's OnClick to OnUnderstoodPressed().
    public class IntroPanel : MonoBehaviour
    {
        [Tooltip("Distance du panel par rapport à la caméra VR")]
        [SerializeField] private float distanceFromCamera = 2f;

        [Tooltip("Hauteur relative à la caméra (négatif = plus bas)")]
        [SerializeField] private float heightOffset = -0.1f;

        private Transform _cameraTransform;

        private void Start()
        {
            var xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin != null)
                _cameraTransform = xrOrigin.Camera.transform;
            else if (Camera.main != null)
                _cameraTransform = Camera.main.transform;

            PositionInFrontOfPlayer();
        }

        private void PositionInFrontOfPlayer()
        {
            if (_cameraTransform == null) return;

            // Projette le forward de la caméra sur le plan horizontal
            Vector3 forward = _cameraTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            Vector3 position = _cameraTransform.position
                + forward * distanceFromCamera
                + Vector3.up * heightOffset;

            transform.position = position;
            transform.rotation = Quaternion.LookRotation(forward);
        }

        /// Appelée par le bouton "J'ai compris" du canvas.
        public void OnUnderstoodPressed()
        {
            GameManager.Instance?.StartGame();
            gameObject.SetActive(false);
        }
    }
}
