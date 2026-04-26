using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Crowd : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent navMeshAgent;

    [Header("Waypoints")]
    [Tooltip("Assigne manuellement les waypoints ici, OU laisse vide pour utiliser le tag 'Target'")]
    public Transform[] waypoints;

    [Header("Behaviour")]
    [Tooltip("Temps d'attente min/max à chaque waypoint avant de repartir")]
    public float waitTimeMin = 0.5f;
    public float waitTimeMax = 2.5f;

    [Tooltip("Distance à laquelle on considère le PNJ arrivé")]
    public float arrivalDistance = 0.8f;

    private Animator animator;
    private Transform currentTarget;
    private bool isWaiting = false;

    // -------------------------------------------------------
    void Start()
    {
        animator = GetComponent<Animator>();
        SetAnimationMode(1); // Walk par défaut

        // Si aucun waypoint assigné manuellement, on cherche par tag
        if (waypoints == null || waypoints.Length == 0)
            RefreshWaypointsFromTag();

        if (waypoints != null && waypoints.Length > 0)
            GoToRandomWaypoint();
        else
            Debug.LogWarning($"[Crowd] {gameObject.name} : aucun waypoint trouvé !");
    }

    // -------------------------------------------------------
    void Update()
    {
        if (isWaiting || currentTarget == null) return;

        // Arrivé à destination ?
        if (!navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= arrivalDistance)
        {
            StartCoroutine(WaitThenMove());
        }

        // Synchronise l'animation avec la vitesse réelle
        bool isMoving = navMeshAgent.velocity.magnitude > 0.1f;
        SetAnimationMode(isMoving ? 1 : 0);
    }

    // -------------------------------------------------------
    /// <summary>
    /// Attend un peu puis choisit un nouveau waypoint aléatoire (différent de l'actuel).
    /// </summary>
    IEnumerator WaitThenMove()
    {
        isWaiting = true;
        SetAnimationMode(0); // Idle pendant l'attente

        float wait = Random.Range(waitTimeMin, waitTimeMax);
        yield return new WaitForSeconds(wait);

        GoToRandomWaypoint();
        isWaiting = false;
    }

    // -------------------------------------------------------
    /// <summary>
    /// Choisit un waypoint aléatoire DIFFÉRENT du waypoint courant et s'y rend.
    /// </summary>
    void GoToRandomWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            RefreshWaypointsFromTag();
            if (waypoints == null || waypoints.Length == 0) return;
        }

        Transform next = currentTarget;

        // On essaie de ne pas retourner immédiatement au même endroit
        int maxAttempts = 10;
        while (next == currentTarget && maxAttempts-- > 0)
            next = waypoints[Random.Range(0, waypoints.Length)];

        currentTarget = next;
        navMeshAgent.SetDestination(currentTarget.position);
        SetAnimationMode(1); // Walk
    }

    // -------------------------------------------------------
    /// <summary>
    /// Remplit le tableau waypoints à partir des GameObjects taggés "Target".
    /// AUCUN tag n'est modifié — tous les PNJ voient toujours tous les waypoints.
    /// </summary>
    void RefreshWaypointsFromTag()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag("Target");
        waypoints = new Transform[tagged.Length];
        for (int i = 0; i < tagged.Length; i++)
            waypoints[i] = tagged[i].transform;
    }

    // -------------------------------------------------------
    void SetAnimationMode(int mode)
    {
        if (animator != null)
            animator.SetInteger("Mode", mode);
    }
}