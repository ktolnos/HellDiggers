using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Bullet: MonoBehaviour
{
    private static Vector2 IMPOSSIBLE_POSITION = new(-10000f, -10000f);

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
    public float maxPenetrationDepth;
    private Vector2 penetrationStartPos = IMPOSSIBLE_POSITION;
    private HashSet<Health> damagedEntities = null;
    private HashSet<Vector3Int> damagedTiles = null;
    public SpriteRenderer spriteRenderer;

    // New flag: when true, do not apply player stat scaling to this bullet (acts like enemy bullet)
    public bool ignoreStatsScaling = false;
    
    private void Awake()
    {
        TryGetComponent(out rb);
        if (explosionDelay != 0f)
        {
            StartCoroutine(DelayedExplode());
        }

        if (isPlayerBullet && !ignoreStatsScaling)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (penetrationStartPos != IMPOSSIBLE_POSITION)
        {
            return;
        }
        penetrationStartPos = rb.position;
        StartCoroutine(PenetrationDestruction());
    }
    
    private float GetExplosionRadius()
    {
        var applyStats = isPlayerBullet && !ignoreStatsScaling;
        return explosionRadius + 
               (isGrenade 
                   ? (applyStats ? Player.I.stats.grenadeExplosionRadius : 0f)
                   : (applyStats ? Player.I.stats.bulletExplosionRadius : 0f));
    }
    
    private float GetEnemyDamage()
    {
        var applyStats = isPlayerBullet && !ignoreStatsScaling;
        var finalEnemyDamage = enemyDamage * (applyStats ? (isGrenade ? Player.I.stats.grenadeDamage : Player.I.stats.bulletEnemyDamage) : 1f);
        if (!isPlayerBullet)
        {
            finalEnemyDamage *= 1f - Player.I.stats.bulletProtection * 0.01f;
        }
        return finalEnemyDamage;
    }
    
    private float GetGroundDamage()
    {
        var applyStats = isPlayerBullet && !ignoreStatsScaling;
        return groundDamage * (applyStats ? (isGrenade ? Player.I.stats.grenadeDamage : Player.I.stats.diggingDamage) : 1f);
    }


    private void Explode(Vector3 pos, bool destroy)
    {
        var finalExplosionRadius = GetExplosionRadius();
        var finalEnemyDamage = GetEnemyDamage();
        var finalGroundDamage = GetGroundDamage();
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

    private IEnumerator PenetrationDestruction()
    {
        var prevPos = rb.position;
        while (true)
        {
            var penetrationDepth = Vector2.Distance(rb.position, penetrationStartPos);
            if (penetrationDepth >= maxPenetrationDepth)
            {
                Explode(transform.position, destroy:true);
            }
            
            var damageType = isPlayerBullet ? DamageDealerType.Player : DamageDealerType.Enemy;
            var newDamagedEntities = GM.DamageEntitiesCapsule(prevPos, rb.position, GetExplosionRadius(), 
                GetEnemyDamage(), damageType, damagedEntities);
            if (damagedEntities == null)
            {
                damagedEntities = newDamagedEntities;
            }
            else
            {
                damagedEntities.UnionWith(newDamagedEntities);
            }

            if (damagedTiles == null)
            {
                damagedTiles = new HashSet<Vector3Int>();
            }
            Level.I.DamageTilesCapsule(prevPos, rb.position, GetExplosionRadius(), 
                GetGroundDamage(), damagedTiles, ignoreTiles:damagedTiles);
            yield return null;
            prevPos = rb.position;
        }
    }
    
    
}

