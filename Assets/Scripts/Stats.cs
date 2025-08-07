using System;
using UnityEngine.Serialization;

[Serializable]
public class Stats
{
    public int numJumps;
    public int numDashes;
    public int jumpHeight;
    public int groundPound;
    public int numberOfBullets;
    public int reloadSpeedUp;
    [FormerlySerializedAs("explosionRadius")] public int bulletExplosionRadius;
    public int numberOfGrenades;
    public int grenadeReloadSpeedUp;
    public int numberOfGrenadesPerLaunch;
    public int grenadeExplosionRadius;
    [FormerlySerializedAs("bulletDamage")] public int bulletEnemyDamage;
    public int grenadeDamage;
    public int lootCollectionDistance;
    public int diggingDamage;
    public int jetPackFuel;
    public int health;
    public int healthRegen;
    public int bulletRicochetCount;
    

    public static Stats operator +(Stats a, Stats b)
    {
        return new Stats
        {
            numJumps = a.numJumps + b.numJumps,
            numDashes = a.numDashes + b.numDashes,
            jumpHeight = a.jumpHeight + b.jumpHeight,
            groundPound = a.groundPound + b.groundPound,
            numberOfBullets = a.numberOfBullets + b.numberOfBullets,
            reloadSpeedUp = a.reloadSpeedUp + b.reloadSpeedUp,
            bulletExplosionRadius = a.bulletExplosionRadius + b.bulletExplosionRadius,
            numberOfGrenades = a.numberOfGrenades + b.numberOfGrenades,
            grenadeExplosionRadius = a.grenadeExplosionRadius + b.grenadeExplosionRadius,
            grenadeReloadSpeedUp = a.grenadeReloadSpeedUp + b.grenadeReloadSpeedUp,
            numberOfGrenadesPerLaunch = a.numberOfGrenadesPerLaunch + b.numberOfGrenadesPerLaunch,
            bulletEnemyDamage = a.bulletEnemyDamage + b.bulletEnemyDamage,
            grenadeDamage = a.grenadeDamage + b.grenadeDamage,
            lootCollectionDistance = a.lootCollectionDistance + b.lootCollectionDistance,
            diggingDamage = a.diggingDamage + b.diggingDamage,
            jetPackFuel = a.jetPackFuel + b.jetPackFuel,
            health = a.health + b.health,
            healthRegen = a.healthRegen + b.healthRegen,
            bulletRicochetCount = a.bulletRicochetCount + b.bulletRicochetCount,
        };
    }
}