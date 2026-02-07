using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerDeath: MonoBehaviour, IDeathHandler
{
    public Light2D playerLight; 
    public Light2D globalLight; 
    
    private InputAction restartAction;
    
    private float startIntensity;
    private float startRadius;
    private float startGlobalIntensity;
    private bool isDying = false;

    private int startGold = 0;
    
    private void Awake()
    {
        restartAction = InputSystem.actions.FindAction("Restart");
        
        startIntensity = playerLight.intensity;
        startRadius = playerLight.pointLightOuterRadius;
        startGlobalIntensity = globalLight.intensity;
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
        if (!isDying)
        {
            StartCoroutine(DeathAnimation());
        }
    }

    public void PlayAgain()
    {
        Player.I.transform.position = new Vector3(0, 2, 0);
        UpgradesController.I.HideUpgrades();
        playerLight.intensity = startIntensity;
        playerLight.pointLightOuterRadius = startRadius;
        globalLight.intensity = startGlobalIntensity;
        Level.I.PlayAgain();
        Player.I.Revive();
        Player.I.rb.bodyType = RigidbodyType2D.Dynamic;
        Player.I.rb.linearVelocity = Vector2.zero;
        startGold = GM.I.money;
        Player.I.collectedKeys.Clear();
    }

    private IEnumerator DeathAnimation()
    {
        var startTime = Time.time;
        var animationDuration = 0.5f;
        while (Time.time < startTime + animationDuration)
        {
            var t = (Time.time - startTime) / animationDuration;
            playerLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            playerLight.pointLightOuterRadius = Mathf.Lerp(startRadius, 0f, t);
            globalLight.intensity = Mathf.Lerp(startGlobalIntensity, 0f, t);
            yield return null;
        }
        OnPlayerDead();
        isDying = false;
        SaveManager.I.SaveGame();
    }

    private void OnPlayerDead()
    {
        HighScoreManager.I.UpdateHighScore(GM.I.GetGainedMoney());
        Level.I.Clear();
        PlayAgain();
        HighScoreManager.I.OpenHighScorePanel();
    }
}