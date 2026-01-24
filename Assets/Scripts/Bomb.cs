using System;
using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour, IDeathHandler
{
    public BombAnimator explosionPrefab;
    public float explosionRadius;
    public float explosionGroundDamage;
    public float explosionEnemyDamage;
    public float explosionDelay;
    public AudioClip explosionSound;
    public AudioClip explosionBip;
    public float bipsPerSecond = 3f;
    public bool explodeOnEnable = true;
    private bool skipDelay = false;
    private BombAnimator explosionAnimator;
    private Health _health;
    public bool isPooled = true;

    private void OnEnable()
    {
        if (explodeOnEnable)
        {
            StartCoroutine(Explode(explosionDelay));
        }
        _health = GetComponent<Health>();
        _health.isInvulnerable = true;
    }

    public IEnumerator Explode(float delay)
    {
        yield return null;
        _health.isInvulnerable = false;
        var pos = transform.position;
        explosionAnimator = Instantiate(explosionPrefab, pos, Quaternion.identity, Level.I.spawnedObjectsParent);
        explosionAnimator.transform.localScale *= explosionRadius;
        explosionAnimator.Explode(delay);
        explosionAnimator.skipWindup = skipDelay;
        
        for (var i = 0; i < delay * bipsPerSecond; i++)
        {
            SoundManager.I.PlaySfx(explosionBip, pos, randomizePitch: false, pitch: 0.9f + 0.3f * (i / (delay * bipsPerSecond)));
            if (skipDelay)
                break;
            yield return new WaitForSeconds(1f / bipsPerSecond);
        }
        SoundManager.I.PlaySfx(explosionSound, pos, 20f);
        Level.I.Explode(pos, explosionRadius, explosionEnemyDamage, explosionGroundDamage,
            DamageDealerType.Environment, recoil:20f);
        if (isPooled)
        {
            GameObjectPoolManager.I.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        CameraController.I.Shake();
    }
    

    public void Die()
    {
        skipDelay = true;
        if (explosionAnimator != null)
        {
            explosionAnimator.skipWindup = true;
        }
    }

    private void OnDisable()
    {
        skipDelay = false;
        explosionAnimator = null;
    }
}