using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class TeleportVolume : MonoBehaviour
{
    [SerializeField] private bool teleportPlayer, teleportNPC;
    [SerializeField] private Transform destination;

    private void Awake()
    {
        if (!teleportNPC) return;
        NavMeshLink newLink = transform.AddComponent<NavMeshLink>();
        newLink.bidirectional = false;
        NavMesh.SamplePosition(transform.position, out NavMeshHit startHit, 100f, NavMesh.AllAreas);
        newLink.startPoint = transform.InverseTransformPoint(startHit.position);
        NavMesh.SamplePosition(destination.position, out NavMeshHit endHit, 100f, NavMesh.AllAreas);
        newLink.endPoint = transform.InverseTransformPoint(endHit.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destination == null) Debug.LogWarning("Destination not set for Teleport Volume");
        if ((other.transform.root.CompareTag("Player") && teleportPlayer) || (other.transform.root.CompareTag("NPC") && teleportNPC))
        {
            other.GetComponentInParent<Character>().Teleport(destination.position);
        }
    }
}