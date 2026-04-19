using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Crowd : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public GameObject Target;
    public GameObject[] AllTargets;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().SetInteger("Mode", 1);
        FindeTarget();
    }

    // Update is called once per frame
    void Update()
    {
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