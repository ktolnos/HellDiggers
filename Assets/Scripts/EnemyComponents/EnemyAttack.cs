using System.Collections;
using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour
{
    public float attackCoolDown;
    public float attackDelay = 0.7f;
    public SpriteAnimator.Animation attackWindUp;
    
    protected float timeOfLastAttack;
    protected SpriteAnimator animator;
    
    protected virtual void Awake()
    {
        animator = GetComponent<SpriteAnimator>();
        timeOfLastAttack = Time.time;
    }
    
    public IEnumerator Attack(Vector3 target)
    {
        if (!(Time.time - timeOfLastAttack > attackCoolDown)) yield break;
        timeOfLastAttack = Time.time;
        if (attackWindUp != null && animator != null)
        {
            animator.PlayOnce(attackWindUp); 
        }
        yield return new WaitForSeconds(attackDelay);
        
        yield return PerformAttack(target);
    }
    
    protected abstract IEnumerator PerformAttack(Vector3 target);
}
