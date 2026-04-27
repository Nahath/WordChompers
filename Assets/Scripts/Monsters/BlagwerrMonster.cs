using UnityEngine;

// 50% chance to move toward player (preferring horizontal when tied).
// Otherwise continues in last direction.
// Does not appear until level 15.
public class BlagwerrMonster : MonsterBase
{
    protected override Vector2Int GetNextMove()
    {
        if (Random.value < 0.5f)
            return DirectionTowardPlayer();
        return lastMoveDir;
    }
}
