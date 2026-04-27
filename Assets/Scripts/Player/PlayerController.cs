using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Attach to the Player UI Image GameObject in the Game scene.
// Requires: Animator, Image, RectTransform.
// Animator parameters: HasWord (bool), Move (trigger), Chomp (trigger), Sick (trigger)
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Animator     animator;
    [SerializeField] private Image        playerImage;
    [SerializeField] private RectTransform rectTransform;

    private const float MoveCooldown = 0.2f;
    private const float MoveAnimDur  = 0.08f;

    private int   row, col;
    private float cooldown;
    private bool  touchUp, touchDown, touchLeft, touchRight;
    private bool  pendingChomp;
    private bool  animatorReady;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        animatorReady = animator != null && animator.runtimeAnimatorController != null;
        if (!animatorReady)
            Debug.LogError("[PlayerController] No AnimatorController on Player — assign one to enable animations.");

        if (GameManager.Instance == null) { Debug.LogError("[PlayerController] GameManager not found — start from the MainMenu scene."); return; }
        GameManager.Instance.OnPlayerShouldReset  += ResetForLevel;
        GameManager.Instance.OnMonstersShouldClear += OnMonstersCleared;
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPlayerShouldReset  -= ResetForLevel;
        GameManager.Instance.OnMonstersShouldClear -= OnMonstersCleared;
    }

    public void ResetForLevel()
    {
        row = 0; col = 0;
        cooldown = 0f;
        ShowPlayer();
        SnapToCell(0, 0);
        UpdateIdleAnim();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlayerAct) return;

        // ── Chomp ────────────────────────────────────────────────────────────
        bool chomp = Input.GetKeyDown(KeyCode.Space)
                  || Input.GetButtonDown("Fire1")
                  || pendingChomp;
        pendingChomp = false;

        if (chomp)
        {
            if (animatorReady) animator.Play("Chomp");
            GridManager.Instance.TryChomp(row, col);
            return;
        }

        // ── Movement cooldown ─────────────────────────────────────────────────
        if (cooldown > 0f) { cooldown -= Time.deltaTime; return; }

        float axisX = Input.GetAxisRaw("Horizontal");
        float axisY = Input.GetAxisRaw("Vertical");

        bool up    = Input.GetKey(KeyCode.UpArrow)    || Input.GetKey(KeyCode.W) || touchUp    || axisY >  0.5f;
        bool down  = Input.GetKey(KeyCode.DownArrow)  || Input.GetKey(KeyCode.S) || touchDown  || axisY < -0.5f;
        bool left  = Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A) || touchLeft  || axisX < -0.5f;
        bool right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || touchRight || axisX >  0.5f;

        var dirs = new List<int>();
        if (up)    dirs.Add(0);
        if (down)  dirs.Add(1);
        if (left)  dirs.Add(2);
        if (right) dirs.Add(3);

        if (dirs.Count > 0)
            TryMove(dirs[Random.Range(0, dirs.Count)]);
    }

    private void TryMove(int dir)
    {
        int nr = row, nc = col;
        switch (dir)
        {
            case 0: nr--; break; // up
            case 1: nr++; break; // down
            case 2: nc--; break; // left
            case 3: nc++; break; // right
        }

        if (nr < 0 || nr >= GameConfig.GridRows || nc < 0 || nc >= GameConfig.GridCols) return;

        GridManager.Instance.SetCellPlayerOccupied(row, col, false);
        row = nr; col = nc;
        cooldown = MoveCooldown;

        AudioManager.Instance.PlaySFX("SFX/sfx_player_move");
        if (animatorReady) animator.Play("Move");
        StartCoroutine(SlideTo(row, col));
    }

    private IEnumerator SlideTo(int targetRow, int targetCol)
    {
        Vector2 from = rectTransform.position;
        Vector2 to   = GridManager.Instance.GetCellCenter(targetRow, targetCol);
        float   t    = 0f;

        while (t < MoveAnimDur)
        {
            t += Time.deltaTime;
            rectTransform.position = Vector2.Lerp(from, to, t / MoveAnimDur);
            yield return null;
        }
        rectTransform.position = to;

        GridManager.Instance.SetCellPlayerOccupied(row, col, true);
        UpdateIdleAnim();
        MonsterSpawner.Instance.CheckPlayerCollision(row, col);
    }

    private void SnapToCell(int r, int c)
    {
        if (GridManager.Instance == null) return;
        rectTransform.position = GridManager.Instance.GetCellCenter(r, c);
        GridManager.Instance.SetCellPlayerOccupied(r, c, true);
    }

    private void UpdateIdleAnim()
    {
        if (GridManager.Instance == null || !animatorReady) return;
        bool hasWord = GridManager.Instance.HasContentAt(row, col);
        animator.Play(hasWord ? "IdleWord" : "Idle");
    }

    private void OnMonstersCleared()
    {
        UpdateIdleAnim();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void PlaySickAnimation() { if (animatorReady) animator.Play("Sick"); }

    public void HidePlayer()
    {
        if (playerImage != null) playerImage.enabled = false;
    }

    public void ShowPlayer()
    {
        if (playerImage != null) playerImage.enabled = true;
    }

    public (int row, int col) GetPosition() => (row, col);

    // Called by D-pad UI buttons.
    public void SetDPadDirection(DPadButton.Direction dir, bool pressed)
    {
        switch (dir)
        {
            case DPadButton.Direction.Up:    touchUp    = pressed; break;
            case DPadButton.Direction.Down:  touchDown  = pressed; break;
            case DPadButton.Direction.Left:  touchLeft  = pressed; break;
            case DPadButton.Direction.Right: touchRight = pressed; break;
        }
    }

    // Called by the Chomp UI button.
    public void TriggerChomp() => pendingChomp = true;

    // Called by GameManager (controller pause button).
    public void CheckControllerPause()
    {
        // joystick button 7 is Start on most gamepads in Unity's legacy Input system
        if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            if (GameManager.Instance.State == GameState.Playing)
                GameManager.Instance.PauseGame();
        }
    }
}
