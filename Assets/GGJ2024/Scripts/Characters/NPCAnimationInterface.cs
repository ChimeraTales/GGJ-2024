using UnityEngine;

public class NPCAnimationInterface : MonoBehaviour
{
    public void InteractNext()
    {
        GetComponentInParent<NPC>().InteractNext();
    }
}
