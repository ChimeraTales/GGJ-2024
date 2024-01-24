using UnityEngine;

public class Horn : Prop
{
    [SerializeField] AudioClip[] clips;

    public override void Interact(MonoBehaviour caller)
    {
        base.Interact(caller);
    }

    public override void Activate()
    {
        if (clips.Length > 0) AudioManager.PlaySFX(clips[Random.Range(0, clips.Length)], gameObject);
    }
}