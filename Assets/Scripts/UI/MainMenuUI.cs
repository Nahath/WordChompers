using UnityEngine;
using UnityEngine.UI;

// Attach to a GameObject in the MainMenu scene.
// Wire both buttons in the inspector.
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button chompLettersButton;
    [SerializeField] private Button chompWordsButton;

    void Start()
    {
        chompLettersButton.onClick.AddListener(() => StartGame(GameMode.ChompLetters));
        chompWordsButton.onClick.AddListener(()   => StartGame(GameMode.ChompWords));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            StartGame(GameMode.ChompLetters);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            StartGame(GameMode.ChompWords);
    }

    private void StartGame(GameMode mode)
    {
        AudioManager.Instance.PlaySFX("SFX/sfx_menu_button_press");
        GameManager.Instance.StartGame(mode);
    }
}
