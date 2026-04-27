using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Attach to a GameObject in the Game scene.
// Manages the HUD: level text, header text, lives display, gear/pause button,
// and the mobile D-pad / Chomp panel.
public class GameplayUI : MonoBehaviour
{
    [Header("HUD Text")]
    [SerializeField] private TMP_Text levelNumberText;
    [SerializeField] private TMP_Text levelHeaderText;

    [Header("Lives Display")]
    [SerializeField] private Transform  livesContainer;  // horizontal layout group
    [SerializeField] private GameObject lifeIconPrefab;  // small player sprite image

    [Header("Gear / Pause")]
    [SerializeField] private Button gearButton;

    [Header("Overlay Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameCompletePanel;

    [Header("Mobile Controls (shown on Android/iOS)")]
    [SerializeField] private GameObject dpadPanel;
    [SerializeField] private GameObject chompButtonPanel;

    private readonly List<GameObject> lifeIcons = new List<GameObject>();

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Start()
    {
        if (GameManager.Instance == null) { Debug.LogError("[GameplayUI] GameManager not found — start from the MainMenu scene."); return; }

        gearButton.onClick.AddListener(() => GameManager.Instance.PauseGame());

        GameManager.Instance.OnStateChanged  += HandleStateChanged;
        GameManager.Instance.OnLivesChanged  += UpdateLivesDisplay;
        GameManager.Instance.OnLevelReady    += UpdateLevelText;

        // Show mobile controls only on touch-screen platforms.
        bool mobile = Application.isMobilePlatform;
        if (dpadPanel       != null) dpadPanel.SetActive(mobile);
        if (chompButtonPanel != null) chompButtonPanel.SetActive(mobile);

        // Initial lives display.
        UpdateLivesDisplay(GameManager.Instance.Lives);
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnStateChanged -= HandleStateChanged;
        GameManager.Instance.OnLivesChanged -= UpdateLivesDisplay;
        GameManager.Instance.OnLevelReady   -= UpdateLevelText;
    }

    // ── Controller pause button ───────────────────────────────────────────────

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape)
            || Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            if (GameManager.Instance.State == GameState.Playing)
                GameManager.Instance.PauseGame();
        }
    }

    // ── HUD Updates ───────────────────────────────────────────────────────────

    private void UpdateLevelText()
    {
        levelNumberText.text = $"Level {GameManager.Instance.CurrentLevel}";
        levelHeaderText.text = GameManager.Instance.CurrentLevelHeader;
    }

    private void UpdateLivesDisplay(int lives)
    {
        // lives = total lives (including current). Display extras = lives - 1.
        int extras = Mathf.Max(0, lives - 1);

        // Remove excess icons.
        while (lifeIcons.Count > extras)
        {
            int last = lifeIcons.Count - 1;
            Destroy(lifeIcons[last]);
            lifeIcons.RemoveAt(last);
        }

        // Add missing icons.
        while (lifeIcons.Count < extras)
        {
            var icon = Instantiate(lifeIconPrefab, livesContainer);
            lifeIcons.Add(icon);
        }
    }

    private void HandleStateChanged(GameState state)
    {
        if (pauseMenuPanel     != null) pauseMenuPanel.SetActive(state == GameState.Paused);
        if (gameOverPanel      != null) gameOverPanel.SetActive(state == GameState.GameOver);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(state == GameState.LevelComplete);
        if (gameCompletePanel  != null) gameCompletePanel.SetActive(state == GameState.GameComplete);
    }
}
