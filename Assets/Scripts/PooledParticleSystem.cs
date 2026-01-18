using System;
using UnityEngine;

public class PooledParticleSystem : MonoBehaviour
{
    private ParticleSystem particles;
    
    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();    
    }
    
    private void OnEnable()
    {
        particles.Play();
    }

    private void OnDisable()
    {
        particles.Stop();
        particles.Clear();
    }
}
