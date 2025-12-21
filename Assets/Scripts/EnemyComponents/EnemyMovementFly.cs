using System.Collections;
using UnityEngine;

public class EnemyMovementFly : EnemyMovementBase
{
    public float speed;

    private float movementAccumulator;
    private float timeSinceLastCheck;
    private Vector3 lastPosition;
    private float checkInterval = 0.5f;
    private float movementThreshold = 0.1f;

    protected override void Awake()
    {
        base.Awake();
        lastPosition = transform.position;
    }

    public override void Move(Vector3 target, DigCallback digCallback)
    {
        base.Move(target, digCallback);
        
        float dist = Vector3.Distance(transform.position, lastPosition);
        movementAccumulator += dist;
        lastPosition = transform.position;
        
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkInterval)
        {
            if (movementAccumulator < movementThreshold)
            {
                Vector3 dir = (target - transform.position).normalized;
                digCallback?.Invoke(transform.position + dir);
            }
            movementAccumulator = 0f;
            timeSinceLastCheck = 0f;
        }

        Vector3 movement = (target - transform.position).normalized * speed * Time.deltaTime;
        rb.AddForce(movement, ForceMode2D.Impulse);
    }
    
    public override void Stop()
    {
        rb.linearVelocity = Vector2.zero;
        movementAccumulator = 0f;
        timeSinceLastCheck = 0f;
        lastPosition = transform.position;
    }
}
