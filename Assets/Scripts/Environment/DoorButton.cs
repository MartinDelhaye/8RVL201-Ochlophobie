using System.Collections;
using UnityEngine;

namespace Ochlophobia.Environment
{
    /// Bouton physique qui ouvre une porte quand le joueur le touche avec sa main.
    /// Placer sur un petit GameObject à côté de la porte.
    /// Ce GO doit avoir un Collider avec IsTrigger = true.
    [RequireComponent(typeof(Collider))]
    public class DoorButton : MonoBehaviour
    {
        [SerializeField] public AutoDoor door;
        [SerializeField] private float cooldown      = 2f;
        [SerializeField] private Color idleColor    = Color.white;
        [SerializeField] private Color pressedColor = new Color(0.1f, 1f, 0.3f);

        private Renderer _renderer;
        private bool     _onCooldown = false;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            _renderer = EnsureVisual();
            if (_renderer != null)
                _renderer.material.color = idleColor;
        }

        private Renderer EnsureVisual()
        {
            var existing = GetComponent<Renderer>();
            if (existing != null) return existing;

            var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.name = "Visual";
            child.transform.SetParent(transform, false);
            child.transform.localScale = new Vector3(0.12f, 0.12f, 0.06f);
            Destroy(child.GetComponent<Collider>());
            return child.GetComponent<Renderer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_onCooldown) return;
            if (!IsHand(other)) return;

            door?.OpenDoor();
            StartCoroutine(PressedFeedback());
        }

        private static bool IsHand(Collider other)
        {
            return other.GetComponentInParent<
                UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>() != null;
        }

        private IEnumerator PressedFeedback()
        {
            _onCooldown = true;
            if (_renderer != null) _renderer.material.color = pressedColor;
            transform.localScale *= 0.85f;

            yield return new WaitForSeconds(0.15f);
            transform.localScale /= 0.85f;

            yield return new WaitForSeconds(cooldown - 0.15f);
            if (_renderer != null) _renderer.material.color = idleColor;
            _onCooldown = false;
        }
    }
}
