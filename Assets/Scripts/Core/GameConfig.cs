public static class GameConfig
{
    // ── Grid ──────────────────────────────────────────────────────────────────
    public const int GridRows   = 6;
    public const int GridCols   = 6;
    public const int TotalCells = GridRows * GridCols;

    // ── Word Level Generation ─────────────────────────────────────────────────
    public const int WordValidMin = 9;   // minimum valid words on the grid
    public const int WordValidMax = 15;  // maximum valid words on the grid

    // ── Letter Level Generation ───────────────────────────────────────────────
    public const int LetterTargetMin  = 7;   // minimum target letter appearances
    public const int LetterTargetMax  = 10;  // maximum target letter appearances
    public const int LetterOtherCount = 6;   // unique non-target letters per level
    public const int LetterOtherBase  = 2;   // minimum appearances per non-target letter
    public const int LetterOtherMax   = 8;   // maximum appearances per non-target letter

    // ── Monster Spawning ──────────────────────────────────────────────────────
    public const int MonsterSpawnMinDistance = 4; // minimum Manhattan distance from player at spawn
}
