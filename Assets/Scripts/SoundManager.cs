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
    public Soundtrack[] soundtracks;
    
    private AudioSource soundtrackSource;
    private int currentSoundtrackIndex = 0;

    public float masterVolume = 1f;
    public float musicVolume = 1f;

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
        soundtrackSource = gameObject.GetComponent<AudioSource>();
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
        if (!soundtrackSource.isPlaying)
        {
            currentSoundtrackIndex = (currentSoundtrackIndex + 1) % soundtracks.Length;
            soundtrackSource.clip = soundtracks[currentSoundtrackIndex].clip;
            soundtrackSource.Play();
        }
        soundtrackSource.volume = Mathf.Clamp(musicVolume * masterVolume, 0, 1);
    }

    private void PlaySfxInternal(AudioClip clip, Vector3 position, float relativeValue = 1f)
    {
        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp(relativeValue * sfxBaseVolume * sfxVolume * masterVolume, 0, sfxVolume * masterVolume));
    }
    
    [Serializable]
    public class Soundtrack
    {
        public AudioClip clip;
    }
}