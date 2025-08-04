using System;
using System.Collections;
using UnityEngine;

public class WonDeath: MonoBehaviour, IDeathHandler
{
    public void Die()
    {
        DeathAnimation();
    }
    
    private void DeathAnimation()
    {        
        WonScreenHelper.I.wonScreen.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }
}