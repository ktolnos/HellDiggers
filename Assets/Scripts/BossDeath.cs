using System.Collections;
using UnityEngine;

public class BossDeath: MonoBehaviour, IDeathHandler
{
    public void Die()
    {
        DeathAnimation();
    }
    
    private async void DeathAnimation()
    {
        await Awaitable.WaitForSecondsAsync(1f);
        Level.I.RemoveBossFloor();
    }
        
}