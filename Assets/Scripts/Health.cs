using UnityEngine;
using UnityEngine.UI;

public class Health: MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isPlayer;
    private bool isDead;
    public bool isInvulnerable;
    
    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void Damage(float damage, DamageDealerType type)
    {
        if (isPlayer && type==DamageDealerType.Player)
            return; // no self damage
        if (!isPlayer && type == DamageDealerType.Enemy)
            return;
        if (isDead) return; // can't take damage if dead
        if (damage < 0f) return; // no negative damage
        if (isInvulnerable) return; // can't take damage if invulnerable
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        if (currentHealth <= 0f)
        {
            isDead = true;
            foreach (var deathHandler in GetComponents<IDeathHandler>())
            {
                deathHandler.Die();
            }
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return; // can't heal if dead
        if (amount < 0f) return; // no negative healing
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void Revive()
    {
        currentHealth = maxHealth;
        isDead = false;
    }
}