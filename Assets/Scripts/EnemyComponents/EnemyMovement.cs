using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    // Common helpers if needed
    protected Rigidbody2D rb;
    protected SpriteAnimator animator;
    public SpriteAnimator.Animation movementAnimation;
    
    public delegate void DigCallback(Vector3 target);
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<SpriteAnimator>();
    }

    public virtual void Move(Vector3 target, DigCallback digCallback)
    {
        animator.animation = movementAnimation;
        animator.autoplay = true;
    }

    public virtual void Stop()
    {
        animator.autoplay = false;
    }
}
