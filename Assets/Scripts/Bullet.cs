using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Bullet: MonoBehaviour
{
    public Rigidbody2D rb;

    private Vector2 velocity;
    public float explosionRadius = 0.3f;
    public bool explodeOnCollision = true;
    public float explosionDelay = 0f;
    public bool isGrenade = false;
    public bool isPlayerBullet = false;

    private void Awake()
    {
        TryGetComponent(out rb);
        if (explosionDelay != 0f)
        {
            StartCoroutine(DelayedExplode());
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (explodeOnCollision)
        {
            Explode();
        }
    }
    
    private void Explode() 
    {
        var explosionRadiusStat = isGrenade ? Player.I.stats.grenadeExplosionRadius * 2 : Player.I.stats.explosionRadius;
        var explosionDamage = isGrenade ? Player.I.stats.grenadeDamage : Player.I.stats.bulletDamage;
        if (isPlayerBullet)
        {
            Level.I.Explode(transform.position, explosionRadius + explosionRadiusStat, explosionDamage, DamageDealerType.Player);
        }
        else
        {
            Level.I.Explode(transform.position, explosionRadius + explosionRadiusStat, explosionDamage, DamageDealerType.Enemy);
        }
        Destroy(gameObject);
    }

    private IEnumerator DelayedExplode()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }
    
    
}