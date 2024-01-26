using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Progress;

public class NPC : Character
{
    public bool canStartle;
    [SerializeField] private Waypoint[] waypoints;
    [SerializeField] private float ragdollRelativeVelocity = 2, forceRagdollDurationMultiplier = 0.25f, forceRagdollMinDuration = 1f, grabDistance;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform[] desiredObjects;

    float lastX = 0;
    Vector3 currentTarget;
    Transform desiredObject;

    public bool Ragdoll
    {
        get { return ragdoll; }
        set
        {
            ragdoll = value;
            SetRagdoll(ragdoll);
            agent.isStopped = value;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        for (int i = 0; i < waypoints.Length; i++) waypoints[i].location = waypoints[i].transform.position;
        lastX = transform.position.x;
        SendToWaypoint(transform.position);
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

    public void SendToWaypoint(Vector3 destination)
    {
        agent.SetDestination(destination);
        currentTarget = destination;
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

    public IEnumerator ForceRagdoll(float duration, Vector3 colliderVelocity)
    {
        Ragdoll = true;
        Drop();
        desiredObject = null;
        foreach (Rigidbody rigidbody in ragdollRootRigidbody.GetComponentsInChildren<Rigidbody>()) rigidbody.velocity = colliderVelocity;
        yield return new WaitForSeconds(duration);
        Ragdoll = false;
        GrabMostDesiredItem();
    }

    public void Grab(Transform target)
    {
        desiredObject = target;
        StartCoroutine(GrabCoroutine());
    }

    private IEnumerator GrabCoroutine()
    {
        while (desiredObject != null)
        {
            if (Vector3.Distance(transform.position, desiredObject.position) < grabDistance && !Ragdoll)
            {
                agent.SetDestination(transform.position);
                nextInteractable = desiredObject.GetComponent<IInteractable>();
                animator.SetTrigger("Grab");
                desiredObject = null;
            }
            else if (agent.isActiveAndEnabled) agent.SetDestination(desiredObject.position);
            yield return null;
        }
        agent.SetDestination(currentTarget);
    }

    private void GrabMostDesiredItem()
    {
        int lowestIndex = int.MaxValue;
        Transform desiredTransform = null;
        foreach (Transform item in interactables.Select(interactable => interactable.transform))
        {
            int itemIndex = Array.IndexOf(desiredObjects, item);
            if ((!item.GetComponent<Prop>().isHeld ||  (holdTransform.childCount > 0 && holdTransform.GetChild(0) == item)) && itemIndex < lowestIndex) { desiredTransform = item; lowestIndex = itemIndex; }
        }
        if (desiredTransform != null && (holdTransform.childCount == 0 || Array.IndexOf(desiredObjects, holdTransform.GetChild(0)) > lowestIndex)) { Drop(); Grab(desiredTransform); }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > ragdollRelativeVelocity && collision.gameObject.layer.ToString() != "Ground")
        {
            StartCoroutine(ForceRagdoll(Mathf.Max((collision.relativeVelocity.magnitude - ragdollRelativeVelocity) * forceRagdollDurationMultiplier, forceRagdollMinDuration), collision.relativeVelocity));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Prop newProp) && desiredObjects.Contains(other.transform))
        {
            if (!interactables.Contains(newProp))
            {
                interactables.Add(newProp);
                GrabMostDesiredItem();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == desiredObject) desiredObject = null;
        if (other.TryGetComponent(out Prop newProp))
        {
            if (interactables.Contains(newProp)) { interactables.Remove(newProp); interactables.TrimExcess(); }
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