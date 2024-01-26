using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ePrompt;
    [SerializeField] private GameObject qPrompt;

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
        instance.ePrompt.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
        instance.qPrompt.SetActive(!string.IsNullOrEmpty(prompt) && prompt == "Use");
        instance.ePrompt.text = prompt;
    }

    [System.Serializable]
    public struct InputSprite
    {
        public Image image;
        public Sprite gamepadSprite, keyboardSprite;
        public float keyboardAspectRatio;
    }
}
