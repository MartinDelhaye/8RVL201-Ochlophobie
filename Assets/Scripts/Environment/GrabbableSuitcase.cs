using UnityEngine;

namespace Ochlophobia.Environment
{
    /// Ajoute automatiquement Rigidbody, Collider et XRGrabInteractable à la valise.
    /// Attacher ce script sur le GameObject racine du prefab Suitcase.
    public class GrabbableSuitcase : MonoBehaviour
    {
        [SerializeField] private Vector3 colliderSize   = new Vector3(0.52f, 0.32f, 0.22f);
        [SerializeField] private Vector3 colliderCenter = new Vector3(0f, 0.16f, 0f);
        [SerializeField] private float   mass           = 2f;

        private void Awake()
        {
            SetupRigidbody();
            SetupCollider();
            SetupGrab();
        }

        private void SetupRigidbody()
        {
            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.mass                   = mass;
            rb.useGravity             = true;
            rb.isKinematic            = false;
            rb.interpolation          = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private void SetupCollider()
        {
            if (GetComponent<Collider>() != null) return;
            var box    = gameObject.AddComponent<BoxCollider>();
            box.size   = colliderSize;
            box.center = colliderCenter;
        }

        private void SetupGrab()
        {
            if (GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() != null) return;

            var grab = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grab.movementType     = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
            grab.throwOnDetach    = true;
            grab.useDynamicAttach = true;
        }
    }
}
