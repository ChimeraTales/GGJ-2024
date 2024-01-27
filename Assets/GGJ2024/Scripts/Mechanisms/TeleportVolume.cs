using UnityEngine;

public class TeleportVolume : MonoBehaviour
{
    [SerializeField] private bool teleportPlayer, teleportNPC;
    [SerializeField] private Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        if (destination == null) Debug.LogWarning("Destination not set for Teleport Volume");
        if ((other.transform.root.CompareTag("Player") && teleportPlayer) || (other.transform.root.CompareTag("NPC") && teleportNPC))
        {
            other.GetComponentInParent<Character>().Teleport(destination.position);
        }
    }
}