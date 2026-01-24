using System.Collections;
using UnityEngine;

public class EnemyAttackExplode: EnemyAttack
{
    public Bomb bomb;

    public override IEnumerator PerformAttack(Vector3 target)
    {
        yield return bomb.Explode(bomb.explosionDelay);
    }
}