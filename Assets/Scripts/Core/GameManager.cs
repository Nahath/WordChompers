using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GameMode  { ChompWords, ChompLetters }
public enum GameState { MainMenu, Playing, Paused, LevelComplete, GameOver, GameComplete }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────
    public event System.Action<GameState> OnStateChanged;
    public event System.Action<int>       OnLivesChanged;       // new lives value
    public event System.Action            OnLevelReady;         // UI can update
    public event System.Action            OnPlayerShouldReset;  // reset to (0,0)
    public event System.Action            OnMonstersShouldClear;

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Config")]
    [SerializeField] private int startingLives           = 4;   // 3 extras + current
    [SerializeField] private int livesGainEveryNLevels   = 5;

    // ── Public State ──────────────────────────────────────────────────────────
    public GameMode  Mode                { get; private set; }
    public GameState State               { get; private set; } = GameState.MainMenu;
    public int       CurrentLevel        { get; private set; }
    public int       Lives               { get; private set; }
    public string    CurrentCategoryName { get; private set; }
    public string    CurrentLevelHeader  { get; private set; }
    public int       CurrentDifficulty   { get; private set; }
    public char      CurrentTargetLetter { get; private set; }

    // Input is blocked during life-loss sequences or menu pauses.
    public bool CanPlayerAct => State == GameState.Playing && !isHandlingSequence;

    // ── Private ───────────────────────────────────────────────────────────────
    private bool           isHandlingSequence;
    private int            validChompsThisLevel;
    private bool           inExtendedReminderMode;
    private float          reminderTimer;
    private List<string>   previousLevelWords = new List<string>();
    private Dictionary<char, int> letterHistory = new Dictionary<char, int>();

    // Registered by GameSceneLoader once the Game scene is ready.
    private GridManager      gridManager;
    private PlayerController playerController;
    private MonsterSpawner   monsterSpawner;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Scene Component Registration ──────────────────────────────────────────

    public void RegisterSceneComponents(GridManager gm, PlayerController pc, MonsterSpawner ms)
    {
        gridManager      = gm;
        playerController = pc;
        monsterSpawner   = ms;
    }

    // ── Game Flow ─────────────────────────────────────────────────────────────

    public void StartGame(GameMode mode)
    {
        Mode = mode;
        CurrentLevel = 1;
        Lives = startingLives;
        previousLevelWords.Clear();
        letterHistory.Clear();
        OnLivesChanged?.Invoke(Lives);
        SceneManager.LoadScene("Game");
    }

    // Called by GameSceneLoader after all references are ready and data is loaded.
    public void BeginLevel()
    {
        validChompsThisLevel    = 0;
        inExtendedReminderMode  = false;
        reminderTimer           = 10f;
        isHandlingSequence      = false;

        if (Mode == GameMode.ChompWords)
            PrepareWordLevel();
        else
            PrepareLetterLevel();
    }

    private void PrepareWordLevel()
    {
        var loader = GameDataLoader.Instance;
        CurrentDifficulty = GetDifficultyForLevel(CurrentLevel);

        var eligible = loader.Categories
            .Where(c => c.minimumDifficulty <= CurrentDifficulty)
            .ToList();
        if (eligible.Count == 0) eligible = loader.Categories.ToList();

        if (eligible.Count == 0)
        {
            Debug.LogError("[GameManager] No category data — add StreamingAssets/Data/categories.json. Running letter mode instead.");
            PrepareLetterLevel();
            return;
        }

        var chosen = eligible[Random.Range(0, eligible.Count)];
        CurrentCategoryName = chosen.name;
        CurrentLevelHeader  = chosen.levelHeader;

        previousLevelWords = gridManager.GenerateWordLevel(
            CurrentDifficulty, CurrentCategoryName, previousLevelWords);

        AudioManager.Instance.PlaySFX("Categories/header_" + CurrentCategoryName.ToLower());

        playerController.ResetForLevel();
        monsterSpawner.StartSpawning();
        SetState(GameState.Playing);
        OnLevelReady?.Invoke();
    }

    private void PrepareLetterLevel()
    {
        CurrentTargetLetter = SelectTargetLetter();
        CurrentLevelHeader  = $"Chomp Letters: {CurrentTargetLetter}";

        gridManager.GenerateLetterLevel(CurrentTargetLetter);

        AudioManager.Instance.PlaySequence(new[]
        {
            "Spoken/spoken_chomp_the_letter",
            "Letters/letter_" + CurrentTargetLetter.ToString().ToLower()
        });

        playerController.ResetForLevel();
        monsterSpawner.StartSpawning();
        SetState(GameState.Playing);
        OnLevelReady?.Invoke();
    }

    private char SelectTargetLetter()
    {
        var pool = new List<(char letter, float weight)>();
        for (char c = 'A'; c <= 'Z'; c++)
        {
            int count = letterHistory.ContainsKey(c) ? letterHistory[c] : 0;
            float w = count == 0 ? 2f : count <= 2 ? 4f : count <= 4 ? 1f : 0f;
            if (w > 0f) pool.Add((c, w));
        }

        if (pool.Count == 0)
        {
            letterHistory.Clear();
            for (char c = 'A'; c <= 'Z'; c++) pool.Add((c, 2f));
        }

        float total = pool.Sum(p => p.weight);
        float roll  = Random.Range(0f, total);
        float cum   = 0f;
        char  sel   = pool[0].letter;
        foreach (var (letter, weight) in pool)
        {
            cum += weight;
            if (roll <= cum) { sel = letter; break; }
        }

        if (!letterHistory.ContainsKey(sel)) letterHistory[sel] = 0;
        letterHistory[sel]++;
        return sel;
    }

    private int GetDifficultyForLevel(int level)
    {
        var loader = GameDataLoader.Instance;
        if (loader?.Levels != null)
            foreach (var e in loader.Levels)
                if (e.level == level) return e.difficulty;
        return 5;
    }

    // ── Update / Reminder Timer ───────────────────────────────────────────────

    void Update()
    {
        if (State != GameState.Playing || isHandlingSequence) return;

        reminderTimer -= Time.deltaTime;
        if (reminderTimer <= 0f)
        {
            PlayReminderSound();
            reminderTimer = inExtendedReminderMode ? 30f : 10f;
        }
    }

    private void PlayReminderSound()
    {
        if (Mode == GameMode.ChompWords)
            AudioManager.Instance.PlaySFX("Categories/header_" + CurrentCategoryName.ToLower());
        else
            AudioManager.Instance.PlaySequence(new[]
            {
                "Spoken/spoken_chomp_the_letter",
                "Letters/letter_" + CurrentTargetLetter.ToString().ToLower()
            });
    }

    // ── Chomp Reports (called by GridManager) ─────────────────────────────────

    public void ReportValidChomp(string wordOrLetter)
    {
        AudioManager.Instance.PlaySFX("SFX/sfx_player_chomp_valid");
        validChompsThisLevel++;
        if (validChompsThisLevel >= 2)
            inExtendedReminderMode = true;
        if (inExtendedReminderMode)
            reminderTimer = 30f;
    }

    public void ReportInvalidChomp(string wordOrLetter)
    {
        if (!CanPlayerAct) return;
        StartCoroutine(InvalidChompSequence(wordOrLetter));
    }

    private IEnumerator InvalidChompSequence(string wordOrLetter)
    {
        isHandlingSequence = true;

        playerController.PlaySickAnimation();

        if (Mode == GameMode.ChompWords)
            AudioManager.Instance.PlaySequence(new[]
            {
                "Words/word_"           + wordOrLetter.ToLower(),
                "Spoken/spoken_is_not_a",
                "Categories/category_" + CurrentCategoryName.ToLower()
            });
        else
            AudioManager.Instance.PlaySequence(new[]
            {
                "Spoken/spoken_you_chomped",
                "Letters/letter_"      + wordOrLetter.ToLower(),
                "Spoken/spoken_only_chomp",
                "Letters/letter_"      + CurrentTargetLetter.ToString().ToLower()
            });

        yield return new WaitForSeconds(0.5f);

        OnMonstersShouldClear?.Invoke();
        monsterSpawner.ResetSpawnTimer();

        isHandlingSequence = false;
        HandleLifeLoss();
    }

    public void ReportMonsterAtePlayer()
    {
        if (!CanPlayerAct) return;
        StartCoroutine(MonsterEatSequence());
    }

    private IEnumerator MonsterEatSequence()
    {
        isHandlingSequence = true;

        playerController.HidePlayer();
        AudioManager.Instance.PlaySFX("SFX/sfx_monster_eat");
        AudioManager.Instance.PlaySFX("SFX/sfx_player_scream");

        yield return new WaitForSeconds(2f);

        OnMonstersShouldClear?.Invoke();
        monsterSpawner.ResetSpawnTimer();

        isHandlingSequence = false;
        HandleLifeLoss();
    }

    private void HandleLifeLoss()
    {
        Lives--;
        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            AudioManager.Instance.PlaySFX("SFX/sfx_game_over");
            SetState(GameState.GameOver);
        }
        else
        {
            OnPlayerShouldReset?.Invoke();
        }
    }

    public void ReportAllTargetsEaten(List<string> wordsThisLevel)
    {
        if (State != GameState.Playing) return;

        previousLevelWords = wordsThisLevel ?? new List<string>();
        AudioManager.Instance.PlaySFX("SFX/sfx_level_complete");

        if (CurrentLevel % livesGainEveryNLevels == 0)
        {
            Lives++;
            OnLivesChanged?.Invoke(Lives);
        }

        SetState(CurrentLevel >= 100 ? GameState.GameComplete : GameState.LevelComplete);
    }

    // ── Level / Pause Controls ────────────────────────────────────────────────

    public void ProceedToNextLevel()
    {
        CurrentLevel++;
        BeginLevel();
    }

    public void PauseGame()
    {
        if (State == GameState.Playing && !isHandlingSequence)
            SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (State == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void ReturnToMainMenu()
    {
        monsterSpawner?.StopSpawning();
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    private void SetState(GameState s)
    {
        State = s;
        OnStateChanged?.Invoke(s);
    }
}
