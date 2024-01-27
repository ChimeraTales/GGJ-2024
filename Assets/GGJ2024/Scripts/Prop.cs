using System.Linq;
using UnityEngine;

public abstract class Prop : MonoBehaviour, IInteractable
{
    public string prompt = "Use";

    [HideInInspector] public bool isHeld;

    [SerializeField] Collider[] persistentColliders;

    public virtual void Interact(MonoBehaviour caller)
    {
        switch (caller)
        {
            case Character player:
                if (player.holdTransform != null)
                {
                    GetComponent<Rigidbody>().isKinematic = true;
                    transform.SetParent(player.holdTransform);
                    transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                    {
                        renderer.material = player.spriteMaterial;
                    }
                    isHeld = true;
                }
                break;
            default:
                break;
        }
    }

    public virtual void Activate()
    {

    }
}
