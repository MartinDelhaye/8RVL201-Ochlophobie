using UnityEngine;

/// <summary>
/// Place ce script sur un GameObject vide avec un SphereCollider (Is Trigger = true).
/// Quand le XR Origin entre dans la zone, maxActive du CrowdSpawner monte
/// progressivement selon la proximité au centre.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class CrowdDensityZone : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Le CrowdSpawner à piloter")]
    public CrowdSpawner crowdSpawner;

    [Tooltip("Le Transform du XR Origin (ou Camera Offset)")]
    public Transform xrOrigin;

    [Header("Densité")]
    [Tooltip("maxActive minimum quand on vient d'entrer dans la zone")]
    public int minActive = 20;

    [Tooltip("maxActive maximum quand on est au centre de la zone")]
    public int maxActive = 100;

    // État interne
    private SphereCollider _zone;
    private bool           _playerInside = false;
    private int            _originalMax;

    void Awake()
    {
        _zone = GetComponent<SphereCollider>();
        _zone.isTrigger = true;
    }

    void Start()
    {
        if (crowdSpawner == null)
            Debug.LogError("[CrowdDensityZone] CrowdSpawner non assigné !");
        if (xrOrigin == null)
            Debug.LogError("[CrowdDensityZone] XR Origin non assigné !");

        if (crowdSpawner != null)
            _originalMax = crowdSpawner.maxActive;
    }

    void Update()
    {
        if (!_playerInside || crowdSpawner == null || xrOrigin == null) return;

        // Distance entre le joueur et le centre de la zone
        float dist   = Vector3.Distance(xrOrigin.position, transform.position);
        float radius = _zone.radius * Mathf.Max(transform.lossyScale.x,
                                                transform.lossyScale.y,
                                                transform.lossyScale.z);

        // t = 0 au bord, t = 1 au centre
        float t = Mathf.Clamp01(1f - (dist / radius));

        // Interpolation linéaire de minActive → maxActive
        int target = Mathf.RoundToInt(Mathf.Lerp(minActive, maxActive, t));

        // On ne fait que monter (pas redescendre pour l'instant)
        if (target > crowdSpawner.maxActive)
            crowdSpawner.SetMaxActive(target);
    }

    void OnTriggerEnter(Collider other)
    {
        if (xrOrigin != null && other.transform == xrOrigin)
        {
            _playerInside = true;
            Debug.Log("[CrowdDensityZone] Joueur entré dans la zone.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (xrOrigin != null && other.transform == xrOrigin)
        {
            _playerInside = false;
            Debug.Log("[CrowdDensityZone] Joueur sorti de la zone.");
        }
    }

#if UNITY_EDITOR
    // Visualisation de la zone dans la scène
    void OnDrawGizmos()
    {
        if (_zone == null) _zone = GetComponent<SphereCollider>();
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, _zone.radius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, _zone.radius);
    }
#endif
}