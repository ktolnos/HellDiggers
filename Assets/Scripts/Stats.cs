using System;
using UnityEngine.Serialization;

[Serializable]
public class Stats
{
    public int numJumps;
    public int numDashes;
    public float jumpHeight;
    public float dashSize;
    public int numberOfBullets;
    public float reloadSpeedUp;
    public float explosionRadius;
    public int numberOfGrenades;
    public float grenadeReloadSpeedUp;
    public int numberOfGrenadesPerLaunch;
    public int grenadeExplosionRadius;
    public float bulletDamage;
    public float grenadeDamage;
    public float lootCollectionDistance = 1f;
    public float diggingDamage;
    public float jetPackFuel;

    public static Stats operator +(Stats a, Stats b)
    {
        return new Stats
        {
            numJumps = a.numJumps + b.numJumps,
            numDashes = a.numDashes + b.numDashes,
            jumpHeight = a.jumpHeight + b.jumpHeight,
            dashSize = a.dashSize + b.dashSize,
            numberOfBullets = a.numberOfBullets + b.numberOfBullets,
            reloadSpeedUp = a.reloadSpeedUp + b.reloadSpeedUp,
            explosionRadius = a.explosionRadius + b.explosionRadius,
            numberOfGrenades = a.numberOfGrenades + b.numberOfGrenades,
            grenadeExplosionRadius = a.grenadeExplosionRadius + b.grenadeExplosionRadius,
            grenadeReloadSpeedUp = a.grenadeReloadSpeedUp + b.grenadeReloadSpeedUp,
            numberOfGrenadesPerLaunch = a.numberOfGrenadesPerLaunch + b.numberOfGrenadesPerLaunch,
            bulletDamage = a.bulletDamage + b.bulletDamage,
            grenadeDamage = a.grenadeDamage + b.grenadeDamage,
            lootCollectionDistance = a.lootCollectionDistance + b.lootCollectionDistance,
        };
    }
    
    public static Stats operator -(Stats a, Stats b)
    {
        return new Stats
        {
            numJumps = a.numJumps - b.numJumps,
            numDashes = a.numDashes - b.numDashes,
            jumpHeight = a.jumpHeight - b.jumpHeight,
            dashSize = a.dashSize - b.dashSize,
            numberOfBullets = a.numberOfBullets - b.numberOfBullets,
            reloadSpeedUp = a.reloadSpeedUp - b.reloadSpeedUp,
            explosionRadius = a.explosionRadius - b.explosionRadius,
            numberOfGrenades = a.numberOfGrenades - b.numberOfGrenades,
            grenadeExplosionRadius = a.grenadeExplosionRadius - b.grenadeExplosionRadius,
            grenadeReloadSpeedUp = a.grenadeReloadSpeedUp - b.grenadeReloadSpeedUp,
            numberOfGrenadesPerLaunch = a.numberOfGrenadesPerLaunch - b.numberOfGrenadesPerLaunch,
            bulletDamage = a.bulletDamage - b.bulletDamage,
            grenadeDamage = a.grenadeDamage - b.grenadeDamage,
            lootCollectionDistance = a.lootCollectionDistance - b.lootCollectionDistance,
        };
    }
}