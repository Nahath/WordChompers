using UnityEngine;
using System.Collections;

// Abstract base for all monster types. Attach a subclass to each monster prefab.
// Requires: Animator, Image/RectTransform (as UI element on the game canvas).
// Animator states: Idle, MoveUp, MoveDown, MoveHorizontal, Eat
public abstract class MonsterBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Animator      animator;
    [SerializeField] protected RectTransform rectTransform;

    // Logical grid position. Values of -1 or 6 are off-board (entering/exiting).
    protected int row, col;

    protected Vector2Int entryDirection;   // direction of first move onto the board
    protected Vector2Int lastMoveDir;
    protected int        moveCount;

    private float baseInterval;           // set by MonsterSpawner from level

    // Subclasses override this to return their actual speed multiplier.
    // An interval is divided by this: 1.0 = normal, 1.43 = 30% faster, 1.67 = 40% faster.
    protected virtual float SpeedMultiplier => 1f;

    private Coroutine movementCoroutine;
    private bool      isDestroyed;
    private bool      animatorReady;
    private int       alertTintRow = -1;
    private int       alertTintCol = -1;

    // ── Initialization ────────────────────────────────────────────────────────

    public void Initialize(int startRow, int startCol, Vector2Int entryDir, float baseMoveInterval)
    {
        row            = startRow;
        col            = startCol;
        entryDirection = entryDir;
        lastMoveDir    = entryDir;
        moveCount      = 0;
        baseInterval   = baseMoveInterval;
        isDestroyed    = false;

        animatorReady = animator != null && animator.runtimeAnimatorController != null;
        if (!animatorReady)
            Debug.LogError($"[{GetType().Name}] No AnimatorController assigned — assign one to enable animations.");

        SnapToPosition(row, col);
        SetFacing(entryDir);
        movementCoroutine = StartCoroutine(MovementLoop());

        // Register with spawner so collisions can be checked.
        MonsterSpawner.Instance.RegisterMonster(this);
    }

    // ── Movement Loop ─────────────────────────────────────────────────────────

    private IEnumerator MovementLoop()
    {
        float interval = baseInterval / SpeedMultiplier;

        // Pre-entry alert: total pre-move wait = interval, with the last 1s showing the tint.
        float alertDuration = Mathf.Min(1f, interval);
        yield return new WaitForSeconds(interval - alertDuration);
        alertTintRow = row + entryDirection.y;
        alertTintCol = col + entryDirection.x;
        GridManager.Instance.SetCellAlertTint(alertTintRow, alertTintCol, true);
        AudioManager.Instance.PlaySFX("SFX/sfx_monster_alert");
        yield return new WaitForSeconds(alertDuration);
        GridManager.Instance.SetCellAlertTint(alertTintRow, alertTintCol, false);
        alertTintRow = -1;

        // Main movement loop — first iteration fires immediately after the alert clears.
        while (!isDestroyed)
        {
            if (!GameManager.Instance.CanPlayerAct)
            {
                yield return new WaitForSeconds(interval);
                continue;
            }

            Vector2Int dir = GetNextMove();
            lastMoveDir = dir;
            moveCount++;

            int nr = row + dir.y;
            int nc = col + dir.x;

            // Moving off board: destroy silently.
            if (nr < -1 || nr > GameConfig.GridRows || nc < -1 || nc > GameConfig.GridCols)
            {
                DestroyMonster();
                yield break;
            }

            row = nr;
            col = nc;

            ApplyFacing(dir);
            yield return StartCoroutine(SlideTo(row, col));
            CheckCollisions();

            yield return new WaitForSeconds(interval);
        }
    }

    // Slides the monster's visual position to the target cell (or off-board position).
    private IEnumerator SlideTo(int targetRow, int targetCol)
    {
        Vector2 from = rectTransform.position;
        Vector2 to   = GridManager.Instance.GetCellCenter(targetRow, targetCol);
        float   t    = 0f;
        float   dur  = 0.48f / SpeedMultiplier;

        while (t < dur)
        {
            t += Time.deltaTime;
            rectTransform.position = Vector2.Lerp(from, to, t / dur);
            yield return null;
        }
        rectTransform.position = to;
    }

    // ── Abstract move logic (implemented by each subclass) ────────────────────

    protected abstract Vector2Int GetNextMove();

    // ── Collision ─────────────────────────────────────────────────────────────

    private void CheckCollisions()
    {
        if (isDestroyed) return;

        // Check player collision.
        var (pr, pc) = PlayerController.Instance.GetPosition();
        if (row == pr && col == pc)
        {
            PlayEatAnimation();
            GameManager.Instance.ReportMonsterAtePlayer();
            return;
        }

        // Check monster-on-monster collision.
        MonsterSpawner.Instance.CheckMonsterCollision(this, row, col);
    }

    public void PlayEatAnimation()
    {
        if (animatorReady) animator.Play("Eat");
        AudioManager.Instance.PlaySFX("SFX/sfx_monster_eat");
    }

    // ── Destruction ───────────────────────────────────────────────────────────

    public void DestroyMonster()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        if (alertTintRow >= 0)
        {
            GridManager.Instance.SetCellAlertTint(alertTintRow, alertTintCol, false);
            alertTintRow = -1;
        }
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        MonsterSpawner.Instance.UnregisterMonster(this);
        Destroy(gameObject);
    }

    public (int row, int col) GetGridPosition() => (row, col);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SnapToPosition(int r, int c)
    {
        if (GridManager.Instance == null) return;
        rectTransform.position = GridManager.Instance.GetCellCenter(r, c);
    }

    // ── Facing ────────────────────────────────────────────────────────────────

    // Sets the horizontal flip only — does not change the animation state.
    private void SetFacing(Vector2Int dir)
    {
        float scaleX = (dir == Left) ? -1f : 1f;
        rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
    }

    // Sets facing AND plays the correct directional move animation.
    private void ApplyFacing(Vector2Int dir)
    {
        SetFacing(dir);
        if (!animatorReady) return;
        if      (dir == Up)   animator.Play("MoveUp");
        else if (dir == Down) animator.Play("MoveDown");
        else                  animator.Play("MoveHorizontal");
    }

    // Convenience: direction vectors
    protected static readonly Vector2Int Up    = new Vector2Int( 0, -1);
    protected static readonly Vector2Int Down  = new Vector2Int( 0,  1);
    protected static readonly Vector2Int Left  = new Vector2Int(-1,  0);
    protected static readonly Vector2Int Right = new Vector2Int( 1,  0);

    protected Vector2Int OppositeOf(Vector2Int dir)
        => new Vector2Int(-dir.x, -dir.y);

    protected Vector2Int RandomDirection()
    {
        var dirs = new[] { Up, Down, Left, Right };
        return dirs[Random.Range(0, dirs.Length)];
    }

    // Returns a direction weighted so lastMoveDir is twice as likely.
    protected Vector2Int WeightedRandomDirection()
    {
        var dirs = new[] { Up, Down, Left, Right };
        float total = 0f;
        foreach (var d in dirs) total += (d == lastMoveDir ? 2f : 1f);

        float roll = Random.Range(0f, total);
        float cum  = 0f;
        foreach (var d in dirs)
        {
            cum += (d == lastMoveDir ? 2f : 1f);
            if (roll <= cum) return d;
        }
        return lastMoveDir;
    }

    // Returns the direction that moves most toward the player (-1 if tied or on same row/col).
    // Prefers horizontal when equally close.
    protected Vector2Int DirectionTowardPlayer()
    {
        var (pr, pc) = PlayerController.Instance.GetPosition();
        int dr = pr - row;
        int dc = pc - col;

        if (dc != 0 && (Mathf.Abs(dc) >= Mathf.Abs(dr) || dr == 0))
            return dc > 0 ? Right : Left;
        if (dr != 0)
            return dr > 0 ? Down : Up;

        return lastMoveDir;
    }

    // Returns the direction that moves most away from the player.
    // Prefers horizontal when equally far.
    protected Vector2Int DirectionAwayFromPlayer()
    {
        var (pr, pc) = PlayerController.Instance.GetPosition();
        int dr = pr - row;
        int dc = pc - col;

        if (dc != 0 && (Mathf.Abs(dc) >= Mathf.Abs(dr) || dr == 0))
            return dc > 0 ? Left : Right;
        if (dr != 0)
            return dr > 0 ? Up : Down;

        return lastMoveDir;
    }
}
