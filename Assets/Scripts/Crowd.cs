using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Crowd : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public GameObject Target;
    public GameObject[] AllTargets;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
            animator.SetInteger("Mode", 1);
        FindeTarget();
    }

    void Update()
    {
        // Mettre à jour l'animation selon la vitesse
        if (animator != null)
        {
            if (navMeshAgent.velocity.magnitude > 0.1f)
                animator.SetInteger("Mode", 1); // Walk
            else
                animator.SetInteger("Mode", 0); // Idle
        }

        // Chercher nouvelle cible quand arrivé
        if (Target != null)
        {
            if (Vector3.Distance(this.transform.position, Target.transform.position) <= 0.5f)
            {
                FindeTarget();
            }
        }
    }

    public void FindeTarget()
    {
        if (Target != null)
            Target.transform.tag = "Target";

        AllTargets = GameObject.FindGameObjectsWithTag("Target");
        Target = AllTargets[Random.Range(0, AllTargets.Length)];
        Target.transform.tag = "Untagged";

        navMeshAgent.destination = Target.transform.position;
    }
}