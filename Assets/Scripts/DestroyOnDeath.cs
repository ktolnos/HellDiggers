using UnityEngine;

public class DestroyOnDeath : MonoBehaviour, IDeathHandler
{

    public void Die()
    {
        Destroy(gameObject);
    }
}
