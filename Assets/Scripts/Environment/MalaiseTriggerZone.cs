using UnityEngine;
using Unity.XR.CoreUtils;
using Ochlophobia.Player;

namespace Ochlophobia.Environment
{
    /// Zone de déclenchement du malaise.
    /// Mode PreSigns  → démarre les clignements précurseurs (à l'entrée de la salle).
    /// Mode Collapse  → déclenche l'effondrement final (près du quai).
    /// Ajouter un BoxCollider avec IsTrigger = true sur ce GameObject.
    [RequireComponent(typeof(Collider))]
    public class MalaiseTriggerZone : MonoBehaviour
    {
        public enum TriggerMode { PreSigns, Collapse }

        [SerializeField] private TriggerMode mode = TriggerMode.Collapse;

        private MalaiseEffect _effect;
        private bool _triggered = false;

        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
            _effect = FindObjectOfType<MalaiseEffect>();

            if (_effect == null)
                Debug.LogWarning($"[MalaiseTriggerZone] Aucun MalaiseEffect trouvé dans la scène.");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || _effect == null) return;
            if (other.GetComponentInParent<XROrigin>() == null) return;

            _triggered = true;

            if (mode == TriggerMode.PreSigns)
                _effect.StartPreSigns();
            else
                _effect.TriggerCollapse();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = mode == TriggerMode.PreSigns
                ? new Color(1f, 0.6f, 0f, 0.3f)   // orange = pré-signes
                : new Color(1f, 0f, 0f, 0.3f);      // rouge  = effondrement
            var col = GetComponent<Collider>();
            if (col is BoxCollider box)
                Gizmos.DrawCube(transform.position + box.center, box.size);
        }
#endif
    }
}
