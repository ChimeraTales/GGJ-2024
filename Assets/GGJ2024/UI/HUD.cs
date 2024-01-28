using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ePrompt, shiftPrompt;
    [SerializeField] private GameObject qPrompt, pauseMenu, questMenu, restartButton, questEntry;
    [SerializeField] private Transform questContainer;
    [SerializeField] private Sprite completedSprite;

    public InputSprite[] inputSprites;

    private static HUD instance;
    private string currentControlScheme;
    private Dictionary<QuestTitle, Image> questImages = new Dictionary<QuestTitle, Image>();
    private static PlayerInput playerInput;
    private PlayerInput PlayerInput
    {
        set { playerInput = value; }
        get
        {
            if (playerInput == null) playerInput = GameManager.Player.input;
            return playerInput;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        foreach (GameManager.QuestEntry quest in GameManager.Quests)
        {
            GameObject questObject = Instantiate(questEntry, questContainer);
            QuestEntry questScript = questObject.GetComponent<QuestEntry>();
            questScript.title.text = quest.name;
            questScript.description.text = quest.description;
            questImages.Add(quest.title, questScript.image);
        }
    }

    private void Update()
    {
        if (PlayerInput != null && currentControlScheme != PlayerInput.currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;
            SwitchControlScheme(currentControlScheme == "Gamepad");
        }
    }

    private void SwitchControlScheme(bool isGamepad)
    {
        foreach (InputSprite inputSprite in inputSprites)
        {
            inputSprite.image.sprite = isGamepad ? inputSprite.gamepadSprite : inputSprite.keyboardSprite;
            inputSprite.image.GetComponent<AspectRatioFitter>().aspectRatio = isGamepad ? 1 : inputSprite.keyboardAspectRatio;
        }
    }

    public static void SetEPrompt(string prompt)
    {
        bool stringEmpty = string.IsNullOrEmpty(prompt) || prompt == "";
        instance.ePrompt.transform.parent.gameObject.SetActive(!stringEmpty);
        instance.qPrompt.SetActive(!stringEmpty);
        instance.ePrompt.text = prompt;
    }

    public static void SetShiftPrompt(string prompt)
    {
        instance.shiftPrompt.text = string.IsNullOrEmpty(prompt) ? "Ragdoll" : prompt;
    }

    public static void Quests()
    {
        instance.questMenu.SetActive(!instance.questMenu.activeInHierarchy);
        instance.SetTime();
    }

    public static void Escape()
    {
        bool pauseActive = instance.pauseMenu.activeInHierarchy;
        bool questsActive = instance.questMenu.activeInHierarchy;
        if (pauseActive) instance.pauseMenu.SetActive(false);
        else if (questsActive) instance.questMenu.SetActive(false);
        else instance.pauseMenu.SetActive(true);
        instance.SetTime();
        if (!pauseActive) EventSystem.current.SetSelectedGameObject(instance.restartButton, new BaseEventData(EventSystem.current));
    }

    private void SetTime()
    {
        Time.timeScale = instance.pauseMenu.activeInHierarchy ? 0 : 1;
    }

    public static void CompleteQuest(QuestTitle title)
    {
        instance.questImages[title].sprite = instance.completedSprite;
    }

    [System.Serializable]
    public struct InputSprite
    {
        public Image image;
        public Sprite gamepadSprite, keyboardSprite;
        public float keyboardAspectRatio;
    }
}
