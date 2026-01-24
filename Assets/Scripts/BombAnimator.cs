using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class BombAnimator: MonoBehaviour
{
    public Color[] circleColors;
    public Sprite fullSprite;
    [FormerlySerializedAs("light")] public Light2D explosionLight;
    public ParticleSystem windupParticles;
    public bool skipWindup = false;

    public void Explode(float time)
    {
        StartCoroutine(ExplodeCoroutine(time));
    }
    
    private IEnumerator ExplodeCoroutine(float time)
    {
        var particleSys = GetComponent<ParticleSystem>();
        var spriteRenderer = GetComponent<SpriteRenderer>();
        windupParticles.Play();

        var startTime = Time.time;

        var i = 0;
        while (Time.time < startTime + time)
        {
            spriteRenderer.color = circleColors[(i++)  % circleColors.Length];
            if (skipWindup)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        windupParticles.Stop();
        windupParticles.Clear();
        windupParticles.gameObject.SetActive(false);
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