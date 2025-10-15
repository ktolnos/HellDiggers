using UnityEngine;

public class DontDestroySingleton: MonoBehaviour
{
    private static DontDestroySingleton instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
        
}