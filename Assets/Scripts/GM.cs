using System.Collections.Generic;
using UnityEngine;

public class GM: MonoBehaviour
{

    public static GM I;
    
    public int money = 0;
    public bool isFree = false;
    
    private void Awake()
    {
        I = this;
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
}