using UnityEngine;

public class EnemyMovementGround : EnemyMovement
{
    public float speed;
    public float jumpForce;
    public float jumpReload;
    public float stoppingDistance;

    private float lastJumpTime;

    protected override void Awake()
    {
        base.Awake();
        lastJumpTime = Time.time;
    }

    public override void Move(Vector3 targetPos, DigCallback digCallback)
    {
        base.Move(targetPos, digCallback);
        if (targetPos.y > transform.position.y)
        {
            if (Time.time - lastJumpTime > jumpReload)
            {
                lastJumpTime = Time.time;
                rb.linearVelocityY = jumpForce;
            }
        }
        else
        { 
            rb.linearVelocityX = speed * Mathf.Sign(targetPos.x - transform.position.x);
            if (Mathf.Abs(targetPos.x - transform.position.x) < stoppingDistance)
            {
                rb.linearVelocityX = 0;
            }
            else
            {
                var tilePos = Level.I.grid.WorldToCell(transform.position);
                var dir = rb.linearVelocityX < 0 ? Vector3Int.left : Vector3Int.right;
                if (Level.I.HasTile(tilePos + dir))
                {
                    rb.linearVelocityX = 0;
                    digCallback?.Invoke(Level.I.grid.GetCellCenterWorld(tilePos + dir));
                }
            }
        }
    }

    public override void Stop()
    {
        base.Stop();
        rb.linearVelocityX = 0;
    }
}
