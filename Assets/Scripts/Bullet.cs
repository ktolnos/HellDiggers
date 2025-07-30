using System;
using UnityEngine;

public class Bullet: MonoBehaviour
{
    public Rigidbody2D rb;

    private Vector2 velocity;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject == Level.I.gameObject)
        {
            Level.I.Explode(transform.position, 0.3f);
            Destroy(gameObject);
            return;
        }
        Debug.Log(other.gameObject.name);
    }
}