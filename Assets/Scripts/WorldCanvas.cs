using TMPro;
using UnityEngine;

public class WorldCanvas : MonoBehaviour
{
    public static WorldCanvas I;
    public DamageNumber damageNumberPrefab;
    
    
    private void Awake() 
    {
        I = this;
    }
    
    public void ShowDamageNumber(Vector3 worldPosition, float damageAmount, bool isCritical=false)
    {
        var pool = GameObjectPoolManager.I.GetOrRegisterPool(damageNumberPrefab.gameObject,
            transform, maxSize:15);
        var damageNumberObj = pool.InstantiateTemporarily(worldPosition, time: 0.5f);
        var damageNumber = damageNumberObj.GetComponent<DamageNumber>();
        var damageText = Mathf.RoundToInt(damageAmount).ToString();
        if (damageAmount < 10f)
        {
            damageText = damageAmount.ToString("0.#");
        }

        damageNumber.text.text = damageText;
        damageNumber.text.color = isCritical ? Color.red : Color.white;
    }
}
