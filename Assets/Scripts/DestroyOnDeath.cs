using UnityEngine;

public class DestroyOnDeath : MonoBehaviour, IDeathHandler
{
    public GameObject loot;
    public GameObject effect;
    public void Die()
    {
        if (effect != null)
        {
            Destroy(Instantiate(effect, transform.position, Quaternion.identity, Level.I.spawnedObjectsParent), 1f);
        }

        if (loot != null)
        {
            GameObjectPoolManager.I.GetOrRegisterPool(loot,  Level.I.pooledObjectsParent).InstantiateTemporarily(
                transform.position, Quaternion.identity, 10f);
        }
        Destroy(gameObject);
    }
}
