using System.Collections;
using UnityEngine;

public class BombAnimator: MonoBehaviour
{
    public Color[] circleColors;
    public Sprite fullSprite;

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
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.clear;
        Destroy(gameObject, 2f);
    }
    
}