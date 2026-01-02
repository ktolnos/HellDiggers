using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public int money;
    public GM.Resources resources;
    public float hp;
    public int ammo;
    
    private float collectionTime = 0.5f;
    public Rigidbody2D rb;
    public AudioClip collectSound;

    private Transform t;
    private Transform playerTransform;
    
    private void OnEnable()
    {
        if (rb != null)
        {
            rb.simulated = true;
        }
        StartCoroutine(DistanceCheck());
    }

    private IEnumerator DistanceCheck()
    {
        t = transform;
        playerTransform = Player.I.transform;
        var waitForSeconds = new WaitForSeconds(0.1f);
        while (true)
        {
            var collectionDistance = GetCollectionDistance();
            var distanceToPlayer = Vector2.Distance(t.position, playerTransform.position);
            if (distanceToPlayer < collectionDistance)
            {
                StartCoroutine(Collect());
                yield break;
            }

            yield return waitForSeconds;
        }
    }

    private IEnumerator Collect()
    {
        yield return null;
        if (rb != null)
        {
            rb.simulated = false;
        }
        var startPosition = t.position;
        var startTime = Time.time;
        var timeToPlayer = (Mathf.Clamp01((playerTransform.position - startPosition).magnitude / GetCollectionDistance()) + 0.2f) *
                           collectionTime;
        while (Time.time < startTime + timeToPlayer)
        {
            var lerp = (Time.time - startTime) / timeToPlayer;
            t.position = DOVirtual.EasedValue(startPosition, playerTransform.position, lerp, Ease.InCubic);
            yield return null;
        }
        if (collectSound != null)
        {
            SoundManager.I.PlaySfx(collectSound, playerTransform.position);
        }
        GameObjectPoolManager.I.Release(gameObject);
        GM.I.money += money;
        GM.I.resources += resources;
        Player.I.health.Heal(hp);
        Player.I.gun.AddAmmo(ammo);
    }

    private float GetCollectionDistance()
    {
        return Player.I.stats.lootCollectionDistance;
    }
}
