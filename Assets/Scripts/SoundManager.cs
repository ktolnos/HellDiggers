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
    
    private AudioSource sfxSource;

    private void Awake()
    {
        I = this;
        soundtrackSource = gameObject.GetComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySfx(AudioClip clip, Vector3 position, float relativeValue = 1f, bool randomizePitch = true, float pitch = 1f)
    {
        sfxCache[clip] = sfxCache.GetValueOrDefault(clip, 0) + 1;
        if (sfxCache[clip] < limitPerFrame)
        {
            pitch = randomizePitch ? UnityEngine.Random.Range(0.8f, 1.2f) * pitch : pitch;
            PlayClipAtPoint(clip, position, relativeValue * sfxVolume, pitch);
        }
    }
    
    public void PlaySfx(AudioClip clip, float relativeValue = 1f, bool randomizePitch = true)
    {
        sfxSource.pitch = randomizePitch ? UnityEngine.Random.Range(0.8f, 1.2f) : 1f;
        sfxSource.volume = Mathf.Clamp(relativeValue * sfxVolume * masterVolume, 0, 1);
        sfxSource.priority = 256;
        sfxSource.PlayOneShot(clip);
    }

    private void LateUpdate()
    {
        sfxCache.Clear();
        if (!soundtrackSource.isPlaying)
        {
            currentSoundtrackIndex = (currentSoundtrackIndex + 1) % soundtracks.Length;
            soundtrackSource.clip = soundtracks[currentSoundtrackIndex].clip;
            soundtrackSource.Play();
        }
        soundtrackSource.volume = Mathf.Clamp(musicVolume * masterVolume, 0, 1);
    }

    [Serializable]
    public class Soundtrack
    {
        public AudioClip clip;
    }
    
    private static void PlayClipAtPoint(AudioClip clip, Vector3 position, 
        [UnityEngine.Internal.DefaultValue("1.0F")] float volume, float pitch = 1f)
    {
        GameObject gameObject = new GameObject("One shot audio");
        gameObject.transform.position = position;
        AudioSource audioSource = (AudioSource) gameObject.AddComponent(typeof (AudioSource));
        audioSource.clip = clip;
        audioSource.spatialBlend = 1f;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
        Destroy(gameObject, clip.length * ((double) Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale));
    }
}