using System.Collections.Generic;
using UnityEngine;

public class Horn : Prop
{
    [SerializeField] AudioClip[] clips;
    [SerializeField] float ragdollDuration, ragdollForce;

    private List<NPC> currentNPCs = new();
    Animation animationComponent;

    private void Start()
    {
        animationComponent = GetComponent<Animation>();
    }

    public override void Interact(MonoBehaviour caller)
    {
        base.Interact(caller);
    }

    public override void Activate()
    {
        animationComponent.Play();
        foreach (NPC npc in currentNPCs) StartCoroutine(npc.ForceRagdoll(ragdollDuration, Vector3.up * ragdollForce, true));
    }

    private void PlaySound()
    {
        if (clips.Length > 0) AudioManager.PlaySFX(clips[Random.Range(0, clips.Length)], gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out NPC nPCScript))
        {
            if (nPCScript.canStartle) currentNPCs.Add(nPCScript);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out NPC nPCScript))
        {
            if (currentNPCs.Contains(nPCScript))
            {
                currentNPCs.Remove(nPCScript);
                currentNPCs.TrimExcess();
            }
        }
    }
}