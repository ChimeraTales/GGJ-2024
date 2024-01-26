using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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
        currentTime += Time.deltaTime * hourMultiplier;
        if (currentTime >= endTime) End();
    }

    void End()
    {

    }
}
