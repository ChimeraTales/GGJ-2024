using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class Character : MonoBehaviour
{
    public Transform holdTransform;
    public Rigidbody mainRigidbody, ragdollRootRigidbody;

    [SerializeField] SpriteRenderer midpointRenderer;
    [SerializeField] float layerSeparation = 0.005f;

    [HideInInspector] public Material spriteMaterial, flippedSpriteMaterial, unflippedSpriteMaterial;
    [HideInInspector] public Animator animator;

    protected bool isFlipped, ragdoll;
    protected Dictionary<Transform, Vector3> bonePositions = new();
    protected List<IInteractable> interactables = new();
    protected IInteractable nextInteractable = null;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        unflippedSpriteMaterial = GetComponentInChildren<SpriteRenderer>().material;
        flippedSpriteMaterial = new(unflippedSpriteMaterial);
        flippedSpriteMaterial.SetInt("_SpriteFlipped", 1);
        spriteMaterial = new(unflippedSpriteMaterial);
        int midpointRendererOrder = midpointRenderer.sortingOrder;
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            float newZ = midpointRenderer.transform.position.z - (renderer.sortingOrder - midpointRendererOrder) * layerSeparation;
            foreach (Transform bone in renderer.GetComponent<SpriteSkin>().boneTransforms)
            {
                bone.position = new Vector3(bone.position.x, bone.position.y, newZ);
            }
            renderer.material = spriteMaterial;
            renderer.sortingOrder = 0;
        }
        SaveBoneZsRecursively(ragdollRootRigidbody.transform);
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
        if (ragdoll) destination += Vector3.up * 0.5f;
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