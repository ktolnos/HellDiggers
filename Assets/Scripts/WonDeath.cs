using System.Collections;
using UnityEngine;

public class WonDeath: MonoBehaviour, IDeathHandler
{
    public GameObject wonScreen;
    
    public void Die()
    {
        DeathAnimation();
    }
    
    private void DeathAnimation()
    {
        Time.timeScale = 0f;
        wonScreen.gameObject.SetActive(true);
    }
        
}