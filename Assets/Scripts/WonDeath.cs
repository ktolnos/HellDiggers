using System;
using System.Collections;
using UnityEngine;

public class WonDeath: MonoBehaviour, IDeathHandler
{
    private WonScreenHelper wonScreen;
    private void Awake()
    {
        wonScreen = GameObject.Find("WonHelper").GetComponent<WonScreenHelper>();
    }
    public void Die()
    {
        DeathAnimation();
    }
    
    private void DeathAnimation()
    {        
        wonScreen.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }
}