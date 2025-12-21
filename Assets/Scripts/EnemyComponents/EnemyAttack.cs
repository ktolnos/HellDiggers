using System.Collections;
using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour
{
    public float attackCoolDown;
    
    protected float timeOfLastAttack;
    public bool isAttacking;
    
    protected virtual void Awake()
    {
        timeOfLastAttack = Time.time;
    }
    
    public IEnumerator Attack(Vector3 target)
    {
        if (!(Time.time - timeOfLastAttack > attackCoolDown) || isAttacking) yield break;
        isAttacking = true;
        yield return PerformAttack(target);
        isAttacking = false;
        timeOfLastAttack = Time.time;
    }
    
    protected abstract IEnumerator PerformAttack(Vector3 target);
}
