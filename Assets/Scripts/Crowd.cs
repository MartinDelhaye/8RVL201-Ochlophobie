using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Crowd : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent navMeshAgent;

    [Header("Errance aléatoire (recommandé)")]
    [Tooltip("Active l'errance aléatoire sur le NavMesh — ignore les waypoints")]
    public bool useRandomWander = true;
    [Tooltip("Rayon max autour de la position de départ pour choisir une destination")]
    public float wanderRadius = 12f;

    [Header("Waypoints (fallback si useRandomWander = false)")]
    [Tooltip("Assigne manuellement les waypoints ici, OU laisse vide pour utiliser le tag 'Target'")]
    public Transform[] waypoints;

    [Header("Behaviour")]
    [Tooltip("Temps d'attente min/max avant de repartir")]
    public float waitTimeMin = 0.5f;
    public float waitTimeMax = 2.5f;
    [Tooltip("Distance à laquelle on considère le PNJ arrivé")]
    public float arrivalDistance = 0.8f;

    private Animator  _animator;
    private Vector3   _origin;        // position de spawn = centre d'errance
    private Vector3   _currentDest;
    private bool      _isWaiting = false;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _origin   = transform.position;

        SetAnimationMode(1);

        if (useRandomWander)
            GoToRandomPosition();
        else
            StartWaypointMode();
    }

    void Update()
    {
        if (_isWaiting) return;

        bool arrived = !navMeshAgent.pathPending
                    && navMeshAgent.remainingDistance <= arrivalDistance;
        if (arrived)
            StartCoroutine(WaitThenMove());

        SetAnimationMode(navMeshAgent.velocity.magnitude > 0.1f ? 1 : 0);
    }

    // ── Errance aléatoire ─────────────────────────────────────────────────────

    void GoToRandomPosition()
    {
        if (TrySampleNavMesh(_origin, wanderRadius, out Vector3 dest))
        {
            _currentDest = dest;
            navMeshAgent.SetDestination(dest);
            SetAnimationMode(1);
        }
    }

    static bool TrySampleNavMesh(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 rand = Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(rand.x, 0f, rand.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    // ── Waypoints (fallback) ──────────────────────────────────────────────────

    void StartWaypointMode()
    {
        if (waypoints == null || waypoints.Length == 0)
            RefreshWaypointsFromTag();

        if (waypoints != null && waypoints.Length > 0)
            GoToRandomWaypoint();
        else
            Debug.LogWarning($"[Crowd] {gameObject.name} : aucun waypoint trouvé !");
    }

    void GoToRandomWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform next = null;
        int attempts = 10;
        do { next = waypoints[Random.Range(0, waypoints.Length)]; }
        while (next.position == _currentDest && --attempts > 0);

        _currentDest = next.position;
        navMeshAgent.SetDestination(_currentDest);
        SetAnimationMode(1);
    }

    void RefreshWaypointsFromTag()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag("Target");
        waypoints = new Transform[tagged.Length];
        for (int i = 0; i < tagged.Length; i++)
            waypoints[i] = tagged[i].transform;
    }

    // ── Attente entre deux déplacements ──────────────────────────────────────

    IEnumerator WaitThenMove()
    {
        _isWaiting = true;
        SetAnimationMode(0);

        yield return new WaitForSeconds(Random.Range(waitTimeMin, waitTimeMax));

        if (useRandomWander) GoToRandomPosition();
        else                 GoToRandomWaypoint();

        _isWaiting = false;
    }

    void SetAnimationMode(int mode)
    {
        if (_animator != null)
            _animator.SetInteger("Mode", mode);
    }
}