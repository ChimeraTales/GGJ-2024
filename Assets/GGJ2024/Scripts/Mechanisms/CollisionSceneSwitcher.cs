using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionSceneSwitcher : MonoBehaviour
{
    public bool on;

    private void OnCollisionEnter(Collision collision)
    {
        if (on && collision.transform.root.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
