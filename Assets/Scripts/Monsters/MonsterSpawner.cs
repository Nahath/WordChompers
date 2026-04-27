using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Manages monster spawning timer and the list of active monsters.
// Assign monster prefabs in the inspector.
public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner Instance { get; private set; }

    [Header("Monster Prefabs")]
    [SerializeField] private MonsterBase squigglerPrefab;
    [SerializeField] private MonsterBase gorblerPrefab;
    [SerializeField] private MonsterBase scaredyPrefab;
    [SerializeField] private MonsterBase blagwerrPrefab;    // level 15+
    [SerializeField] private MonsterBase gallumpherPrefab;  // level 25+
    [SerializeField] private MonsterBase zabyssprefab;      // level 35+

    [Header("Spawn Container")]
    [SerializeField] private Transform monsterContainer; // parent Transform for spawned monsters

    private readonly List<MonsterBase> activeMonsters = new List<MonsterBase>();
    private Coroutine spawnCoroutine;

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance == null) { Debug.LogError("[MonsterSpawner] GameManager not found — start from the MainMenu scene."); return; }
        GameManager.Instance.OnMonstersShouldClear += ClearAllMonsters;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMonstersShouldClear -= ClearAllMonsters;
    }

    // ── Spawning ──────────────────────────────────────────────────────────────

    public void StartSpawning()
    {
        StopSpawning();
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }

    public void ResetSpawnTimer() => StartSpawning();

    private IEnumerator SpawnLoop()
    {
        // TEMP: fast first spawn for testing
        yield return new WaitForSeconds(3f);
        if (GameManager.Instance.State == GameState.Playing)
            TrySpawnMonster();

        while (true)
        {
            // TEMP: 50% interval reduction for testing
            float interval = Mathf.Max(5f, 20f - (GameManager.Instance.CurrentLevel / 18f)) * 0.5f;
            yield return new WaitForSeconds(interval);

            if (GameManager.Instance.State == GameState.Playing)
                TrySpawnMonster();
        }
    }

    private void TrySpawnMonster()
    {
        var positions = GetValidSpawnPositions();
        if (positions.Count == 0) return;

        var (spawnRow, spawnCol, dir) = positions[Random.Range(0, positions.Count)];
        var prefab = PickMonsterPrefab();
        if (prefab == null) return;

        float baseMoveInterval = CalculateBaseMoveInterval();
        var monster = Instantiate(prefab, monsterContainer);
        monster.Initialize(spawnRow, spawnCol, dir, baseMoveInterval);
    }

    // ── Spawn Position Selection ──────────────────────────────────────────────

    private List<(int row, int col, Vector2Int dir)> GetValidSpawnPositions()
    {
        var (pr, pc) = PlayerController.Instance.GetPosition();
        var valid    = new List<(int, int, Vector2Int)>();

        // Non-corner edge slots. Monster starts one step OUTSIDE the board.

        // Top edge (cols 1-4), enters moving Down (row increases)
        for (int c = 1; c <= 4; c++)
            if (ManhattanDist(0, c, pr, pc) >= 4)
                valid.Add((-1, c, new Vector2Int(0, 1)));

        // Bottom edge (cols 1-4), enters moving Up (row decreases)
        for (int c = 1; c <= 4; c++)
            if (ManhattanDist(5, c, pr, pc) >= 4)
                valid.Add((6, c, new Vector2Int(0, -1)));

        // Left edge (rows 1-4), enters moving Right
        for (int r = 1; r <= 4; r++)
            if (ManhattanDist(r, 0, pr, pc) >= 4)
                valid.Add((r, -1, Vector2Int.right));

        // Right edge (rows 1-4), enters moving Left
        for (int r = 1; r <= 4; r++)
            if (ManhattanDist(r, 5, pr, pc) >= 4)
                valid.Add((r, 6, Vector2Int.left));

        return valid;
    }

    private static int ManhattanDist(int r1, int c1, int r2, int c2)
        => Mathf.Abs(r1 - r2) + Mathf.Abs(c1 - c2);

    // ── Monster Type Selection ────────────────────────────────────────────────

    private MonsterBase PickMonsterPrefab()
    {
        int level = GameManager.Instance.CurrentLevel;
        var pool  = new List<MonsterBase>();

        if (squigglerPrefab  != null) pool.Add(squigglerPrefab);
        if (gorblerPrefab    != null) pool.Add(gorblerPrefab);
        if (scaredyPrefab    != null) pool.Add(scaredyPrefab);
        if (level >= 15 && blagwerrPrefab   != null) pool.Add(blagwerrPrefab);
        if (level >= 25 && gallumpherPrefab != null) pool.Add(gallumpherPrefab);
        if (level >= 35 && zabyssprefab     != null) pool.Add(zabyssprefab);

        return pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null;
    }

    // ── Move Interval Calculation ─────────────────────────────────────────────

    // Decreases 20% every 10 levels: 2 * 0.8^floor(level/10)
    public static float CalculateBaseMoveInterval()
    {
        int level = GameManager.Instance.CurrentLevel;
        return 2f * Mathf.Pow(0.8f, Mathf.Floor(level / 10f));
    }

    // ── Monster Registry ──────────────────────────────────────────────────────

    public void RegisterMonster(MonsterBase m)
    {
        if (!activeMonsters.Contains(m)) activeMonsters.Add(m);
    }

    public void UnregisterMonster(MonsterBase m) => activeMonsters.Remove(m);

    public void ClearAllMonsters()
    {
        var copy = new List<MonsterBase>(activeMonsters);
        foreach (var m in copy) m.DestroyMonster();
        activeMonsters.Clear();
    }

    // ── Collision Helpers ─────────────────────────────────────────────────────

    public void CheckPlayerCollision(int playerRow, int playerCol)
    {
        foreach (var m in new List<MonsterBase>(activeMonsters))
        {
            var (mr, mc) = m.GetGridPosition();
            if (mr == playerRow && mc == playerCol)
            {
                m.PlayEatAnimation();
                GameManager.Instance.ReportMonsterAtePlayer();
                return;
            }
        }
    }

    public void CheckMonsterCollision(MonsterBase mover, int moverRow, int moverCol)
    {
        foreach (var other in new List<MonsterBase>(activeMonsters))
        {
            if (other == mover) continue;
            var (or, oc) = other.GetGridPosition();
            if (or == moverRow && oc == moverCol)
            {
                bool moverWins = Random.value < 0.5f;
                var loser  = moverWins ? other : mover;
                var winner = moverWins ? mover : other;
                winner.PlayEatAnimation();
                loser.DestroyMonster();
                return;
            }
        }
    }
}
