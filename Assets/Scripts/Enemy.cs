using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    public float hardness = 1f;
    public float speed;
    public float damage;
    public float groundDamage;
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
    public bool isBoss;
    public AudioClip beatSound;

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
        rb.simulated = animator.spriteRenderer.isVisible || player.transform.position.y < transform.position.y;
        if (!rb.simulated)
        {
            return;
        }
        if (isBoss)
        {
            if ((player.transform.position - transform.position).magnitude < attackDistance)
            {
                Stay();
                StartCoroutine(Attack(attackDelay, AttackType.Shoot));
            }
            if ((player.transform.position - transform.position).magnitude > stoppingDistance)
            {
                Follow();
            }
            else
            {
                StartCoroutine(Attack(attackDelay, AttackType.Beat));
            }
        }
        else
        {
            if (enemyLookTowardsPlayer)
            {
                Vector3 gunVector = player.transform.position - gun.transform.position;
                transform.localEulerAngles = Vector3.back * (Mathf.Atan2(gunVector.x, gunVector.y) * Mathf.Rad2Deg - 90f);
            }
    
            if ((player.transform.position - transform.position).magnitude < attackDistance)
            {
                StartCoroutine(Attack(attackDelay, attackType));
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
                rb.linearVelocityY = jumpForce;
            }
        }
        else
        { 
            rb.linearVelocityX = speed * Mathf.Sign(player.transform.position.x - transform.position.x);
            if (Mathf.Abs(player.transform.position.x - transform.position.x) < stoppingDistance)
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
                    StartCoroutine(Attack(attackDelay, attackType));
                }
            }
        }

        if (rb.linearVelocityX != 0)
        {
            animator.spriteRenderer.flipX = (rb.linearVelocityX < 0)^invertSprite;
        }
    }

    void Stay()
    {
        rb.linearVelocityX = 0;
        Vector3 gunVector = player.transform.position - gun.transform.position;
        animator.spriteRenderer.flipX = (gunVector.x <= 0)^invertSprite;
    }

    void Fly()
    {
        Vector3 movement = (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        rb.AddForce(movement, ForceMode2D.Impulse);
        animator.spriteRenderer.flipX = (movement.x < 0)^invertSprite;
    }

    public IEnumerator Attack(float _attackDelay, AttackType attackType)
    {
        if (!(Time.time - timeOfLastAttack > attackCoolDown)) yield break;
        timeOfLastAttack = Time.time;
        if (attackAnimation != null)
        {
            animator.PlayOnce(attackAnimation); 
        }
        yield return new WaitForSeconds(_attackDelay);
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
        Level.I.Explode(transform.position, attackRadius, damage, groundDamage, DamageDealerType.Enemy); 
        if (beatSound != null)
        {
            SoundManager.I.PlaySfx(beatSound, transform.position, 5f);
        }
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