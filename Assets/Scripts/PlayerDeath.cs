using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerDeath: MonoBehaviour, IDeathHandler
{
    public Light2D playerLight; 
    public Light2D globalLight; 
    public GameObject upgradeUI;
    
    private InputAction restartAction;

    private void Awake()
    {
        restartAction = InputSystem.actions.FindAction("Restart");
    }

    private void Update()
    {
        if (restartAction.WasPerformedThisFrame())
        {
            // For testing purposes, trigger death animation with R key
            Die();
        }
    }

    public void Die()
    {
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        var startTime = Time.time;
        var startIntensity = playerLight.intensity;
        var startRadius = playerLight.pointLightOuterRadius;
        var startGlobalIntensity = globalLight.intensity;
        var animationDuration = 0.5f;
        while (Time.time < startTime + animationDuration)
        {
            var t = (Time.time - startTime) / animationDuration;
            playerLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            playerLight.pointLightOuterRadius = Mathf.Lerp(startRadius, 0f, t);
            globalLight.intensity = Mathf.Lerp(startGlobalIntensity, 0f, t);
            yield return null;
        }
        playerLight.intensity = 0f;
        globalLight.intensity = 0f;
        upgradeUI.SetActive(true);
    }
}