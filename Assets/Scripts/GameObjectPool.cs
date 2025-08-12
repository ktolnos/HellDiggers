using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameObjectPool
{
    private GameObject prefab;
    private Transform parent;
    private ObjectPool<GameObject> pool;
    private HashSet<GameObject> pendingRelease = new();
    
    public GameObjectPool(GameObject prefab, Transform parent,
                          int initialCapacity, int maxSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new ObjectPool<GameObject>(
            CreatePooledObject,
            OnGet,
            OnRelease,
            OnDestroy,
            true,
            initialCapacity,
            maxSize
        );
    }
    
    public GameObject Instantiate(Vector3 position, Quaternion rotation)
    {
        var obj = pool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }
    
    public GameObject InstantiateTemporarily(Vector3 position = default, Quaternion rotation = default, float time = 0f)
    {
        var obj = Instantiate(position, rotation);
        Release(obj, time);
        return obj;
    }
    
    public async void Release(GameObject obj, float time=0f)
    {
        if (time != 0f)
        {
            pendingRelease.Add(obj);
            await Awaitable.WaitForSecondsAsync(time);
            if (!pendingRelease.Remove(obj))
            {
                return;
            }
        }
        else
        {
            pendingRelease.Remove(obj);
        }
        pool.Release(obj);
    }
    
    private GameObject CreatePooledObject()
    {
        var obj = Object.Instantiate(prefab, parent);
        GameObjectPoolManager.I.Register(obj, prefab);
        obj.SetActive(false);
        return obj;
    }
    
    private void OnGet(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Destroyed object requested from pool: " + prefab.name);
        }
        obj.SetActive(true);
    }
    
    private void OnRelease(GameObject obj)
    {
        obj.SetActive(false);
    }
    
    private void OnDestroy(GameObject obj)
    {
        pendingRelease.Remove(obj);
        Object.Destroy(obj);
        GameObjectPoolManager.I.OnObjectDestroyed(obj);
    }
}