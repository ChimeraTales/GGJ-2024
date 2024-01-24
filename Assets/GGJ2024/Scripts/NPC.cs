using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private float ragdollRelativeVelocity = 2, forceRagdollDurationMultiplier = 0.25f, forceRagdollMinDuration = 1f;
    [SerializeField] Rigidbody mainRigidbody, ragdollRootRigidbody;

    private bool ragdoll;
    Animator animator;

    public bool Ragdoll
    {
        get { return ragdoll; }
        set
        {
            ragdoll = value;
            SetRagdoll(ragdoll);
        }
    }
    void Awake()
    {
        animator = GetComponent<Animator>();
        /*spriteRenderer = GetComponent<SpriteRenderer>();
        unflippedSpriteMaterial = GetComponentInChildren<SpriteRenderer>().material;
        flippedSpriteMaterial = new(unflippedSpriteMaterial);
        flippedSpriteMaterial.SetInt("_SpriteFlipped", 1);
        spriteMaterial = new(unflippedSpriteMaterial);
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>()) spriteRenderer.material = spriteMaterial;*/
    }

    private void SetRagdoll(bool enabled)
    {
        foreach (HingeJoint hinge in transform.GetComponentsInChildren<HingeJoint>(true))
        {
            //hinge.enabled = enabled;
        }
        foreach (Rigidbody rigidbody in transform.GetComponentsInChildren<Rigidbody>())
        {
            if (mainRigidbody == rigidbody)
            {
                if (!enabled) rigidbody.velocity = ragdollRootRigidbody.velocity;
                continue;
            }
            rigidbody.velocity = enabled ? mainRigidbody.velocity : Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        mainRigidbody.transform.GetComponent<CapsuleCollider>().enabled = !enabled;
        animator.enabled = !enabled;
    }

    private IEnumerator ForceRagdoll(float duration)
    {
        Ragdoll = true;
        yield return new WaitForSeconds(duration);
        Ragdoll = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > ragdollRelativeVelocity)
        {
            StartCoroutine(ForceRagdoll(Mathf.Max((collision.relativeVelocity.magnitude - ragdollRelativeVelocity) * forceRagdollDurationMultiplier, forceRagdollMinDuration)));
        }
    }
}
