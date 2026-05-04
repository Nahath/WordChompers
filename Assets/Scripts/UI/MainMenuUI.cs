using UnityEngine;
using UnityEngine.UI;

// Attach to a GameObject in the MainMenu scene.
// Wire all buttons in the inspector.
public class MainMenuUI : MonoBehaviour
{
    [Header("Mode Buttons")]
    [SerializeField] private Button chompLettersButton;
    [SerializeField] private Button chompWordsButton;
    [SerializeField] private Button exitButton;

    [Header("Character Select")]
    [SerializeField] private Button boyButton;
    [SerializeField] private Button girlButton;

    private static readonly Color SelectedColor   = Color.white;
    private static readonly Color UnselectedColor = new Color(1f, 1f, 1f, 0.45f);

    void Start()
    {
        chompLettersButton.onClick.AddListener(() => StartGame(GameMode.ChompLetters));
        chompWordsButton.onClick.AddListener(()   => StartGame(GameMode.ChompWords));
        exitButton.onClick.AddListener(ExitGame);

        boyButton.onClick.AddListener(() =>  SelectCharacter(CharacterType.Boy));
        girlButton.onClick.AddListener(() => SelectCharacter(CharacterType.Girl));

        UpdateCharacterButtons();
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

    private void SelectCharacter(CharacterType type)
    {
        AudioManager.Instance.PlaySFX("SFX/sfx_menu_button_press");
        GameManager.Instance.SetCharacter(type);
        UpdateCharacterButtons();
    }

    private void UpdateCharacterButtons()
    {
        SetNormalColor(boyButton,  GameManager.Instance.SelectedCharacter == CharacterType.Boy);
        SetNormalColor(girlButton, GameManager.Instance.SelectedCharacter == CharacterType.Girl);
    }

    private void SetNormalColor(Button btn, bool selected)
    {
        var colors = btn.colors;
        colors.normalColor = selected ? SelectedColor : UnselectedColor;
        btn.colors = colors;
    }

    private void ExitGame()
    {
        AudioManager.Instance.PlaySFX("SFX/sfx_menu_button_press");
        Application.Quit();
    }
}
