using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float damage;
    public float jumpForce;
    public float jumpReload;
    public float stoppingDistance;
    public float attackDelay = 0.7f;
    public BevaviorType bevaviorType;
    public AttackType attackType;
    public float attackCoolDown;
    public Gun gun;
    public SpriteAnimator.Animation movementAnimation;
    public SpriteAnimator.Animation attackAnimation;
    public SpriteAnimator animator;
    public bool invertSprite = false;
    private GameObject player;
    private float lastJumpTime;
    private Rigidbody2D rb;
    private Vector3Int direction;
    private float timeOfLastAttack;
    private SpriteRenderer gunSpriteRenderer;
    
    void Start()
    {
        player = Player.I.gameObject;
        lastJumpTime = Time.time;
        rb = GetComponent<Rigidbody2D>();
        direction = Vector3Int.right;
        timeOfLastAttack = Time.time;
        animator = GetComponent<SpriteAnimator>();
        animator.animation = movementAnimation;
        if (gun != null)
        {
            gunSpriteRenderer = gun.gameObject.GetComponentInChildren<SpriteRenderer>();
        }
    }

    void Update()
    {
        if ((player.transform.position - transform.position).magnitude < stoppingDistance)
        {
            StartCoroutine(Attack(attackDelay));
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
        animator.spriteRenderer.flipX = (rb.linearVelocity.x < 0)^invertSprite;
    }

    void Stray()
    {
        if (Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) + direction) && Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) - direction))
        {
            rb.linearVelocityX = 0;
        }
        else if (Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) + 2*direction) && Level.I.tilemap.HasTile(Level.I.tilemap.WorldToCell(transform.position) - direction))
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
        animator.spriteRenderer.flipX = (rb.linearVelocity.x < 0)^invertSprite;
    }

    void Fly()
    {
        Vector3 movement = (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        transform.position += movement;
        animator.spriteRenderer.flipX = (movement.x < 0)^invertSprite;
    }

    public IEnumerator Attack(float attackDelay)
    {
        if (Time.time - timeOfLastAttack > attackCoolDown)
        {
            timeOfLastAttack = Time.time;
            if (attackAnimation != null)
            {
               animator.PlayOnce(attackAnimation); 
            }
            yield return new WaitForSeconds(attackDelay);
            if (attackType == AttackType.Beat)
            {
                Beat();
            }

            if (attackType == AttackType.Shoot)
            {
                Shoot();
            }
        }
    }

    void Beat()
    {
        Level.DamageEntities(player.transform.position, 2f, damage, DamageDealerType.Enemy); 
    }

    void Shoot()
    {
        Vector3 gunVector = player.transform.position - gun.transform.position;
        gunSpriteRenderer.flipY = gunVector.x < 0f;
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