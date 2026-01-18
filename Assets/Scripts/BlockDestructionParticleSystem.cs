using System;
using UnityEngine;

public class BlockDestructionParticleSystem : MonoBehaviour
{
    private ParticleSystem particles;
    public Color[] colors;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        var main = particles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(
            min: colors[Level.I.currentCircleIndex],
            max: Color.black);
    }
}