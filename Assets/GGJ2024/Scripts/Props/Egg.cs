using UnityEngine;

public class Egg : Prop
{
    [SerializeField] float breakImpulse;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && collision.impulse.magnitude >= breakImpulse)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            transform.rotation = Quaternion.identity;
            GetComponent<Animator>().SetTrigger("Break");
            GetComponent<Collider>().enabled = false;
            GameManager.CompleteQuest(QuestTitle.Egg);
        }
    }
}
