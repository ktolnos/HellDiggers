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
    public int magSize = 1;
    public int mags = 1;
 
    public float bulletSpeed = 5f;
    public bool ignoreStatsScaling = false;

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
    public bool infiniteAmmo = true;
    public Transform bulletSpawn;
    public float recoilForce = 1f;
    private Rigidbody2D rb;
    private bool hasRB = false;

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
        var applyStats = bulletPrefab.isPlayerBullet && !ignoreStatsScaling;
        var statMult = applyStats ? 1f : 0f;
        float fireRateBonus = isSecondary ? Player.I.stats.grenadeReloadSpeedUp : Player.I.stats.fireRateBoost;
        fireRateBonus *= statMult;
        return fireDelay / (1f + fireRateBonus);
    }

    private int GetMagSize()
    {
        var applyStats = bulletPrefab.isPlayerBullet && !ignoreStatsScaling;
        return magSize + (int)((isSecondary ? 0 : Player.I.stats.magSize) *  (applyStats ? 1 : 0));
    }

    private int GetMags()
    {
        var applyStats = bulletPrefab.isPlayerBullet && !ignoreStatsScaling;
        return mags + (int)((isSecondary ? 0 : Player.I.stats.mags) *  (applyStats ? 1 : 0));
    }

    private int GetTotalAmmo()
    {
        return GetMagSize() * GetMags();
    }

    private float GetReloadTime()
    {
        var applyStats = bulletPrefab != null && bulletPrefab.isPlayerBullet && !ignoreStatsScaling && !bulletPrefab.ignoreStatsScaling;
        return reloadTime * (applyStats ? Player.I.stats.reloadTime : 1f);
    }

    public IEnumerator Shoot()
    {
        var playerOnlyMult = (bulletPrefab != null && bulletPrefab.isPlayerBullet && !ignoreStatsScaling && !bulletPrefab.ignoreStatsScaling) ? 1 : 0;
        var fireDelayUpgraded = GetFireRate();
        if (Time.time - lastFireTime < fireDelayUpgraded || AmmoInMagLeft <= 0)
            yield break;
        lastFireTime = Time.time;
        if (shootSound != null)
        {
            SoundManager.I.PlaySfx(shootSound, bulletSpawn.position);
        }
        if (animator != null)
        {
            animator.PlayOnce();
        }
        var numberOfBulletsStat = isSecondary ? Player.I.stats.numberOfGrenadesPerLaunch : Player.I.stats.numberOfBullets;
        var bulletsCount = numberOfBullets * (1+(int)(numberOfBulletsStat * playerOnlyMult));
        if (hasRB)
        {
            rb.AddForce(-bulletSpawn.right * recoilForce * bulletsCount, ForceMode2D.Impulse);
        }
        for (var i = 0; i < bulletsCount; i++)
        {
            var bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity, Level.I.spawnedObjectsParent);
            bullet.ignoreStatsScaling = ignoreStatsScaling || bulletPrefab.ignoreStatsScaling;
            bullet.transform.localEulerAngles = bulletSpawn.eulerAngles + Vector3.forward *
                ((i + 1) / 2 * spread * (i % 2 == 0 ? 1f : -1f) + Random.Range(-randomSpread, randomSpread));
            bullet.gameObject.SetActive(true);
            bullet.rb.linearVelocity = bullet.transform.right * bulletSpeed;
            Destroy(bullet.gameObject, bulletLifeTime);
            yield return new WaitForFixedUpdate();
        }
        if (!infiniteAmmo)
        {
            AmmoInMagLeft-= 1 + playerOnlyMult * (isSecondary ? 1 : Player.I.stats.numberOfBullets);
        }
        if (AmmoInMagLeft <= 0)
        {
            yield return Reload();
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
        var totalToAdd = ammo * GetMagSize();
        var addedAmmo = Mathf.Min(GetMagSize() - AmmoInMagLeft, totalToAdd);
        AmmoInMagLeft += addedAmmo;
        totalToAdd -= addedAmmo;
        AmmoOutOfMagLeft += totalToAdd;
        AmmoOutOfMagLeft = Mathf.Min(AmmoOutOfMagLeft, GetTotalAmmo() - GetMagSize());
    }

    public void Reset()
    {
        isReloading = false;
        AmmoInMagLeft = GetMagSize();
        AmmoOutOfMagLeft = GetTotalAmmo() - GetMagSize();
        rb = GetComponentInParent<Rigidbody2D>();
        hasRB = rb != null;
    }
}