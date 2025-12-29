using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Bullet: MonoBehaviour
{
    public Rigidbody2D rb;

    private Vector2 velocity;
    public float enemyDamage;
    public float groundDamage;
    public float explosionRadius = 0.3f;
    public bool explodeOnCollision = true;
    public float explosionDelay = 0f;
    public bool isGrenade = false;
    public bool isPlayerBullet = false;
    public int ricochetCount = 0;
    public GameObject effect;
    public bool hasSound;
    public AudioClip sound;
    
    private void Awake()
    {
        TryGetComponent(out rb);
        if (explosionDelay != 0f)
        {
            StartCoroutine(DelayedExplode());
        }

        if (isPlayerBullet)
        {
            ricochetCount += Player.I.stats.bulletRicochetCount;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (explodeOnCollision)
        {
            var pos = transform.position;
            if (collision2D.contactCount > 0)
            {
                pos = collision2D.contacts[0].point;
            }
            Explode(pos, ricochetCount <= 0);
        }
        ricochetCount--;
    }
    
    private void Explode(Vector3 pos, bool destroy)
    {
        var playerStatsMult = isPlayerBullet ? 1f : 0f;
        var finalExplosionRadius = explosionRadius + 
                                    (isGrenade 
                                        ? Player.I.stats.grenadeExplosionRadius * playerStatsMult * 2f
                                        : Player.I.stats.bulletExplosionRadius * playerStatsMult * 1f);
        var finalEnemyDamage = enemyDamage * (1f + Player.I.stats.bulletEnemyDamage * playerStatsMult * 1f);
        if (!isPlayerBullet)
        {
            finalEnemyDamage *= 2f / (2f + Player.I.stats.bulletProtection);
        }
        var finalGroundDamage = groundDamage * (1f + Mathf.Pow(Player.I.stats.diggingDamage, 1.2f) * playerStatsMult * 1f);
        var damageType = isPlayerBullet ? DamageDealerType.Player : DamageDealerType.Enemy;
        Level.I.Explode(pos, finalExplosionRadius, finalEnemyDamage, finalGroundDamage, damageType);
        if (effect != null)
        {
            Destroy(Instantiate(effect, pos, Quaternion.identity, Level.I.spawnedObjectsParent), 2f);
        }
        if (destroy)
        {
            if (hasSound)
            {
                SoundManager.I.PlaySfx(sound, pos, 10f);
            }
            Destroy(gameObject);
        }
    }

    private IEnumerator DelayedExplode()
    {
        yield return new WaitForSeconds(explosionDelay + UnityEngine.Random.Range(-0.1f, 0.1f));
        Explode(transform.position, true);
    }
    
    
}