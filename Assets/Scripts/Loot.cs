using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public int money;
    public float hp;
    
    private float collectionTime = 0.5f;
    public Rigidbody2D rb;
    
    private void Update()
    {
        var collectionDistance = Player.I.stats.lootCollectionDistance * 3f;
        var distanceToPlayer = Vector2.Distance(transform.position, Player.I.transform.position);
        if (distanceToPlayer < collectionDistance)
        {
            StartCoroutine(Collect());
        }
    }

    private IEnumerator Collect()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            var colliders = new List<Collider2D>();
            rb.GetAttachedColliders(colliders);
            foreach (var coll in colliders)
            {
                coll.enabled = false;
            }
        }
        var startPosition = transform.position;
        var startTime = Time.time;

        while (Time.time < startTime + collectionTime)
        {
            var t = (Time.time - startTime) / collectionTime;
            transform.position = DOVirtual.EasedValue(startPosition, Player.I.transform.position, t, Ease.InCubic);
            yield return null;
        }
        Destroy(gameObject);
        Player.I.money += money;
        Player.I.health.Damage(-hp, isPlayerDamage:false);
    }
}
