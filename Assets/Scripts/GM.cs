using System;
using System.Collections.Generic;
using IPD;
using UnityEngine;
using UnityEngine.InputSystem;

public class GM: MonoBehaviour
{

    public static GM I;
    public Resources resources;
    
    public int money = 0;
    public bool isFree = false;
    public delegate void CloseUIDelegate();
    private static Stack<CloseUIDelegate> closeUIDelegates = new();
    public static bool IsUIOpen => closeUIDelegates.Count > 0 || Time.unscaledTime == uiCloseTimestamp;
    
    private InputAction closeAction;
    private static float uiCloseTimestamp = 0f;
    
    private async void Awake()
    {
        I = this;
        bool isLoaded = await InputPromptUtility.Load();
        closeAction = InputSystem.actions.FindAction("Cancel");
        
        closeAction.performed += ctx => PopTopUI();
    }
    
    public static HashSet<Health> DamageEntities(Vector3 position, float radius, float damage, DamageDealerType type, HashSet<Health> excluded = null, float recoil = 0)
    {
        var colliders = Physics2D.OverlapCircleAll(position, radius);
        return DamageEntities(colliders, damage, type, excluded, position, recoil);
    }
    
    public static HashSet<Health> DamageEntitiesCapsule(Vector3 start, Vector3 end, float radius, float damage, DamageDealerType type, HashSet<Health> excluded)
    {
        var colliders = Physics2D.OverlapCapsuleAll((start + end) / 2, new Vector2(radius * 2, Vector3.Distance(start, end)), CapsuleDirection2D.Vertical, 
            Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg);
        return DamageEntities(colliders, damage, type, excluded, start, 0f);
    }



    private static HashSet<Health> DamageEntities(Collider2D[] colliders, float damage, DamageDealerType type,
        HashSet<Health> excluded, Vector3 sourcePosition, float recoil)
    {
        var healths = new HashSet<Health>();
        foreach (var col in colliders)
        {
            healths.Add(col.GetComponentInParent<Health>());
        }
        if (excluded != null)
        {
            healths.ExceptWith(excluded);
        }
        foreach (var health in healths)
        {
            if (health != null)
            {
                health.Damage(damage, type);
                if (health.hasRb)
                {
                    health.rb.AddForce((health.rb.position - (Vector2)sourcePosition).normalized * recoil, ForceMode2D.Impulse);
                }
            }
        }

        return healths;
    }

    public static void OnUIOpen(CloseUIDelegate closeDelegate)
    {
        closeUIDelegates.Push(closeDelegate);
    }

    [Serializable]
    public struct Resources
    {
        public int copper;
        public int iron;
        public int gold;
        public int emerald;
        public int diamond;
        
        public static Resources operator +(Resources a, Resources b)
        {
            return new Resources
            {
                copper = a.copper + b.copper,
                iron = a.iron + b.iron,
                gold = a.gold + b.gold,
                emerald = a.emerald + b.emerald,
                diamond = a.diamond + b.diamond,
            };
        }   
    }
    
    public int copperPrice = 1;
    public int ironPrice = 10;
    public int goldPrice = 50;
    public int emeraldPrice = 200;
    public int diamondPrice = 1000;

    public int GetTotalMoney()
    {
        var total = money;
        total += resources.copper * copperPrice;
        total += resources.iron * ironPrice;
        total += resources.gold * goldPrice;
        total += resources.emerald * emeraldPrice;
        total += resources.diamond * diamondPrice;
        return total;
    }
    
    public static void PopTopUI()
    {
        if (closeUIDelegates.Count > 0)
        {
            closeUIDelegates.Pop().Invoke();
            uiCloseTimestamp = Time.unscaledTime;
        }
    }
}