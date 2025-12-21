using UnityEngine;

public class EnemyMovementBase : EnemyMovement
{
    protected Rigidbody2D rb;
    protected SpriteAnimator animator;
    public SpriteAnimator.Animation movementAnimation;
    public bool invertSprite;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<SpriteAnimator>();
    }

    public override void Move(Vector3 target, DigCallback digCallback)
    {
        animator.animation = movementAnimation;
        animator.autoplay = true;
        if (Mathf.Abs(target.x - transform.position.x) > 0.1f && Mathf.Abs(rb.linearVelocityX) > 0.01f)
        {
            animator.spriteRenderer.flipX = invertSprite ^ target.x < transform.position.x;
        }
    }

    public override void Stop()
    {
        animator.autoplay = false;
    }
}
