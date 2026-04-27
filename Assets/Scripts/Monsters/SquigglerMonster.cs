using UnityEngine;

// First two moves: toward the opposite side from entry.
// After that: random direction, with last direction twice as likely.
public class SquigglerMonster : MonsterBase
{
    protected override Vector2Int GetNextMove()
    {
        if (moveCount < 2)
            return entryDirection; // moves toward opposite side (entry dir points inward)

        return WeightedRandomDirection();
    }
}
