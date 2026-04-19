using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private static readonly int SpeedHash = 
        Animator.StringToHash("Speed");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Cherche l'Animator sur l'enfant !
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null || agent == null) return;
        
        float speed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
    }

    public void SetDestination(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }
}