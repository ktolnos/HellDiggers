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
    public bool rotateAtTarget;
    private float randomOffsetAngle;

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
        randomOffsetAngle  = Random.Range(-90, 90);
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
        Move();
        
        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distToPlayer < attackDistance && isAgro)
        {
            StartCoroutine(enemyAttack.Attack(player.transform.position));
        }
    }

    private void Move()
    {
        var targetPos = player.transform.position;
        float distToPlayer = Vector2.Distance(transform.position, targetPos);
        enemyMovement = isAgro ? defaultMovement : standbyMovement ?? defaultMovement;
        isAgro = isAgro || distToPlayer < agroDistance;
        if (rotateAtTarget)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Vector3.forward,
                targetPos - transform.position) * Quaternion.Euler(0,0,90f), 360f*Time.fixedDeltaTime);
        }
        if (distToPlayer < stoppingDistance && !Level.I.HasTile(transform.position))
        {
            return;
        }
        targetPos += Quaternion.Euler(0,0,randomOffsetAngle) * (targetPos - transform.position).normalized * stoppingDistance;
        
        if (stopWhenAttacking && enemyAttack.isAttacking)
        {
            enemyMovement.Stop();
            return;
        }
        enemyMovement.Move(targetPos, target => StartCoroutine(enemyAttack.Attack(target)));
    }
}