using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Collections;

public class Gun: MonoBehaviour
{
    public string id;
    public LocalizedString gunName;
    public Bullet bulletPrefab;
    public float fireDelay = 0.5f;
    public int numberOfBullets = 5;
    public float bulletLifeTime = 5f;
    public float spread = 0.1f;
    public float randomSpread;
    public int price;
    public int magSize;
    public int mags;
 
    public float bulletSpeed = 5f;

    private float lastFireTime = -100f;
    [FormerlySerializedAs("secondary")] public bool isSecondary = false;
    public SpriteAnimator animator;
    
    public Image reloadIndicator;
    public AudioClip shootSound;
    
    public GunStation gunStation;
    [NonSerialized] public int AmmoInMagLeft;
    [NonSerialized] public int AmmoOutOfMagLeft;
    [NonSerialized] public bool isReloading;
    public float reloadTime;
    private float reloadStartTime = -1000f;

    private void Update()
    {
        if (bulletPrefab.isPlayerBullet && reloadIndicator != null)
        {
            // reloadIndicator.gameObject.SetActive(Player.I.stats.numberOfGrenadesPerLaunch  > 0);
            if (isSecondary)
            {
                reloadIndicator.fillAmount = Mathf.Clamp01((Time.time - lastFireTime) / GetFireRate());
            }
            else
            {
                reloadIndicator.fillAmount = Mathf.Clamp01((Time.time - reloadStartTime) / GetReloadTime());
            }
            reloadIndicator.color = reloadIndicator.fillAmount > 0.999f ? new Color(0.8f, 0.8f, 0.8f) : Color.gray;
            reloadIndicator.enabled = reloadIndicator.fillAmount < 0.999f || isSecondary;
        }
    }

    private float GetFireRate()
    {
        var statMult = bulletPrefab.isPlayerBullet ? 1f : 0f;
        float fireRateStat = isSecondary ? Player.I.stats.grenadeReloadSpeedUp : Player.I.stats.reloadSpeedUp;
        fireRateStat *= statMult;
        var fireDelayUpgraded = fireDelay * Mathf.Pow(0.8f, fireRateStat);
        return fireDelayUpgraded;
    }

    private int GetMagSize()
    {
        return magSize + (isSecondary ? 0 : Player.I.stats.magSize);
    }

    private int GetMags()
    {
        return mags + (isSecondary ? 0 : Player.I.stats.mags);
    }

    private int GetTotalAmmo()
    {
        return GetMagSize() * GetMags();
    }

    private float GetReloadTime()
    {
        return reloadTime * Mathf.Pow(0.8f, Player.I.stats.reloadTime);
    }

    public void Shoot()
    {
        var statMult = bulletPrefab.isPlayerBullet ? 1f : 0f;
        var fireDelayUpgraded = GetFireRate();
        if (Time.time - lastFireTime < fireDelayUpgraded || AmmoInMagLeft <= 0)
            return;
        lastFireTime = Time.time;
        if (shootSound != null)
        {
            SoundManager.I.PlaySfx(shootSound, transform.position);
        }
        if (animator != null)
        {
            animator.PlayOnce();
        }
        var numberOfBulletsStat = isSecondary ? Player.I.stats.numberOfGrenadesPerLaunch : Player.I.stats.numberOfBullets * 2f;
        var bulletsCount = numberOfBullets + numberOfBulletsStat * statMult;
        for (var i = 0; i < bulletsCount; i++)
        {
            var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity, Level.I.spawnedObjectsParent);
            bullet.transform.localEulerAngles = transform.eulerAngles + Vector3.forward *
                ((i + 1) / 2 * spread * (i % 2 == 0 ? 1f : -1f) + Random.Range(-randomSpread, randomSpread));
            bullet.gameObject.SetActive(true);
            bullet.rb.linearVelocity = bullet.transform.right * bulletSpeed;
            Destroy(bullet.gameObject, bulletLifeTime);
        }
        AmmoInMagLeft--;
        if (AmmoInMagLeft <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    public IEnumerator Reload()
    {
        if (isReloading)
            yield break;
        isReloading = true;
        reloadStartTime = Time.time;
        yield return new WaitForSeconds(GetReloadTime());
        isReloading = false;
        var reloadedAmmo = Mathf.Min(GetMagSize() - AmmoInMagLeft, AmmoOutOfMagLeft);
        AmmoInMagLeft += reloadedAmmo;
        AmmoOutOfMagLeft -= reloadedAmmo;
    }

    public void AddAmmo(int ammo)
    {
        AmmoOutOfMagLeft += ammo * GetMagSize();
    }

    public void Reset()
    {
        isReloading = false;
        AmmoInMagLeft = GetMagSize();
        AmmoOutOfMagLeft = GetTotalAmmo() - GetMagSize();
    }
}