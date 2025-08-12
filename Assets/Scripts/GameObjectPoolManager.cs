using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameObjectPoolManager: MonoBehaviour
{
    public static GameObjectPoolManager I;
    
    private Dictionary<GameObject, GameObjectPool> pools = new();
    private Dictionary<GameObject, GameObject> objToPrefab = new();
    
    private void Awake()
    {
        if (I != null)
        {
            Destroy(I.gameObject);
        }
        I = this;
    }
    
    public GameObjectPool GetOrRegisterPool(GameObject prefab, Transform parent, 
        int initialCapacity = 25, int maxSize = 1000)
    {
        if (pools.TryGetValue(prefab, out var pool))
        {
            return pool;
        }
        
        pool = new GameObjectPool(prefab, parent, initialCapacity, maxSize);
        pools[prefab] = pool;
        return pool;
    }


    public void Register(GameObject obj, GameObject prefab)
    {
        objToPrefab[obj] = prefab;
    }

    public void Release(GameObject obj)
    {
        if (objToPrefab.TryGetValue(obj, out var prefab) && pools.TryGetValue(prefab, out var pool))
        {
            pool.Release(obj);
        }
        else
        {
            Debug.LogWarning("Trying to release an object that is not registered in any pool: " + obj.name + " objToPrefab: " + objToPrefab.GetValueOrDefault(obj, null));
            Destroy(obj);
        }
    }
    
    public void OnObjectDestroyed(GameObject obj)
    {
        objToPrefab.Remove(obj);
    }
}