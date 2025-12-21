using System.Collections;
using UnityEngine;

public class EnemyMovementFly : EnemyMovement
{
    public float speed;
    public bool invertSprite = false;

    public override void Move(Vector3 target, DigCallback digCallback)
    {
        Vector3 movement = (target - transform.position).normalized * speed * Time.deltaTime;
        rb.AddForce(movement, ForceMode2D.Impulse);
    }
    
    public override void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
