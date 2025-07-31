using UnityEngine;
using UnityEngine.UI;

public class Health: MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isPlayer;
    
    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void Damage(float damage, bool isPlayerDamage = false)
    {
        if (isPlayer && isPlayerDamage)
            return; // no self damage
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        if (currentHealth <= 0f)
        {
            foreach (var deathHandler in GetComponents<IDeathHandler>())
            {
                deathHandler.Die();
            }
        }
    }
}