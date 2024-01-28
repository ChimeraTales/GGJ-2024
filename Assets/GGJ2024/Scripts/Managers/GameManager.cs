using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] QuestEntry[] quests;

    public bool sceneCountsTime;

    [HideInInspector] public static QuestEntry[] Quests { get {  return instance.quests; } }

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

    public static void CompleteQuest(QuestTitle title)
    {
        QuestEntry questEntry = Quests.First(quest => quest.title == title);
        if (questEntry.completed) return;
        questEntry.completed = true;
        HUD.CompleteQuest(title);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void End()
    {

    }

    [System.Serializable]
    public struct QuestEntry
    {
        public QuestTitle title;
        public string name;
        public string description;
        public Image questImage;
        [HideInInspector] public bool completed;
    }
}

public enum QuestTitle
{ 
    CakeKing,
    Startle3,
    Well,
    Fall,
    Fish
}