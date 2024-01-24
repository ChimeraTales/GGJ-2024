using UnityEngine;

public abstract class Prop : MonoBehaviour, IInteractable
{
    public virtual void Interact(MonoBehaviour caller)
    {
        switch (caller)
        {
            case Player player:
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
