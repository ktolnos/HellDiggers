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

    private void Start()
    {
        if (upgradeUI.gameObject.activeSelf)
        {
            SetPlayerDead();
        }
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
        Player.I.transform.position = new Vector3(0, 0, 0);
        upgradeUI.gameObject.SetActive(false);
        playerLight.intensity = startIntensity;
        playerLight.pointLightOuterRadius = startRadius;
        globalLight.intensity = startGlobalIntensity;
        Level.I.PlayAgain();
        Player.I.Revive();
        Player.I.rb.bodyType = RigidbodyType2D.Dynamic;
        Player.I.rb.linearVelocity = Vector2.zero;
        startGold = Player.I.money;
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
        SetPlayerDead();
        isDying = false;
    }

    private void SetPlayerDead()
    {
        Player.I.rb.bodyType = RigidbodyType2D.Kinematic;
        playerLight.intensity = 0f;
        globalLight.intensity = 0f;
        Player.I.health.currentHealth = 0;
        upgradeUI.SetActive(true);
        Level.I.Clear();
        HighScoreManager.Instance.UpdateHighScore(Player.I.money - startGold);
    }
}