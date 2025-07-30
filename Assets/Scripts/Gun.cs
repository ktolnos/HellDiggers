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
    
    public void Shoot()
    {
        if (Time.time - lastFireTime < fireDelay)
            return;
        lastFireTime = Time.time;
        
        for (var i = 0; i < numberOfBullets; i++)
        {
            var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.localEulerAngles = transform.localEulerAngles + Vector3.forward * (i * spread - spread / 2f);
            bullet.gameObject.SetActive(true);
            bullet.rb.linearVelocity = bullet.transform.right * bulletSpeed;
            Destroy(bullet.gameObject, bulletLifeTime);
        }
    }
}