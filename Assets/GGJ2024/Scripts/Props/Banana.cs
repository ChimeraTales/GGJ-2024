using UnityEngine;

public class Banana : Prop
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root.CompareTag("NPC"))
        {
            StartCoroutine(collision.gameObject.GetComponentInParent<NPC>().ForceRagdoll(5f, Vector3.up - collision.impulse, true));
        }
    }
}