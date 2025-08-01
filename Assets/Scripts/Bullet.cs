﻿using System;
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (explodeOnCollision)
        {
            Explode(ricochetCount <= 0);
        }
        ricochetCount--;
    }
    
    private void Explode(bool destroy)
    {
        var playerStatsMult = isPlayerBullet ? 1f : 0f;
        var finalExplosionRadius = explosionRadius + 
                                    (isGrenade 
                                        ? Player.I.stats.grenadeExplosionRadius * playerStatsMult * 2f
                                        : Player.I.stats.bulletExplosionRadius * playerStatsMult * 1f);
        var finalEnemyDamage = enemyDamage + Player.I.stats.bulletEnemyDamage * playerStatsMult * 1f;
        var finalGroundDamage = groundDamage + Player.I.stats.diggingDamage * playerStatsMult * 1f;
        var damageType = isPlayerBullet ? DamageDealerType.Player : DamageDealerType.Enemy;
        Level.I.Explode(transform.position, finalExplosionRadius, finalEnemyDamage, finalGroundDamage, damageType);
        if (destroy)
        {
            Destroy(gameObject);
            if (effect != null)
            {
                Destroy(Instantiate(effect, transform.position, Quaternion.identity, Level.I.spawnedObjectsParent), 2f);
            }
        }
    }

    private IEnumerator DelayedExplode()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode(true);
    }
    
    
}