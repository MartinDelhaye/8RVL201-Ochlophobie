using UnityEngine;

namespace Ochlophobia.Environment
{
    /// Zone de déclenchement à placer sur le quai (invisible, IsTrigger = true).
    /// Quand le joueur entre dedans, prévient le GameManager qu'il a rejoint le train.
    [RequireComponent(typeof(Collider))]
    public class TrainTrigger : MonoBehaviour
    {
        [Tooltip("Tag du joueur (XR Origin)")]
        [SerializeField] private string playerTag = "Player";

        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            GameManager.Instance?.PlayerReachedTrain();
        }
    }
}
