using UnityEngine;

public interface IInteractable
{
    public Transform transform { get; }
    void Interact(MonoBehaviour caller);
}

public interface IActivate
{
    public Transform transform { get; }
    void Activate();
}