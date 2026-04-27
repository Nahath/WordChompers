using UnityEngine;

// Always moves in the entry direction. Never turns.
// Moves 30% faster than base (interval divided by 1/0.7 ≈ 1.43).
public class GorblerMonster : MonsterBase
{
    protected override float SpeedMultiplier => 1f / 0.7f;

    protected override Vector2Int GetNextMove() => entryDirection;
}
