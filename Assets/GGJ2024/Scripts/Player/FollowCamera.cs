using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;

    [SerializeField] float speed;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
    }
}
