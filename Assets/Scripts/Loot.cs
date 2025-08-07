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
    public AudioClip collectSound;
    
    private void Update()
    {
        var collectionDistance = (Player.I.stats.lootCollectionDistance + 0.5f) * 5f;
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
        var timeToPlayer = Mathf.Clamp01((Player.I.transform.position - startPosition).magnitude / 10f) * collectionTime;

        while (Time.time < startTime + timeToPlayer)
        {
            var t = (Time.time - startTime) / collectionTime;
            transform.position = DOVirtual.EasedValue(startPosition, Player.I.transform.position, t, Ease.InCubic);
            yield return null;
        }
        if (collectSound != null)
        {
            SoundManager.I.PlaySfx(collectSound, Player.I.transform.position);
        }
        Destroy(gameObject);
        GM.I.money += money;
        Player.I.health.Heal(hp);
        
    }
}
