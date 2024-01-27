using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    public PlayerInput input;

    [SerializeField] float speed = 1, maxSpeed = 5, jumpForce, airControlMultiplier = 0.2f, maxSlope = Mathf.PI / 2f, ragdollBoost = 10f;
    [SerializeField] LayerMask groundedLayers;
    [SerializeField] readonly LayerMask cameraZoneLayers;
    [SerializeField] Collider baseCollider;
    [SerializeField] bool jauntyRotate;
    [SerializeField] SpriteRenderer midpointRenderer;

    float lastX = 0, baseDynamicFriction;
    bool hasRagdolled, isGrounded, ragdollLocked;
    Vector3 groundNormal, steepGroundNormal;
    PhysicMaterialCombine baseCombine;
    Vector2 walk;

    public bool Ragdoll
    {
        get { return ragdoll; }
        set
        {
            ragdoll = value;
            if (!ragdollLocked) SetRagdoll(ragdoll);
        }
    }

    public bool RagdollLocked
    {
        get { return ragdollLocked; }
        set
        {
            ragdollLocked = value;
            if (!value) SetRagdoll(ragdoll);
        }
    }

    public Transform FocusPoint
    {
        get
        {
            return Ragdoll ? ragdollRootRigidbody.transform : transform;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        baseCollider = mainRigidbody.GetComponent<Collider>();
        baseDynamicFriction = baseCollider.material.dynamicFriction;
        baseCombine = baseCollider.material.frictionCombine;
    }

    private void OnWalk(InputValue value)
    {
        walk = value.Get<Vector2>();
    }

    private void OnRagdoll(InputValue value)
    {
        Ragdoll = value.isPressed;
    }

    private void LateUpdate()
    {
        baseCollider.material.dynamicFriction = walk.magnitude > 0 ? 0 : baseDynamicFriction;
        baseCollider.material.frictionCombine = walk.magnitude > 0 ? PhysicMaterialCombine.Minimum : baseCombine;
        isGrounded = Grounded();
        if (!ragdollLocked) hasRagdolled &= !isGrounded;
        animator.SetBool("Grounded", isGrounded);
        Vector2 inputWalk = (isGrounded? 1 : airControlMultiplier) * speed * Time.deltaTime * walk;
        if (Mathf.Abs(inputWalk.x) > 0) lastX = inputWalk.x;
        animator.SetBool("Moving", inputWalk.magnitude > 0);
        if (!Ragdoll)
        {
            bool currentFlipped = lastX < 0;
            if (currentFlipped != isFlipped)
            {
                isFlipped = currentFlipped;
                Flip();
            }
            transform.rotation = Quaternion.Euler(0, isFlipped ? 180 : 0, 0);
            float upAngle = Vector3.SignedAngle(Vector3.up, Vector3.Scale(new Vector3(groundNormal.x * (isFlipped ? -1 : 1), groundNormal.y, groundNormal.z), new Vector3(1, 1, 0)), Vector3.forward);
            if (jauntyRotate) transform.rotation *= isGrounded? Quaternion.AngleAxis(Mathf.Abs(upAngle) < maxSlope? upAngle : maxSlope * Mathf.Sign(upAngle), Vector3.forward) : Quaternion.identity;
            spriteMaterial.SetInt("_SpriteFlipped", isFlipped ? 1 : 0);
        }
        Vector3 moveVector = new Vector3(inputWalk.x, 0, inputWalk.y);
        Vector3 planeVector = Vector3.ProjectOnPlane(moveVector, steepGroundNormal);
        if (planeVector.y > 0) moveVector = planeVector;
        float slopeAngle = Vector3.Angle(Vector3.up, steepGroundNormal);
        moveVector = new Vector3(moveVector.x, Mathf.Max(moveVector.y, 0), moveVector.z);
        if (slopeAngle > maxSlope) moveVector = Vector3.Scale(moveVector, new Vector3(1, 0, 1));
        else moveVector = Vector3.Scale(moveVector, new Vector3(1, 1, 1));
        (Ragdoll? ragdollRootRigidbody : mainRigidbody).AddForce(moveVector, ForceMode.Acceleration);
        if (new Vector3(mainRigidbody.velocity.x, 0, mainRigidbody.velocity.z).magnitude > maxSpeed) mainRigidbody.velocity = new Vector3(mainRigidbody.velocity.x, 0, mainRigidbody.velocity.z).normalized * maxSpeed + Vector3.up * mainRigidbody.velocity.y;
    }

    private void OnJump()
    {
        if (!isGrounded || Ragdoll) return;
        else
        {
            mainRigidbody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    private bool Grounded()
    {
        CapsuleCollider collider = mainRigidbody.GetComponent<CapsuleCollider>();
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + (collider.height / 2) * Vector3.up, collider.radius, -transform.up, collider.height / 2 + 0.1f, groundedLayers);
        if (hits.Length > 0)
        {
            RaycastHit steepestHit = default, closestHit = default;
            float steepestAngle = 0, closestDistance = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                float hitAngle = Vector3.Angle(Vector3.up, hit.normal);
                if (hitAngle > steepestAngle) { steepestAngle = hitAngle; steepestHit = hit; }
                if (hit.distance < closestDistance) { closestDistance = hit.distance; closestHit = hit; }
            }
            groundNormal = closestHit.normal;
            steepGroundNormal = steepestHit.normal;
            return true;
        }
        else return false;
    }

    private void SetRagdoll(bool enabled)
    {
        foreach (Rigidbody rigidbody in transform.GetComponentsInChildren<Rigidbody>())
        {
            if (rigidbody.transform.parent == holdTransform) continue;
            if (mainRigidbody == rigidbody)
            {
                if (!enabled) rigidbody.velocity = ragdollRootRigidbody.velocity;
                continue;
            }
            rigidbody.isKinematic = !enabled;
            if (!enabled) continue;
            rigidbody.velocity = enabled? mainRigidbody.velocity : Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        if (!enabled)
        {
            mainRigidbody.transform.position = ragdollRootRigidbody.transform.position;
        }
        mainRigidbody.GetComponent<CapsuleCollider>().enabled = !enabled;
        mainRigidbody.useGravity = !enabled;
        animator.enabled = !enabled;
        if (!hasRagdolled)
        {
            ragdollRootRigidbody.AddForce(mainRigidbody.velocity.normalized * ragdollBoost, ForceMode.Impulse);
            Mechanism nearestMechanism = NearestInteractable(interactables.Where(interactable => interactable.GetType().IsSubclassOf(typeof(Mechanism))).ToList()) as Mechanism;
            if (nearestMechanism != null) nearestMechanism.Activate();
        }
        if (enabled) hasRagdolled = true;
        if (isFlipped) Flip();
    }

    private IInteractable NearestInteractable(List<IInteractable> checkedInteractables = null)
    {
        IInteractable closestInteractable = null;
        float nearestDistance = float.MaxValue;
        foreach (IInteractable interactable in checkedInteractables ?? interactables)
        {
            if (interactable is Prop && (interactable as Prop).isHeld) continue;
            float interactableDistance = Vector3.Distance(interactable.transform.position, holdTransform.position);
            if (interactableDistance < nearestDistance)
            {
                closestInteractable = interactable;
                nearestDistance = interactableDistance;
            }
        }
        return closestInteractable;
    }

    private bool InteractableAvailable()
    {
        return interactables != null && interactables.Where(interactable => interactable is not Prop || !(interactable as Prop).isHeld).ToList().Count > 0;
    }

    private void OnInteract()
    {
        if (nextInteractable != null) return;
        Mechanism nearestMechanism = NearestInteractable(interactables.Where(interactable => interactable.GetType().IsSubclassOf(typeof(Mechanism))).ToList()) as Mechanism;
        if (holdTransform.childCount > 0)
        {
            if (nearestMechanism != null) { foreach (SpriteRenderer spriteRenderer in holdTransform.GetChild(0).GetComponentsInChildren<SpriteRenderer>()) spriteRenderer.material = unflippedSpriteMaterial; nearestMechanism.Interact(this); }
            else (holdTransform.GetChild(0).GetComponent<IInteractable>() as Prop).Activate();
            return;
        }
        if (InteractableAvailable())
        {
            nextInteractable = NearestInteractable();
            if (nextInteractable is Prop) animator.SetTrigger("Grab");
            else InteractNext();
        }
    }

    protected override void InteractNext()
    {
        base.InteractNext();
        SetPrompts();
    }

    private void OnDrop()
    {
        Drop();
        SetPrompts();
    }

    private void SetPrompts()
    {
        string ePrompt = "";
        if (holdTransform.childCount > 0) ePrompt = holdTransform.GetChild(0).GetComponent<Prop>().prompt;
        IInteractable nearestInteractable = NearestInteractable();
        if (string.IsNullOrEmpty(ePrompt))
        {
            if (nearestInteractable is Prop) ePrompt = "Pick Up";
        }
        if (nearestInteractable is Mechanism) ePrompt = (nearestInteractable as Mechanism).interactPrompt;
        HUD.SetEPrompt(ePrompt);
        HUD.SetShiftPrompt(nearestInteractable is Mechanism ? (nearestInteractable as Mechanism).shiftPrompt : "");
    }

    private void OnMenu()
    {
        HUD.TogglePause();
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable[] newInteractables = other.GetComponentsInChildren<IInteractable>();
        if (newInteractables.Length > 0) { interactables.AddRange(newInteractables); SetPrompts(); return; }

        if (other.gameObject.layer == LayerMask.NameToLayer("Camera Zone"))
        {
            GameManager.Camera.CurrentView = other.gameObject.GetComponent<CameraView>();
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable[] oldInteractables = other.GetComponentsInChildren<IInteractable>();
        if (oldInteractables.Length > 0)
        {
            interactables.RemoveAll(interactable => oldInteractables.Contains(interactable));
            interactables.TrimExcess();
            SetPrompts();
        }
    }
}
