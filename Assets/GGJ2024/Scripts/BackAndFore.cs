using UnityEngine;

public class BackAndFore : MonoBehaviour
{
    [SerializeField] bool isAngular;
    [SerializeField] float speed;
    [SerializeField] Vector3 axis;
    [SerializeField] Vector2 minMax;

    private Vector3 startVector;
    private int dir = 1;

    private void Start()
    {
        startVector = isAngular? transform.localRotation.eulerAngles : transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float offset = 0;
        if (isAngular)
        {
            transform.localRotation = transform.localRotation * Quaternion.Euler(dir * speed * axis);
            offset = Quaternion.Angle(transform.localRotation, Quaternion.Euler(startVector));
        }
        else
        {
            transform.localPosition += speed * dir * Time.deltaTime * axis;
            offset = (transform.localPosition - startVector).magnitude;
        }
        if (offset <= minMax.x || offset >= minMax.y)
        {
            dir *= -1;
        }
    }
}
