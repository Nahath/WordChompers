using UnityEngine;

// First 3 moves: toward the opposite side of the board from entry.
// After that: 50% chance to move away from player (preferring horizontal when tied),
// otherwise continues in last direction.
public class ScaredyMonster : MonsterBase
{
    protected override Vector2Int GetNextMove()
    {
        if (moveCount < 3)
            return entryDirection;

        if (Random.value < 0.5f)
            return DirectionAwayFromPlayer();
        return lastMoveDir;
    }
}
