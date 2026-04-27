using UnityEngine;

// 50% chance to move away from player (preferring horizontal when tied).
// Otherwise continues in last direction.
public class ScaredyMonster : MonsterBase
{
    protected override Vector2Int GetNextMove()
    {
        if (Random.value < 0.5f)
            return DirectionAwayFromPlayer();
        return lastMoveDir;
    }
}
