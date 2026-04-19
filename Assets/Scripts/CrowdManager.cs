using UnityEngine;
using UnityEngine.AI;

public class CrowdManager : MonoBehaviour
{
    public GameObject[] agentPrefabs;
    public int numberOfAgents = 20;
    public float spawnRadius = 10f;
    public Transform player;

    void Start()
    {
        SpawnCrowd();
    }

    void SpawnCrowd()
    {
        for (int i = 0; i < numberOfAgents; i++)
        {
            // Position aléatoire autour du joueur
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos.y = 0;
            randomPos += transform.position;

            // Trouver le point le plus proche sur le NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
            {
                GameObject prefab = agentPrefabs[Random.Range(0, agentPrefabs.Length)];
                GameObject agent = Instantiate(prefab, hit.position, Quaternion.identity);
                
                AgentController controller = agent.GetComponent<AgentController>();
                if (controller != null && player != null)
                {
                    controller.SetDestination(player.position);
                }
            }
        }
    }

    void Update()
    {
        // Mettre à jour la destination de tous les agents
        if (player == null) return;
        
        AgentController[] agents = FindObjectsByType<AgentController>(FindObjectsSortMode.None);
        foreach (var agent in agents)
        {
            agent.SetDestination(player.position);
        }
    }
}