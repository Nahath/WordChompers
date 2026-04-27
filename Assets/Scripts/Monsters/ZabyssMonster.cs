using UnityEngine;

// Behaves like Blagwerr but moves 40% faster.
// Does not appear until level 35.
public class ZabyssMonster : BlagwerrMonster
{
    protected override float SpeedMultiplier => 1f / 0.6f;
}
