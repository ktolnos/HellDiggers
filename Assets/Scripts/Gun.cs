using DG.Tweening;
using UnityEngine;

public class Gun: MonoBehaviour
{
    public Bullet bulletPrefab;
    public float fireDelay = 0.5f;
    public int numberOfBullets = 5;
    public float bulletLifeTime = 5f;
    public float spread = 0.1f;
    public float bulletSpeed = 5f;

    private float lastFireTime = -100f;
    public bool grenadeMode = false;
    public SpriteAnimator animator;
    
    public void Shoot()
    {
        var statMult = bulletPrefab.isPlayerBullet ? 1f : 0f;
        var fireRateStat = grenadeMode ? Player.I.stats.grenadeReloadSpeedUp : Player.I.stats.reloadSpeedUp;
        fireRateStat *= statMult;
        var fireDelayUpgraded = fireDelay / (fireRateStat + 1f);
        if (Time.time - lastFireTime < fireDelayUpgraded)
            return;
        lastFireTime = Time.time;
        if (animator != null)
        {
            animator.PlayOnce();
        }
        var numberOfBulletsStat = grenadeMode ? Player.I.stats.numberOfGrenadesPerLaunch : Player.I.stats.numberOfBullets * 2f;
        var bulletsCount = numberOfBullets + numberOfBulletsStat * statMult;
        for (var i = 0; i < bulletsCount; i++)
        {
            var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity, Level.I.spawnedObjectsParent);
            bullet.transform.localEulerAngles = transform.eulerAngles + Vector3.forward * 
                (((i+1) / 2) * spread * (i%2 == 0 ? 1f : -1f));
            bullet.gameObject.SetActive(true);
            bullet.rb.linearVelocity = bullet.transform.right * bulletSpeed;
            Destroy(bullet.gameObject, bulletLifeTime);
        }
    }
}