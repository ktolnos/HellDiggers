using System;
using System.Collections;
using UnityEngine;

public class SpriteAnimator: MonoBehaviour
{
    public new Animation animation;
    public bool autoplay = true;
    
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (animation.frames.Length == 0 || !autoplay)
        {
            return;
        }
        spriteRenderer.sprite = animation.frames[(int)(Time.time *  animation.fps) % animation.frames.Length];
    }
    
    [Serializable]
    public class Animation
    {
        public Sprite[] frames;
        public float fps = 10f;
        
        public Animation(Sprite[] frames, float fps)
        {
            this.frames = frames;
            this.fps = fps;
        }
    }

    public void PlayOnce()
    {
        StartCoroutine(PlayOnceCoroutine());
    }

    private IEnumerator PlayOnceCoroutine()
    {
        foreach (var animationFrame in animation.frames)
        {
            spriteRenderer.sprite = animationFrame;
            yield return new WaitForSeconds(1f / animation.fps);
        }
    }
}