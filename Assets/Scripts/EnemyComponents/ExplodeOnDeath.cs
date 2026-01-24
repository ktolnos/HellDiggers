using UnityEngine;

public class ExplodeOnDeath: MonoBehaviour, IDeathHandler
{
    public Bomb bomb;
    
    public void Die()
    {
        if (bomb != null)
        {
            bomb.Explode(0f);
        }
    }
}