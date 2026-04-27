using UnityEngine;

// Behaves like Squiggler but moves 40% faster.
// Does not appear until level 25.
public class GallumpherMonster : SquigglerMonster
{
    protected override float SpeedMultiplier => 1f / 0.6f;
}
