using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Health: MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isPlayer;
    private bool isDead;
    public bool isInvulnerable;
    public AudioClip hurt;
    public AudioClip deathSound;
    public SpriteRenderer hurtAnimation;
    public Material hurtMaterial;
    private bool hasHurtAnimation;
    private Material defaultMaterial;
    
    public Volume vignetteVolume;
    private bool isRunningHurtFlash = false;
    public delegate void OnDamageTaken(float damage, DamageDealerType type);
    public OnDamageTaken onDamageTaken;
    
    private void Start()
    {
        currentHealth = maxHealth;
        hasHurtAnimation = hurtAnimation != null && hurtMaterial != null;
        if (hasHurtAnimation)
        {
            defaultMaterial = hurtAnimation.material;
        }
    }
    
    public void Damage(float damage, DamageDealerType type)
    {
        if (isPlayer && type==DamageDealerType.Player)
            return; // no self damage
        if (!isPlayer && type == DamageDealerType.Enemy)
            return;
        if (isDead) return; // can't take damage if dead
        if (damage <= 0f) return; // no negative damage
        if (isInvulnerable) return; // can't take damage if invulnerable
        onDamageTaken?.Invoke(damage, type);
        if (hurt != null)
        {
            if (!isPlayer)
            {
                SoundManager.I.PlaySfx(hurt, transform.position, relativeValue:2f);
            }
            else
            {
                SoundManager.I.PlaySfx(hurt, relativeValue:100f);
            }
        }

        if (isPlayer)
        {
            HurtFlashVignette();
        }

        if (hasHurtAnimation)
        {
            StartCoroutine(AnimateHurt());
        }
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        if (currentHealth <= 0f)
        {
            isDead = true;
            foreach (var deathHandler in GetComponents<IDeathHandler>())
            {
                deathHandler.Die();
            }
            if (deathSound != null)
            {
                SoundManager.I.PlaySfx(deathSound, transform.position, 2f);
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

    private IEnumerator AnimateHurt()
    {
        hurtAnimation.material = hurtMaterial;
        yield return new WaitForSeconds(0.2f);
        hurtAnimation.material = defaultMaterial;
    }
    
    private async void HurtFlashVignette()
    {
        if (isRunningHurtFlash)
            return;
        isRunningHurtFlash = true;
        vignetteVolume.profile.TryGet(out Vignette vignette);
        var initialIntensity = vignette.intensity.value;
        var initialColor = vignette.color.value;
        vignette.intensity.value = 0.5f;
        vignette.color.value = Color.red;
        await Awaitable.WaitForSecondsAsync(0.1f);
        vignette.intensity.value = initialIntensity;
        vignette.color.value = initialColor;
        isRunningHurtFlash = false;
    }
}