using UnityEngine;

public class EnemyMovementGround : EnemyMovementBase
{
    public float speed;
    public float jumpForce;
    public float jumpReload;
    public Collider2D mainCollider;
    public int maxJumps = 2;
    private int jumpCount;

    private float lastJumpTime;
    private float lastHorizontalMovementTime;
    private float gravityScale;

    public bool allowDiggingWithAttack = true;

    protected override void Awake()
    {
        base.Awake();
        lastJumpTime = Time.time;
        if (mainCollider == null)
        {
            mainCollider = GetComponent<Collider2D>();
        }
        gravityScale = rb.gravityScale;
    }

    public override void Move(Vector3 targetPos, DigCallback digCallback)
    {
        rb.gravityScale = gravityScale;
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (Vector2.Distance(targetPos, transform.position) <= 0.1f)
        {
            rb.linearVelocityX = 0;
            return;
        }
        if (targetPos.y > transform.position.y + 0.25f)
        {
            if (Time.time - lastJumpTime > jumpReload && jumpCount < maxJumps)
            {
                lastJumpTime = Time.time;
                rb.linearVelocityY = jumpForce;
                jumpCount++;
                return;
            }
        }
        var targetVelocityX = speed * Mathf.Sign(targetPos.x - transform.position.x);
        rb.linearVelocityX = Mathf.Lerp(rb.linearVelocityX, targetVelocityX, speed * Time.deltaTime);
        
        var bounds = mainCollider.bounds;
        var bottomLeft = new Vector2(bounds.min.x, bounds.min.y + 0.1f);
        var bottomRight = new Vector2(bounds.max.x, bounds.min.y + 0.1f);
        var topLeft = new Vector2(bounds.min.x, bounds.max.y);
        var topRight = new Vector2(bounds.max.x, bounds.max.y);

        var topRightTile = Level.I.GetTileInfo(topRight + Vector2.right * 0.2f);
        var bottomRightTile = Level.I.GetTileInfo(bottomRight + Vector2.right * 0.2f);
        var topLeftTile = Level.I.GetTileInfo(topLeft + Vector2.left * 0.2f);
        var bottomLeftTile = Level.I.GetTileInfo(bottomLeft + Vector2.left * 0.2f);
        var leftBottomTile = Level.I.GetTileInfo(bottomLeft + Vector2.down * 0.2f);
        var rightBottomTile = Level.I.GetTileInfo(bottomRight + Vector2.down * 0.2f);
        
        var rightHit = bottomRightTile != null || topRightTile != null;
        var leftHit = bottomLeftTile != null || topLeftTile != null;
        
        var isGrounded = leftBottomTile != null || rightBottomTile != null;
        if (isGrounded)
        {
            jumpCount = 0;
        }

        if (rightHit && rb.linearVelocityX > 0 ||
            leftHit && rb.linearVelocityX < 0)
        {
            if (allowDiggingWithAttack && Time.time - lastHorizontalMovementTime > 0.5f)
            {
                digCallback?.Invoke(targetPos);
            }
            rb.linearVelocityX = 0;
        }
        if (Mathf.Abs(rb.linearVelocityX) > 0.001f)
        {
            lastHorizontalMovementTime = Time.time;
        }
        base.Move(targetPos, digCallback);
    }

    public override void Stop()
    {
        base.Stop();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        jumpCount = 0;
    }
}
