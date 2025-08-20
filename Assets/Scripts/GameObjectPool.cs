using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class GameObjectPool
{
    private GameObject prefab;
    private Transform parent;
    private Dictionary<GameObject, Awaitable> pendingRelease = new();
    private HashSet<GameObject> inactiveObjects = new();
    private LinkedList<GameObject> activeObjects = new();
    
    private int maxSize;
    
    public GameObjectPool(GameObject prefab, Transform parent, int maxSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.maxSize = maxSize;
    }
    
    public GameObject Instantiate(Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetFromPool();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true); // Activate after setting position and rotation to have correct position OnEnable
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
        if (pendingRelease.ContainsKey(obj))
        {
            try
            {
                pendingRelease[obj].Cancel();
            }
            catch (InvalidOperationException)
            {
                // This can happen if the awaitable was already completed or cancelled
            }
            pendingRelease.Remove(obj);
        }
        if (time != 0f)
        {
            var awaitable = Awaitable.WaitForSecondsAsync(time);
            pendingRelease[obj] = awaitable;
            try
            {
                await awaitable;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                pendingRelease.Remove(obj);
            }
        }
      
        ReleaseToPool(obj);
    }

    private GameObject GetFromPool()
    {
        GameObject obj;
        if (activeObjects.Count == maxSize)
        {
            Release(activeObjects.First.Value);
        }
        if (inactiveObjects.Count > 0)
        {
            obj = inactiveObjects.First();
            inactiveObjects.Remove(obj);
        }
        else
        {
            obj = CreatePooledObject();
        }

        activeObjects.AddLast(obj);
        return obj;
    }

    private void ReleaseToPool(GameObject obj)
    {
        obj.SetActive(false);

        if (!inactiveObjects.Add(obj)) return;
        activeObjects.Remove(obj);
    }
    
    private GameObject CreatePooledObject()
    {
        var obj = Object.Instantiate(prefab, parent);
        GameObjectPoolManager.I.Register(obj, prefab);
        obj.SetActive(false);
        return obj;
    }

    private IEnumerator RemoveFromPoolDelayed(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Release(obj);
    }
}