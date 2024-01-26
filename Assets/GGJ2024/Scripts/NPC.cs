using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    [SerializeField] private Waypoint[] waypoints;
    [SerializeField] private float ragdollRelativeVelocity = 2, forceRagdollDurationMultiplier = 0.25f, forceRagdollMinDuration = 1f;
    [SerializeField] Rigidbody mainRigidbody, ragdollRootRigidbody;
    [SerializeField] NavMeshAgent agent;

    [HideInInspector] public Material spriteMaterial, flippedSpriteMaterial, unflippedSpriteMaterial;

    private bool ragdoll, isFlipped;
    Animator animator;
    Dictionary<Transform, Vector3> bonePositions = new();
    float lastX = 0;

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
        unflippedSpriteMaterial = GetComponentInChildren<SpriteRenderer>().material;
        flippedSpriteMaterial = new(unflippedSpriteMaterial);
        flippedSpriteMaterial.SetInt("_SpriteFlipped", 1);
        spriteMaterial = new(unflippedSpriteMaterial);
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>()) spriteRenderer.material = spriteMaterial;
        agent = GetComponent<NavMeshAgent>();
        SaveBoneZsRecursively(ragdollRootRigidbody.transform);
        for (int i = 0; i < waypoints.Length; i++) waypoints[i].location = waypoints[i].transform.position;
        lastX = transform.position.x;
        SendToWaypoint(transform.position);
    }

    private void SaveBoneZsRecursively(Transform currentBone)
    {
        foreach (Transform bone in currentBone)
        {
            bonePositions.Add(bone, bone.localPosition);
            SaveBoneZsRecursively(bone);
        }
    }

    private void Update()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (!waypoints[i].triggered && waypoints[i].time < GameManager.CurrentTime)
            {
                SendToWaypoint(waypoints[i].location);
                waypoints[i].triggered = true;
            }
        }
    }

    private void LateUpdate()
    {
        float currentX = transform.position.x;
        if (!Ragdoll)
        {
            bool currentFlipped = lastX > currentX;
            if (currentFlipped != isFlipped)
            {
                if (lastX != currentX) isFlipped = currentFlipped;
                Flip();
            }
            transform.rotation = Quaternion.Euler(0, isFlipped ? 180 : 0, 0);
            spriteMaterial.SetInt("_SpriteFlipped", isFlipped ? 1 : 0);
        }
        lastX = transform.position.x;
        animator.SetBool("Moving", agent.pathPending || agent.remainingDistance > agent.stoppingDistance || agent.hasPath || agent.velocity.sqrMagnitude != 0f);
    }

    private void Flip()
    {
        foreach (KeyValuePair<Transform, Vector3> kvp in bonePositions)
        {
            kvp.Key.localPosition = isFlipped ? Vector3.Scale(kvp.Value, new Vector3(1, 1, -1)) : kvp.Value;
        }
    }

    public void SendToWaypoint(Vector3 destination)
    {
        agent.SetDestination(destination);
    }

    private void SetRagdoll(bool enabled)
    {
        foreach (Rigidbody rigidbody in transform.GetComponentsInChildren<Rigidbody>())
        {
            if (mainRigidbody == rigidbody) continue;
            rigidbody.isKinematic = !enabled;
            if (!enabled) continue;
            rigidbody.velocity = enabled ? mainRigidbody.velocity : Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        if (!enabled) mainRigidbody.transform.position = ragdollRootRigidbody.transform.position;
        animator.enabled = !enabled;
    }

    private IEnumerator ForceRagdoll(float duration, Vector3 colliderVelocity)
    {
        Ragdoll = true;
        foreach (Rigidbody rigidbody in ragdollRootRigidbody.GetComponentsInChildren<Rigidbody>()) rigidbody.velocity = colliderVelocity;
        yield return new WaitForSeconds(duration);
        Ragdoll = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > ragdollRelativeVelocity && collision.gameObject.layer.ToString() != "Ground")
        {
            StartCoroutine(ForceRagdoll(Mathf.Max((collision.relativeVelocity.magnitude - ragdollRelativeVelocity) * forceRagdollDurationMultiplier, forceRagdollMinDuration), collision.relativeVelocity));
        }
    }

    [System.Serializable]
    private struct Waypoint
    {
        public Transform transform;
        public float time;
        [HideInInspector] public Vector3 location;
        [HideInInspector] public bool triggered;
    }
}