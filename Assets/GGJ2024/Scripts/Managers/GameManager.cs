using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool sceneCountsTime;

    public static float CurrentTime
    {
        get { return instance.currentTime; }
    }

    public static FollowCamera Camera { get { return instance.cameraScript; } }
    public static Player Player
    {
        get
        {
            return instance.player;
        }
    }

    [SerializeField] private FollowCamera cameraScript;
    [SerializeField] private Player player;
    [SerializeField] private float startTime = 8f, endTime = 20f, gameDuration;

    private static GameManager instance;
    private float hourMultiplier, currentTime;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentTime = startTime;
        hourMultiplier = (endTime - startTime) / gameDuration;
    }

    // Update is called once per frame
    void Update()
    {
        if (sceneCountsTime) currentTime += Time.deltaTime * hourMultiplier;
        if (currentTime >= endTime) End();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void End()
    {

    }
}
