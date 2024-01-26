using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanism : MonoBehaviour, IInteractable, IActivate
{
    public string interactPrompt = "Interact", shiftPrompt = "Activate";

    public virtual void Interact(MonoBehaviour caller)
    {
        switch (caller)
        {
            case Player player:
                if (player.holdTransform.childCount > 0)
                {
                    player.holdTransform.GetChild(0);
                }
                break;
            default:
                break;
        }
    }

    public virtual void Activate()
    {
        
    }

    public virtual bool CanInteract()
    {
        return true;
    }
}