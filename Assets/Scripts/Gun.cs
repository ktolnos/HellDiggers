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
    
    public void Shoot()
    {
        var fireRateStat = grenadeMode ? Player.I.stats.grenadeReloadSpeedUp : Player.I.stats.reloadSpeedUp;
        var fireDelayUpgraded = fireDelay / (fireRateStat + 1f);
        if (Time.time - lastFireTime < fireDelayUpgraded)
            return;
        lastFireTime = Time.time;

        var numberOfBulletsStat = grenadeMode ? Player.I.stats.numberOfGrenadesPerLaunch : Player.I.stats.numberOfBullets;
        var bulletsCount = numberOfBullets + numberOfBulletsStat;
        for (var i = 0; i < bulletsCount; i++)
        {
            var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.localEulerAngles = transform.eulerAngles + Vector3.forward * (i * spread - spread / 2f);
            bullet.gameObject.SetActive(true);
            bullet.rb.linearVelocity = bullet.transform.right * bulletSpeed;
            Destroy(bullet.gameObject, bulletLifeTime);
        }
    }
}