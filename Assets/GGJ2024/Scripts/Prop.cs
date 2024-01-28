using System.Collections;
using UnityEngine;

public abstract class Prop : MonoBehaviour, IInteractable
{
    public string prompt = "Throw";

    [HideInInspector] public bool isHeld;

    [SerializeField] Collider[] persistentColliders;
    [SerializeField] protected Vector3 throwForce = new(5, 5, 0);
    [SerializeField] protected float throwOffset = 0.1f, throwIsHeldDelay = 2;

    protected Player player;

    public virtual void Interact(MonoBehaviour caller)
    {
        switch (caller)
        {
            case Character character:
                if (character.holdTransform != null)
                {
                    GetComponent<Rigidbody>().isKinematic = true;
                    transform.SetParent(character.holdTransform);
                    transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                    {
                        renderer.material = character.spriteMaterial;
                    }
                    isHeld = true;
                    if (character is Player) player = character as Player;
                }
                break;
            default:
                break;
        }
    }

    public virtual void Activate()
    {
        player.thrownProp = this;
        player.animator.SetTrigger("Throw");
    }

    public void Throw(Transform playerTransform)
    {
        transform.SetParent(null, true);
        transform.SetPositionAndRotation(transform.position + playerTransform.right * throwOffset, Quaternion.identity);
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = false;
        Vector3 throwForceCurrent = playerTransform.TransformDirection(throwForce);
        rigidbody.AddForce(throwForceCurrent, ForceMode.Impulse);
        transform.GetComponent<SpriteRenderer>().material = player.unflippedSpriteMaterial;
        StartCoroutine(DelayedHeld());
    }

    private IEnumerator DelayedHeld()
    {
        yield return new WaitForSeconds(throwIsHeldDelay);
        isHeld = false;
    }
}
