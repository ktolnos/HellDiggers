using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    public float hardness = 1f;
    public float speed;
    public float damage;
    public float jumpForce;
    public float jumpReload;
    public float stoppingDistance;
    public float attackDistance;
    public float attackRadius = 2f;
    public float attackDelay = 0.7f;
    public BevaviorType bevaviorType;
    public AttackType attackType;
    public bool enemyLookTowardsPlayer = false;
    public float attackCoolDown;
    public Gun gun;
    public SpriteAnimator.Animation movementAnimation;
    public SpriteAnimator.Animation attackAnimation;
    public SpriteAnimator animator;
    public bool invertSprite = false;
    public bool aimGun = true;

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
        if (enemyLookTowardsPlayer)
        {
            Vector3 gunVector = player.transform.position - gun.transform.position;
            transform.localEulerAngles = Vector3.back * (Mathf.Atan2(gunVector.x, gunVector.y) * Mathf.Rad2Deg - 90f);
        }

        if ((player.transform.position - transform.position).magnitude < attackDistance)
        {
            StartCoroutine(Attack(attackDelay));
        }
        if ((player.transform.position - transform.position).magnitude > stoppingDistance)
        {
            if (bevaviorType == BevaviorType.Follow)
            {
                Follow();
            }
    
            if (bevaviorType == BevaviorType.Stay)
            {
                Stay();
            }
    
            if (bevaviorType == BevaviorType.Fly)
            {
                Fly();
            }
        }

        if (enemyLookTowardsPlayer)
        {
            animator.spriteRenderer.flipX = invertSprite;
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

    void Stay()
    {
        rb.linearVelocityX = 0;
        Vector3 gunVector = player.transform.position - gun.transform.position;
        animator.spriteRenderer.flipX = (gunVector.x < 0)^invertSprite;
    }

    void Fly()
    {
        Vector3 movement = (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        rb.AddForce(movement, ForceMode2D.Impulse);
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
        Level.DamageEntities(player.transform.position, attackRadius, damage, DamageDealerType.Enemy); 
    }

    void Shoot()
    {
        if (aimGun)
        {
            Vector3 gunVector = player.transform.position - gun.transform.position;
            gunSpriteRenderer.flipY = gunVector.x < 0f;
            gun.transform.localEulerAngles = Vector3.back * (Mathf.Atan2(gunVector.x, gunVector.y) * Mathf.Rad2Deg - 90f);
        }
        gun.Shoot();
    }
}

public enum BevaviorType
{
    Stay,
    Follow,
    Fly
}
public enum AttackType
{
    Shoot,
    Beat,
}