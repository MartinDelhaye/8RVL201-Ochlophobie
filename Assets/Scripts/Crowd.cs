using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Crowd : MonoBehaviour
{
    public enum CrowdMode { Transit, TowardDestination }

    [Header("Mode")]
    public CrowdMode mode = CrowdMode.Transit;

    [Header("Navigation")]
    public NavMeshAgent navMeshAgent;

    [Header("Waypoints (Transit)")]
    public Transform[] waypoints;

    [Header("Destination (TowardDestination)")]
    public Transform destination;

    [Header("Comportement")]
    public float waitTimeMin     = 0.5f;
    public float waitTimeMax     = 2.0f;
    public float arrivalDistance = 0.8f;
    public float moveSpeed       = 1.4f;

    [HideInInspector] public CrowdSpawner spawner;

    private Animator  animator;
    private Transform currentTarget;
    private bool      isWaiting  = false;
    private bool      hasArrived = false;
    private bool      isReady    = false;

    public void Init()
    {
        isWaiting  = false;
        hasArrived = false;
        isReady    = true;

        // Cherche l'Animator sur l'enfant qui a le vrai squelette/mesh
        // On cherche dans les enfants en IGNORANT le root lui-même
        animator = null;
        foreach (Animator a in GetComponentsInChildren<Animator>(true))
        {
            // Prend le premier Animator qui a un controller assigné
            if (a.runtimeAnimatorController != null)
            {
                animator = a;
                break;
            }
        }

        if (animator == null)
            Debug.LogWarning($"[Crowd] {name} : aucun Animator avec controller trouvé !");
        else
            Debug.Log($"[Crowd] {name} : Animator trouvé sur '{animator.gameObject.name}' controller='{animator.runtimeAnimatorController.name}'");

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled   = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.speed     = moveSpeed;
        }

        SetMode(1);
        StartNavigation();
    }

    void OnEnable()  { }
    void OnDisable()
    {
        isReady = false;
        StopAllCoroutines();
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;
    }

    void Update()
    {
        if (!isReady || isWaiting || hasArrived) return;
        if (navMeshAgent == null || !navMeshAgent.enabled) return;

        SetMode(navMeshAgent.velocity.magnitude > 0.1f ? 1 : 0);

        if (navMeshAgent.pathPending) return;
        if (navMeshAgent.remainingDistance <= arrivalDistance)
            OnReachedTarget();
    }

    void StartNavigation()
    {
        if (mode == CrowdMode.Transit)
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError($"[Crowd] {name} : waypoints vides !");
                return;
            }
            GoToRandomWaypoint();
        }
        else
        {
            if (destination == null)
            {
                Debug.LogError($"[Crowd] {name} : destination nulle !");
                return;
            }
            navMeshAgent.SetDestination(destination.position);
        }
    }

    void OnReachedTarget()
    {
        if (mode == CrowdMode.Transit)
            StartCoroutine(WaitThenMove());
        else
        {
            hasArrived = true;
            SetMode(0);
            StartCoroutine(DespawnAfterDelay(0.3f));
        }
    }

    IEnumerator WaitThenMove()
    {
        isWaiting = true;
        SetMode(0);
        yield return new WaitForSeconds(Random.Range(waitTimeMin, waitTimeMax));
        GoToRandomWaypoint();
        isWaiting = false;
    }

    void GoToRandomWaypoint()
    {
        Transform next = currentTarget;
        int attempts = 10;
        while (next == currentTarget && attempts-- > 0)
            next = waypoints[Random.Range(0, waypoints.Length)];

        currentTarget = next;
        navMeshAgent.SetDestination(currentTarget.position);
        SetMode(1);
    }

    IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (spawner != null) spawner.ReturnToPool(gameObject);
        else gameObject.SetActive(false);
    }

    void SetMode(int m)
    {
        if (animator != null)
            animator.SetInteger("Mode", m);
    }
}