using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Transform holdTransform;
    public Rigidbody mainRigidbody, ragdollRootRigidbody;

    [HideInInspector] public Material spriteMaterial, flippedSpriteMaterial, unflippedSpriteMaterial;

    protected bool isFlipped, ragdoll;
    protected Animator animator;
    protected Dictionary<Transform, Vector3> bonePositions = new();
    protected List<IInteractable> interactables = new();
    protected IInteractable nextInteractable = null;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        SaveBoneZsRecursively(ragdollRootRigidbody.transform);
        unflippedSpriteMaterial = GetComponentInChildren<SpriteRenderer>().material;
        flippedSpriteMaterial = new(unflippedSpriteMaterial);
        flippedSpriteMaterial.SetInt("_SpriteFlipped", 1);
        spriteMaterial = new(unflippedSpriteMaterial);
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.material = spriteMaterial;
            spriteRenderer.sortingOrder = 0;
        }
    }

    protected void SaveBoneZsRecursively(Transform currentBone)
    {
        foreach (Transform bone in currentBone)
        {
            bonePositions.Add(bone, bone.localPosition);
            SaveBoneZsRecursively(bone);
        }
    }

    protected void Flip()
    {
        foreach (KeyValuePair<Transform, Vector3> kvp in bonePositions)
        {
            kvp.Key.localPosition = isFlipped ? Vector3.Scale(kvp.Value, new Vector3(1, 1, -1)) : kvp.Value;
        }
    }

    public void Teleport(Vector3 destination, bool freeze = false)
    {
        mainRigidbody.transform.position = destination;
        ragdollRootRigidbody.transform.position = destination;
        if (freeze)
        {
            ragdollRootRigidbody.velocity = Vector3.zero;
            mainRigidbody.velocity = Vector3.zero;
        }
    }

    public virtual void InteractNext()
    {
        nextInteractable?.Interact(this);
        nextInteractable = null;
    }

    protected virtual void Drop()
    {
        if (holdTransform.childCount > 0)
        {
            Transform heldTransform = holdTransform.GetChild(0);
            heldTransform.SetParent(null);
            foreach (Collider collider in heldTransform.GetComponentsInChildren<Collider>(true)) collider.enabled = true;
            heldTransform.GetComponent<Rigidbody>().isKinematic = false;
            foreach (SpriteRenderer spriteRenderer in heldTransform.GetComponentsInChildren<SpriteRenderer>()) spriteRenderer.material = isFlipped ? flippedSpriteMaterial : unflippedSpriteMaterial;
            heldTransform.GetComponent<Prop>().isHeld = false;
        }
    }
}