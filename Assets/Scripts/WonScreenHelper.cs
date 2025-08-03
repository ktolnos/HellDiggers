using UnityEngine;

public class WonScreenHelper: MonoBehaviour
{
    public GameObject wonScreen;
    
    public void PlayMore()
    {
        Time.timeScale = 1f;
        wonScreen.gameObject.SetActive(false);
        Player.I.health.Damage(100000000000, DamageDealerType.Environment);
    }
        
}