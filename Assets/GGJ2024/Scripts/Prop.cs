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
                    foreach (Collider collider in GetComponentsInChildren<Collider>()) if (!collider.isTrigger) collider.enabled = false;
                    transform.SetParent(player.holdTransform);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                    {
                        renderer.material = player.spriteMaterial;
                    }
                    isHeld = true;
                    foreach (Collider collider in GetComponentsInChildren<Collider>().Where(collider => collider.isTrigger && !persistentColliders.Contains(collider)))
                    {
                        collider.enabled = false;
                    }
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
