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
        Level.I.Explode(transform.position, explosionRadius + explosionRadiusStat);
        Destroy(gameObject);
    }

    private IEnumerator DelayedExplode()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }
}