using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager: MonoBehaviour
{
    public static SoundManager I;
    public float sfxBaseVolume = 0.01f;
    public float sfxVolume = 1f;
    private Dictionary<AudioClip, int> sfxCache = new();
    private int limitPerFrame = 10;

    private void Awake()
    {
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySfx(AudioClip clip, Vector3 position, float relativeValue = 1f)
    {
        sfxCache[clip] = sfxCache.GetValueOrDefault(clip, 0) + 1;
        if (sfxCache[clip] < limitPerFrame)
        {
            AudioSource.PlayClipAtPoint(clip, position, relativeValue * sfxVolume);
        }
    }

    private void LateUpdate()
    {
        foreach (var (audioClip, num) in sfxCache)
        {
            PlaySfxInternal(audioClip, Player.I.transform.position, sfxCache[audioClip] - limitPerFrame);
        }
        sfxCache.Clear();
    }

    private void PlaySfxInternal(AudioClip clip, Vector3 position, float relativeValue = 1f)
    {
        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp(relativeValue * sfxBaseVolume * sfxVolume, 0, sfxVolume));
    }
}