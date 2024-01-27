using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ePrompt, shiftPrompt;
    [SerializeField] private GameObject qPrompt, pauseMenu, restartButton;

    public InputSprite[] inputSprites;

    private static HUD instance;
    private string currentControlScheme;
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

    private void OnEnable()
    {
        instance = this;
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

    public static void TogglePause()
    {
        bool currentlyActive = instance.pauseMenu.activeInHierarchy;
        Time.timeScale = currentlyActive ? 1 : 0;
        instance.pauseMenu.SetActive(!currentlyActive);
        EventSystem.current.SetSelectedGameObject(instance.restartButton, new BaseEventData(EventSystem.current));
    }

    [System.Serializable]
    public struct InputSprite
    {
        public Image image;
        public Sprite gamepadSprite, keyboardSprite;
        public float keyboardAspectRatio;
    }
}
