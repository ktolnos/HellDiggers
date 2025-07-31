using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float damage;
    public float jumpForce;
    public float jumpReload;
    public float stoppingDistance;
    public BevaviorType bevaviorType;
    public AttackType attackType;
    public float attackCoolDown;
    public Gun gun;
    private GameObject player;
    private float lastJumpTime;
    private Rigidbody2D rb;
    private Vector3Int direction;
    private float timeOfLastAttack;
    
    void Start()
    {
        player = Player.I.gameObject;
        lastJumpTime = Time.time;
        rb = GetComponent<Rigidbody2D>();
        direction = Vector3Int.right;
        timeOfLastAttack = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if ((player.transform.position - transform.position).magnitude < stoppingDistance)
        {
            Attack();
        }
        else
        {
            if (bevaviorType == BevaviorType.Follow)
            {
                Follow();
            }
    
            if (bevaviorType == BevaviorType.Stray)
            {
                Stray();
            }
    
            if (bevaviorType == BevaviorType.Fly)
            {
                Fly();
            }
        }
        
    }

    void Follow()
    {
        if (player.transform.position.y > transform.position.y)
        {
            if (Time.time - lastJumpTime > jumpReload)
            {
                lastJumpTime = Time.time;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
        else
        {
            if (player.transform.position.x > transform.position.x + stoppingDistance)
            {
                rb.linearVelocityX = speed;
            }
            else
            {
                if (player.transform.position.x < transform.position.x - stoppingDistance)
                {
                    rb.linearVelocityX = -speed;
                }
            }  
        }
    }

    void Stray()
    {
        if (Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) + direction) && Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) - direction))
        {
            rb.linearVelocityX = 0;
        }
        else if (!Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) - 2*direction + Vector3Int.down) && !Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) + direction + Vector3Int.down))
        {
            rb.linearVelocityX = 0;
        }
        else if (!Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) + direction) && Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) + direction + Vector3Int.down))
        {
            rb.linearVelocityX = direction.x*speed;
        }
        else
        {
            direction.x = -direction.x;
        }
    }

    void Fly()
    {
        transform.position += (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
    }

    void Attack()
    {
        if (attackType == AttackType.Beat)
        {
            Beat();
        }

        if (attackType == AttackType.Shoot)
        {
            Shoot();
        }
    }

    void Beat()
    {
        if (Time.time - timeOfLastAttack > attackCoolDown)
        {
            Level.I.damageEntities(player.transform.position, 2f, damage, DamageDealerType.Enemy); 
            timeOfLastAttack = Time.time;
        }
    }

    void Shoot()
    {
        Vector3 gunVector = player.transform.position - gun.transform.position;
        gun.transform.localEulerAngles = Vector3.back * (Mathf.Atan2(gunVector.x, gunVector.y) * Mathf.Rad2Deg - 90f);
        gun.Shoot();
    }
}

public enum BevaviorType
{
    Stray,
    Follow,
    Fly
}
public enum AttackType
{
    Shoot,
    Beat,
}