using System.Collections;
using UnityEngine;

public class EnemyAttackMelee : EnemyAttack
{
    public float damage;
    public float groundDamage;
    public float attackRadius = 2f;
    public AudioClip beatSound;
    
    protected override IEnumerator PerformAttack(Vector3 target)
    {
        Level.I.Explode(transform.position, attackRadius, damage, groundDamage, DamageDealerType.Enemy); 
        if (beatSound != null)
        {
            SoundManager.I.PlaySfx(beatSound, transform.position, 5f);
        }
        yield break;
    }
}
