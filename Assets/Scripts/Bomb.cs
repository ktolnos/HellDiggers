using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public BombAnimator explosionPrefab;
    public float explosionRadius;
    public float explosionGroundDamage;
    public float explosionEnemyDamage;
    public float explosionDelay;
    public AudioClip explosionSound;
    public AudioClip explosionBip;
    public float bipsPerSecond = 3f;

    private void OnEnable()
    {
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        yield return null;
        var pos = transform.position;
        var bombExplosion = Instantiate(explosionPrefab, pos, Quaternion.identity, Level.I.spawnedObjectsParent);
        bombExplosion.transform.localScale *= explosionRadius;
        bombExplosion.Explode(explosionDelay);
        
        for (var i = 0; i < explosionDelay * bipsPerSecond; i++)
        {
            SoundManager.I.PlaySfx(explosionBip, pos, randomizePitch: false, pitch: 0.9f + 0.3f * (i / (explosionDelay * bipsPerSecond)));
            yield return new WaitForSeconds(1f / bipsPerSecond);
        }
        SoundManager.I.PlaySfx(explosionSound, pos, 20f);
        Level.I.Explode(pos, explosionRadius, explosionEnemyDamage, explosionGroundDamage,
            DamageDealerType.Environment, recoil:20f);
        GameObjectPoolManager.I.Release(gameObject);
        CameraController.I.Shake();
    }
}