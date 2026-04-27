using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Place on a GameObject in the Game scene. Assign cellPrefab and gridContainer in the inspector.
// gridContainer should have a GridLayoutGroup set to 6 columns, cell size 160x100.
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GridCell       cellPrefab;
    [SerializeField] private RectTransform  gridContainer;

    [Header("Cell Size (must match GridLayoutGroup)")]
    [SerializeField] private float cellWidth  = 160f;
    [SerializeField] private float cellHeight = 100f;

    private GridCell[,] cells       = new GridCell[6, 6];
    private int         validCount;
    private bool        levelDone;
    private List<string> allWordsOnGrid = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 6; c++)
                cells[r, c] = Instantiate(cellPrefab, gridContainer);
    }

    // ── Level Generation ──────────────────────────────────────────────────────

    // Returns the list of ALL words placed (for previous-level weighting next time).
    public List<string> GenerateWordLevel(int difficulty, string category, List<string> previousWords)
    {
        allWordsOnGrid.Clear();
        levelDone  = false;
        validCount = 0;

        var loader  = GameDataLoader.Instance;

        if (loader.Words == null || loader.Words.Length == 0)
        {
            Debug.LogError("[GridManager] No word data — add StreamingAssets/Data/words.json. Grid will be empty.");
            for (int r = 0; r < 6; r++)
                for (int c = 0; c < 6; c++)
                    cells[r, c].ClearContent();
            return new List<string>();
        }

        var allElig = loader.Words.Where(w => w.difficulty <= difficulty).ToList();
        if (allElig.Count == 0) allElig = loader.Words.ToList();

        var inCat  = allElig.Where(w => w.categories.Contains(category)).ToList();
        var outCat = allElig.Where(w => !w.categories.Contains(category)).ToList();

        if (inCat.Count == 0)
        {
            Debug.LogWarning($"No words for category '{category}' at difficulty {difficulty}. Using all eligible words.");
            inCat  = allElig.Take(Mathf.Max(1, allElig.Count / 2)).ToList();
            outCat = allElig.Skip(inCat.Count).ToList();
        }
        if (outCat.Count == 0) outCat = inCat; // fallback

        int validTarget = Random.Range(9, 16); // 9..15
        int invalidTarget = 36 - validTarget;

        var slots = new List<(string word, bool isValid)>();
        for (int i = 0; i < validTarget;  i++) slots.Add((WeightedRandom(inCat,  previousWords).word, true));
        for (int i = 0; i < invalidTarget; i++) slots.Add((WeightedRandom(outCat, previousWords).word, false));

        Shuffle(slots);

        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 6; c++)
            {
                var (word, isValid) = slots[r * 6 + c];
                cells[r, c].SetWord(word, isValid);
                allWordsOnGrid.Add(word);
                if (isValid) validCount++;
            }

        return new List<string>(allWordsOnGrid);
    }

    public void GenerateLetterLevel(char targetLetter)
    {
        allWordsOnGrid.Clear();
        levelDone  = false;
        validCount = 0;

        var all = Enumerable.Range('A', 26).Select(i => (char)i).ToList();
        all.Remove(targetLetter);
        Shuffle(all);
        var others = all.Take(7).ToList();

        int targetCount = Random.Range(5, 9); // 5..8
        int remaining   = 36 - targetCount;   // 28..31

        int[] otherCounts = Enumerable.Repeat(2, 7).ToArray();
        remaining -= 14;

        var idxOrder = Enumerable.Range(0, 7).OrderBy(_ => Random.value).ToList();
        int cursor = 0;
        while (remaining > 0)
        {
            int i = idxOrder[cursor % 7];
            if (otherCounts[i] < 8) { otherCounts[i]++; remaining--; }
            cursor++;
        }

        var letters = new List<char>();
        for (int i = 0; i < targetCount; i++) letters.Add(targetLetter);
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < otherCounts[i]; j++)
                letters.Add(others[i]);

        Shuffle(letters);

        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 6; c++)
            {
                char letter  = letters[r * 6 + c];
                bool isValid = letter == targetLetter;
                cells[r, c].SetLetter(letter, isValid);
                allWordsOnGrid.Add(letter.ToString());
                if (isValid) validCount++;
            }
    }

    // ── Chomp ─────────────────────────────────────────────────────────────────

    public void TryChomp(int row, int col)
    {
        if (row < 0 || row >= 6 || col < 0 || col >= 6) return;
        var cell = cells[row, col];
        if (!cell.HasContent) return;

        bool   isValid = cell.IsValidTarget;
        string content = cell.DisplayText;
        cell.ClearContent();

        if (isValid)
        {
            validCount--;
            GameManager.Instance.ReportValidChomp(content);
            if (validCount <= 0 && !levelDone)
            {
                levelDone = true;
                GameManager.Instance.ReportAllTargetsEaten(new List<string>(allWordsOnGrid));
            }
        }
        else
        {
            GameManager.Instance.ReportInvalidChomp(content);
        }
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public bool HasContentAt(int row, int col)
    {
        if (row < 0 || row >= 6 || col < 0 || col >= 6) return false;
        return cells[row, col].HasContent;
    }

    // Returns world/canvas position of a cell (or extrapolated off-board position for monsters).
    public Vector2 GetCellCenter(int row, int col)
    {
        if (row >= 0 && row < 6 && col >= 0 && col < 6)
            return cells[row, col].transform.position;

        int cr = Mathf.Clamp(row, 0, 5);
        int cc = Mathf.Clamp(col, 0, 5);
        Vector2 refPos = cells[cr, cc].transform.position;
        return refPos + new Vector2((col - cc) * cellWidth, -(row - cr) * cellHeight);
    }

    public void SetCellPlayerOccupied(int row, int col, bool occupied)
    {
        if (row < 0 || row >= 6 || col < 0 || col >= 6) return;
        cells[row, col].SetPlayerOccupied(occupied);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private WordData WeightedRandom(List<WordData> pool, List<string> prev)
    {
        float total = 0f;
        foreach (var w in pool)
        {
            float wt = w.difficulty;
            if (prev != null && prev.Contains(w.word)) wt *= 3f;
            total += wt;
        }

        float roll = Random.Range(0f, total);
        float cum  = 0f;
        foreach (var w in pool)
        {
            float wt = w.difficulty;
            if (prev != null && prev.Contains(w.word)) wt *= 3f;
            cum += wt;
            if (roll <= cum) return w;
        }
        return pool[pool.Count - 1];
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
