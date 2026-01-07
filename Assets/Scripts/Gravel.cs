using System;
using System.Collections;
using UnityEngine;

public class Gravel : MonoBehaviour
{
    public float fallDamage;
    public bool destroyOnContact;
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Level.TileInfo tileInfo;
    private float fallStart;
    
    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out coll);
    }

    private void OnEnable()
    {
        tileInfo = Level.I.GetTileInfo(transform.position);
        if (tileInfo == null)
        {
            Debug.LogError("No tile info found for gravel at position: " + transform.position);
            StartCoroutine(ReleaseNextFrame());
            return;
        }
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(transform.position);
        rb.bodyType = RigidbodyType2D.Kinematic;
        fallStart = Time.time;
        StartCoroutine(CheckGroundOnStart());
    }

    private void FixedUpdate()
    {
        if (rb.bodyType != RigidbodyType2D.Dynamic || !rb.simulated)
        {
            return;
        }
        var isStanding = IsStanding();
        // rb.bodyType = isStanding ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
        if (isStanding)
        {
            tileInfo.hp = tileInfo.tileData.maxHp;
            var pos = Level.I.WorldToCell(transform.position);
            if (!IsValidPos(pos))
            {
                Debug.LogWarning("Gravel is trying to set a tile at an invalid position: " + pos);
                GameObjectPoolManager.I.Release(gameObject);
                return;
            }
            tileInfo.pos = pos;
            Level.I.SetTile(tileInfo);
            GameObjectPoolManager.I.Release(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (IsStanding())
        {
            return;
        }

        if (other.transform.position.y - transform.position.y > 0f)
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Loot"))
        {
            GameObjectPoolManager.I.Release(other.gameObject);
            return;
        }

        var health = other.gameObject.GetComponentInParent<Health>();
        if (!health)
        {
            return;
        }

        var dmgMult = 1f;
        if (health.isPlayer)
        {
            dmgMult *= 1f - Player.I.stats.headProtection * 0.01f;
        }
        health.Damage(fallDamage * dmgMult, DamageDealerType.Environment);
        if (destroyOnContact)
        {
            GameObjectPoolManager.I.Release(gameObject);
        }
    }

    private bool IsStanding()
    {
        var bottomTilePos = new Vector3(coll.bounds.center.x, coll.bounds.min.y - 0.1f);
        return Level.I.HasTile(bottomTilePos) ||
               (Time.time - fallStart) > 0.5f && Mathf.Abs(rb.linearVelocityY) < 0.001f;
    }

    private bool IsValidPos(Vector3Int pos)
    {
        return !Level.I.HasTile(pos);
    } 

    private IEnumerator CheckGroundOnStart()
    {
        yield return null;
        if (IsStanding())
        {
            GameObjectPoolManager.I.Release(gameObject);
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
    
    private IEnumerator ReleaseNextFrame()
    {
        yield return null;
        GameObjectPoolManager.I.Release(gameObject);
    }

    private void OnDisable()
    {
        rb.simulated = false;
    }
}
