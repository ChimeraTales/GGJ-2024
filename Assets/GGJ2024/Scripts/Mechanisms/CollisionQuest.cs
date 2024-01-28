using UnityEngine;

public class CollisionQuest : MonoBehaviour
{
    [SerializeField] QuestTitle quest;
    [SerializeField] private bool completeOnCollision, completeOnTrigger, triggerForCake, triggerForNPC;
    [SerializeField] private GameObject[] TriggerObjects;

    private void CheckCollision(GameObject collider)
    {
        if (triggerForCake && collider.TryGetComponent<Cake>(out _)) { GameManager.CompleteQuest(quest); return; };
        if (triggerForNPC && collider.TryGetComponent<NPC>(out _)) { GameManager.CompleteQuest(quest); return; };
        foreach (GameObject go in TriggerObjects)
        {
            if (go == collider) GameManager.CompleteQuest(quest);
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (completeOnCollision) CheckCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (completeOnTrigger) CheckCollision(other.gameObject);
    }
}
