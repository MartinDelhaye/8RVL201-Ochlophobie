using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CrowdSpawner : MonoBehaviour
{
    [Header("Prefabs PNJ")]
    [Tooltip("Glisser les prefabs qui ont déjà Crowd + NavMeshAgent + Animator dessus")]
    public GameObject[] npcPrefabs;

    [Header("Pool")]
    public int   poolSize  = 15;
    public int   maxActive = 10;
    [Tooltip("Scale uniforme appliquée à chaque PNJ")]
    public float npcScale  = 0.7f;

    [Header("Points de spawn")]
    public Transform[] spawnPoints;

    [Header("Waypoints partagés (Transit)")]
    public Transform[] waypoints;

    [Header("Destination métro (TowardDestination)")]
    public Transform metroDestination;

    [Header("Timing")]
    public float spawnInterval = 2f;

    [Range(0f, 1f)]
    public float metroRatio = 0.3f;

    private List<GameObject>  pool      = new List<GameObject>();
    private Queue<GameObject> available = new Queue<GameObject>();
    private int activeCount = 0;

    void Start()
    {
        BuildPool();
        StartCoroutine(SpawnLoop());
    }

    void BuildPool()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogError("[CrowdSpawner] Aucun prefab NPC assigné !");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];

            prefab.SetActive(false);
            GameObject npc = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            prefab.SetActive(true);

            npc.name = $"NPC_{i:00}";
            npc.transform.localScale = Vector3.one * npcScale;
            npc.SetActive(false);

            // Crowd doit être sur le prefab — on ne l'ajoute pas ici
            Crowd crowd = npc.GetComponent<Crowd>();
            if (crowd == null)
            {
                Debug.LogError($"[CrowdSpawner] Le prefab '{prefab.name}' n'a pas de composant Crowd ! Ajoute-le sur le prefab.");
                continue;
            }

            // NavMeshAgent doit être sur le prefab — on ne l'ajoute pas ici
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError($"[CrowdSpawner] Le prefab '{prefab.name}' n'a pas de NavMeshAgent ! Ajoute-le sur le prefab.");
                continue;
            }

            crowd.navMeshAgent = agent;
            crowd.spawner      = this;

            pool.Add(npc);
            available.Enqueue(npc);
        }

        Debug.Log($"[CrowdSpawner] Pool prêt : {pool.Count} PNJ.");
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (activeCount < maxActive && available.Count > 0)
                SpawnOne();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOne()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[CrowdSpawner] Aucun SpawnPoint !");
            return;
        }

        Transform spawnPt = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (!NavMesh.SamplePosition(spawnPt.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"[CrowdSpawner] '{spawnPt.name}' hors NavMesh.");
            return;
        }

        GameObject npc = available.Dequeue();
        npc.transform.SetPositionAndRotation(hit.position, spawnPt.rotation);

        Crowd crowd      = npc.GetComponent<Crowd>();
        bool towardMetro = metroDestination != null && Random.value < metroRatio;

        crowd.mode        = towardMetro ? Crowd.CrowdMode.TowardDestination : Crowd.CrowdMode.Transit;
        crowd.waypoints   = waypoints;
        crowd.destination = metroDestination;

        npc.SetActive(true);
        crowd.Init();

        activeCount++;
    }

    public void ReturnToPool(GameObject npc)
    {
        npc.SetActive(false);
        available.Enqueue(npc);
        activeCount = Mathf.Max(0, activeCount - 1);
    }

    public void SetMetroRatio(float ratio)   => metroRatio    = Mathf.Clamp01(ratio);
    public void SetSpawnInterval(float secs) => spawnInterval = Mathf.Max(0.1f, secs);
    public void SetMaxActive(int max)        => maxActive     = Mathf.Clamp(max, 0, poolSize);
}