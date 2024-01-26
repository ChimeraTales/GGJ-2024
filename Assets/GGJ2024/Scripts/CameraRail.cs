using UnityEngine;

public class CameraRail : MonoBehaviour
{
    [SerializeField] bool lockY, lockX;
    [SerializeField] Player player;

    private void Start()
    {
        if (player == null) player = GameManager.Player;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Transform current = player.Ragdoll ? player.ragdollRootRigidbody.transform : player.transform;
        transform.position = new Vector3(lockX? transform.position.x : current.position.x, lockY ? transform.position.y : current.position.y, transform.position.z);
    }
}