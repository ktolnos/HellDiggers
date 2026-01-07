using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    public float hardness = 1f;
    public float stoppingDistance;
    public float attackDistance;
    public SpriteAnimator animator;
    private GameObject player;
    private Rigidbody2D rb;
    public EnemyMovement defaultMovement;
    public EnemyMovement standbyMovement;
    public EnemyAttack defaultAttack;
    
    public bool stopWhenAttacking = true;
    public bool isAgro = false;
    public float agroDistance = 5f;
    public bool agroOnHurt = true;
    
    public EnemyMovement enemyMovement;
    private EnemyAttack enemyAttack;
    public bool disableOffScreen = true;

    void Start()
    {
        player = Player.I.gameObject;
        rb = GetComponent<Rigidbody2D>();
        
        if (defaultAttack == null)
        {
            defaultAttack = GetComponent<EnemyAttack>();
        }
        if (defaultMovement == null)
        {
            defaultMovement = GetComponent<EnemyMovement>();
        }

        enemyMovement = defaultMovement;
        enemyAttack = defaultAttack;
        if (agroOnHurt && TryGetComponent(out Health enemyHealth))
        {
            enemyHealth.onDamageTaken += (damage, type) =>
            {
                if (type == DamageDealerType.Player) StartCoroutine(Agro());
            };
        }
    }

    private IEnumerator Agro()
    {
        yield return new WaitForSeconds(0.3f);
        isAgro = true;
    }

    void FixedUpdate()
    {
        rb.simulated = !disableOffScreen || CameraController.IsObjectVisible(animator.spriteRenderer) ||
                       player.transform.position.y < transform.position.y;
        if (!rb.simulated)
        {
            return;
        }
        
        enemyMovement = isAgro ? defaultMovement : standbyMovement ?? defaultMovement;
        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
        isAgro = isAgro || distToPlayer < agroDistance;
        if (distToPlayer > stoppingDistance && !(stopWhenAttacking && enemyAttack.isAttacking))
        {
            enemyMovement.Move(player.transform.position, target => StartCoroutine(enemyAttack.Attack(target)));
        }
        else
        {
            enemyMovement.Stop();
        }

        if (distToPlayer < attackDistance && isAgro)
        {
            StartCoroutine(enemyAttack.Attack(player.transform.position));
        }
    }
}