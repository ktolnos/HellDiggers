using System.Collections.Generic;
using IPD;
using UnityEngine;

public class GM: MonoBehaviour
{

    public static GM I;
    
    public int money = 0;
    public bool isFree = false;
    private static int uiDepth = 0;
    public static bool IsUIOpen => uiDepth > 0;
    
    private async void Awake()
    {
        I = this;
        bool isLoaded = await InputPromptUtility.Load();
    }
    
    public static void DamageEntities(Vector3 position, float radius, float damage, DamageDealerType type)
    {
        var colliders = Physics2D.OverlapCircleAll(position, radius);
        var healths = new HashSet<Health>();
        foreach (var col in colliders)
        {
            healths.Add(col.GetComponentInParent<Health>());
        }
        foreach (var health in healths)
        {
            if (health != null)
            {
                health.Damage(damage, type);
            }
        }
    }

    public static void OnUIOpen()
    {
        uiDepth++;
    }
    
    public static void OnUIClose()
    {
        uiDepth--;
        if (uiDepth < 0)
        {
            uiDepth = 0;
        }
    }
}