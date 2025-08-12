using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class BombAnimator: MonoBehaviour
{
    public Color[] circleColors;
    public Sprite fullSprite;
    [FormerlySerializedAs("light")] public Light2D explosionLight;

    public void Explode(float time)
    {
        StartCoroutine(ExplodeCoroutine(time));
    }
    
    private IEnumerator ExplodeCoroutine(float time)
    {
        var particleSys = GetComponent<ParticleSystem>();
        var spriteRenderer = GetComponent<SpriteRenderer>();

        var startTime = Time.time;

        var i = 0;
        while (Time.time < startTime + time)
        {
            spriteRenderer.color = circleColors[(i++)  % circleColors.Length];
            yield return new WaitForSeconds(0.1f);
        }
        particleSys.Play();
        spriteRenderer.sprite = fullSprite;
        spriteRenderer.color = Color.white;
        explosionLight.enabled = true;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.clear;
        yield return new WaitForSeconds(0.1f);
        explosionLight.enabled = false;
        Destroy(gameObject, 2f);
    }
    
}