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

    [SerializeField] float speed, viewSwitchSpeed;
    [SerializeField] Camera followCamera;
    [SerializeField] private CameraView currentView;

    private void Start()
    {
        followCamera = GetComponentInChildren<Camera>();
        if (currentView != null) target = currentView.target;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null) return;
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, (target.TryGetComponent(out Player player) ? player.FocusPoint : target).position, viewSwitchSpeed * Time.deltaTime), Quaternion.Slerp(transform.rotation, Quaternion.Euler(currentView.rotation), viewSwitchSpeed * Time.deltaTime));
        followCamera.transform.localPosition = Vector3.Lerp(followCamera.transform.localPosition, currentView.position, speed * Time.deltaTime);
    }
}