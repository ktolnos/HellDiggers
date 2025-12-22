using UnityEngine;

public class EnemyMovementBase : EnemyMovement
{
    protected Rigidbody2D rb;
    protected SpriteAnimator animator;
    public SpriteAnimator.Animation movementAnimation;
    public bool invertSprite;
    private float lastDirectionFlipTime = -100f;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<SpriteAnimator>();
    }

    public override void Move(Vector3 target, DigCallback digCallback)
    {
        animator.animation = movementAnimation;
        animator.autoplay = true;
        if (Mathf.Abs(target.x - transform.position.x) > 0.1f && Mathf.Abs(rb.linearVelocityX) > 0.1f && Time.time - lastDirectionFlipTime > 0.5f)
        {
            currentFacingDirectionRight = target.x > transform.position.x;
            animator.spriteRenderer.flipX = invertSprite ^ !currentFacingDirectionRight;
            lastDirectionFlipTime = Time.time;
        }
    }

    public override void Stop()
    {
        animator.autoplay = false;
    }
}
