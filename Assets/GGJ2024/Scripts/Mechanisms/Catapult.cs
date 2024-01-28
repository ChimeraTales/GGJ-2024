using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Catapult : Mechanism
{
    [SerializeField] Transform loadTransform, targetTransform, destinationTransform;
    [SerializeField] float launchForce, teleportDelay = 1;

    private Animator animator;
    private Player player;
    private bool shooting, playerShot;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact(MonoBehaviour caller)
    {
        switch (caller)
        {
            case Player player:
                if (player.holdTransform.childCount > 0)
                {
                    Load(player.holdTransform.GetChild(0));
                }
                break;
            default: break;
        }
    }

    public override void Activate()
    {
        shooting = true;
        if (player != null) player.RagdollLocked = true;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot")) animator.SetTrigger("Shoot");
    }

    public override bool CanInteract()
    {
        return loadTransform.childCount == 0;
    }

    private void Load(Transform loadedTransform)
    {
        Vector3 scale = loadedTransform.lossyScale;
        loadedTransform.GetComponent<Prop>().isHeld = false;
        loadedTransform.SetParent(loadTransform, false);
        loadedTransform.localPosition = new Vector3(0, loadedTransform.GetComponentsInChildren<Collider>(true).First(collider => !collider.isTrigger).bounds.extents.y, 0);
        loadedTransform.localScale = transform.InverseTransformVector(scale);
    }

    private void Shoot()
    {
        List<Rigidbody> rigidbodies = new();
        if (loadTransform.childCount > 0)
        {
            Transform projectile = loadTransform.GetChild(0);
            Vector3 scale = projectile.lossyScale;
            projectile.SetParent(null);
            projectile.localScale = transform.TransformVector(scale);
            Rigidbody rigidbody = projectile.GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.AddForce((targetTransform.position - loadTransform.position) * launchForce, ForceMode.Impulse);
            rigidbodies.Add(rigidbody);
        }
        if (player != null)
        {
            foreach (Rigidbody rigidbody in player.ragdollRootRigidbody.transform.GetComponentsInChildren<Rigidbody>().Where(ragdoll => ragdoll.transform.parent != player.holdTransform))
            {
                rigidbody.AddForce((targetTransform.position - loadTransform.position) * launchForce, ForceMode.Impulse);
            }
            playerShot = true;
        }
        else { shooting = false; playerShot = false; }
        StartCoroutine(Teleport(rigidbodies.ToArray()));
    }

    private IEnumerator Teleport(Rigidbody[] rigidbodies)
    {
        yield return new WaitForSeconds(teleportDelay);
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.transform.localRotation = Quaternion.identity;
                rigidbody.position = destinationTransform.position;
                foreach (Collider collider in rigidbody.GetComponentsInChildren<Collider>()) collider.enabled = true;
            }
        }
        if (playerShot && player != null)
        {
            player.Teleport(destinationTransform.position, true);
            player.RagdollLocked = false;
            shooting = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            player = collision.gameObject.GetComponent<Player>();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(UnassignPlayer());
        }
    }

    private IEnumerator UnassignPlayer()
    {
        while (shooting) yield return null;
        player = null;
    }
}
