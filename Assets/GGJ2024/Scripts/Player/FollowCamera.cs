using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public CameraView CurrentView
    {
        get { return currentView; }
        set
        {
            currentView = value;
            target = value.target;
        }
    }

    [SerializeField] float speed;
    [SerializeField] Camera followCamera;
    [SerializeField] private CameraView currentView;

    private void Start()
    {
        followCamera = GetComponentInChildren<Camera>();
        target = currentView.target;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null) return;
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime), Quaternion.Slerp(transform.rotation, Quaternion.Euler(currentView.rotation), speed * Time.deltaTime));
        followCamera.transform.localPosition = Vector3.Lerp(followCamera.transform.localPosition, currentView.position, speed * Time.deltaTime);
    }
}
