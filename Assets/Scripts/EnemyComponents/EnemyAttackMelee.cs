using System.Collections;
using UnityEngine;

public class EnemyAttackMelee : EnemyAttack, IDeathHandler
{
    public float damage;
    public float groundDamage;
    public float attackRadius = 2f;
    public AudioClip beatSound;
    public SpriteAnimator.Animation attackWindUp;
    public float attackDelay = 0.2f;
    public SpriteAnimator.Animation attack;
    private SpriteAnimator animator;
    public BombAnimator explosionPrefab;
    public Vector2 offset;
    public bool attackDirectlyAtTarget = false;
    
    private BombAnimator currentExplosion;
    private Enemy _enemy;
    
    protected override void Awake()
    {
        base.Awake();
        TryGetComponent(out animator);
        TryGetComponent(out _enemy);
    }
    
    
    public override IEnumerator PerformAttack(Vector3 target)
    {
        var attackPos = attackDirectlyAtTarget ? target : transform.position + (Vector3)offset;
        var toTheLeft = target.x < transform.position.x;
        if (_enemy?.enemyMovement?.IsCurrentFacingDirectionRight() != null)
        {
            toTheLeft = !_enemy.enemyMovement.IsCurrentFacingDirectionRight();
        }
        if (toTheLeft)
        {
            attackPos.x -= 2 * offset.x;
        }
        
        if (explosionPrefab != null)
        {
            currentExplosion = Instantiate(explosionPrefab, attackPos, Quaternion.identity, Level.I.spawnedObjectsParent);
            currentExplosion.transform.localScale *= attackRadius;
        }
        
        var totalDelay = attackDelay;
        if (animator != null && attackWindUp != null)
        {
            totalDelay += attackWindUp.frames.Length / attackWindUp.fps;
            currentExplosion?.Explode(totalDelay);
            yield return animator.PlayOnceCoroutine(attackWindUp);
        }
        yield return new WaitForSeconds(attackDelay);
        
        Level.I.Explode(attackPos, attackRadius, damage, groundDamage, DamageDealerType.Enemy); 
        if (beatSound != null)
        {
            SoundManager.I.PlaySfx(beatSound, attackPos, 5f);
        }
        if (animator != null && attack != null)
        {
            yield return animator.PlayOnceCoroutine(attack);
        }
    }

    public void Die()
    {
        StopAllCoroutines();
        if (currentExplosion != null)
        {
            Destroy(currentExplosion.gameObject);
        }
    }
}
