using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Stats
{
    public int numJumps;
    public int numDashes;
    public float jumpHeight;
    public float groundPound;
    public int numberOfBullets;
    public float reloadSpeedUp;
    [FormerlySerializedAs("explosionRadius")] public float bulletExplosionRadius;
    public int numberOfGrenades;
    public float grenadeReloadSpeedUp;
    public float numberOfGrenadesPerLaunch;
    public float grenadeExplosionRadius;
    [FormerlySerializedAs("bulletDamage")] public float bulletEnemyDamage;
    public float grenadeDamage;
    public float lootCollectionDistance;
    public float diggingDamage;
    public float jetPackFuel;
    public float health;
    public float healthRegen;
    public int bulletRicochetCount;
    public float magSize;
    public float mags;
    public float reloadTime;
    public float dashDamage;
    public float dashRadius;
    public float dashReloadSpeed;
    public float spikeProtection;
    public float headProtection;
    public float bulletProtection;


    public static Stats operator +(Stats a, float b)
    {
        return new Stats
        {
            numJumps = a.numJumps + Mathf.RoundToInt(b),
            numDashes = a.numDashes + Mathf.RoundToInt(b),
            jumpHeight = a.jumpHeight + b,
            groundPound = a.groundPound + b,
            numberOfBullets = a.numberOfBullets + Mathf.RoundToInt(b),
            reloadSpeedUp = a.reloadSpeedUp + b,
            bulletExplosionRadius = a.bulletExplosionRadius + b,
            numberOfGrenades = a.numberOfGrenades + Mathf.RoundToInt(b),
            grenadeExplosionRadius = a.grenadeExplosionRadius + b,
            grenadeReloadSpeedUp = a.grenadeReloadSpeedUp + b,
            numberOfGrenadesPerLaunch = a.numberOfGrenadesPerLaunch + b,
            bulletEnemyDamage = a.bulletEnemyDamage + b,
            grenadeDamage = a.grenadeDamage + b,
            lootCollectionDistance = a.lootCollectionDistance + b,
            diggingDamage = a.diggingDamage + b,
            jetPackFuel = a.jetPackFuel + b,
            health = a.health + b,
            healthRegen = a.healthRegen + b,
            bulletRicochetCount = a.bulletRicochetCount + Mathf.RoundToInt(b),
            magSize = a.magSize + b,
            mags = a.mags + b,
            reloadTime = a.reloadTime + b,
            dashDamage = a.dashDamage + b,
            dashRadius = a.dashRadius + b,
            dashReloadSpeed = a.dashReloadSpeed + b,
            spikeProtection = a.spikeProtection + b,
            headProtection = a.headProtection + b,
            bulletProtection = a.bulletProtection + b,
        };
    }

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
            magSize = a.magSize + b.magSize,
            mags = a.mags + b.mags,
            reloadTime = a.reloadTime + b.reloadTime,
            dashDamage = a.dashDamage + b.dashDamage,
            dashRadius = a.dashRadius + b.dashRadius,
            dashReloadSpeed = a.dashReloadSpeed + b.dashReloadSpeed,
            spikeProtection = a.spikeProtection + b.spikeProtection,
            headProtection = a.headProtection + b.headProtection,
            bulletProtection = a.bulletProtection + b.bulletProtection,
        };
    }

    public static Stats operator *(Stats a, float multiplier)
    {
         return new Stats
        {
            numJumps = Mathf.RoundToInt(a.numJumps * multiplier),
            numDashes = Mathf.RoundToInt(a.numDashes * multiplier),
            jumpHeight = a.jumpHeight * multiplier,
            groundPound = a.groundPound * multiplier,
            numberOfBullets = Mathf.RoundToInt(a.numberOfBullets * multiplier),
            reloadSpeedUp = a.reloadSpeedUp * multiplier,
            bulletExplosionRadius = a.bulletExplosionRadius * multiplier,
            numberOfGrenades = Mathf.RoundToInt(a.numberOfGrenades * multiplier),
            grenadeExplosionRadius = a.grenadeExplosionRadius * multiplier,
            grenadeReloadSpeedUp = a.grenadeReloadSpeedUp * multiplier,
            numberOfGrenadesPerLaunch = a.numberOfGrenadesPerLaunch * multiplier,
            bulletEnemyDamage = a.bulletEnemyDamage * multiplier,
            grenadeDamage = a.grenadeDamage * multiplier,
            lootCollectionDistance = a.lootCollectionDistance * multiplier,
            diggingDamage = a.diggingDamage * multiplier,
            jetPackFuel = a.jetPackFuel * multiplier,
            health = a.health * multiplier,
            healthRegen = a.healthRegen * multiplier,
            bulletRicochetCount = Mathf.RoundToInt(a.bulletRicochetCount * multiplier),
            magSize = a.magSize * multiplier,
            mags = a.mags * multiplier,
            reloadTime = a.reloadTime * multiplier,
            dashDamage = a.dashDamage * multiplier,
            dashRadius = a.dashRadius * multiplier,
            dashReloadSpeed = a.dashReloadSpeed * multiplier,
            spikeProtection = a.spikeProtection * multiplier,
            headProtection = a.headProtection * multiplier,
            bulletProtection = a.bulletProtection * multiplier,
        };
    }

    public static Stats operator *(Stats a, Stats b)
    {
            return new Stats
            {
                numJumps = Mathf.RoundToInt(a.numJumps * b.numJumps),
                numDashes = Mathf.RoundToInt(a.numDashes * b.numDashes),
                jumpHeight = a.jumpHeight * b.jumpHeight,
                groundPound = a.groundPound * b.groundPound,
                numberOfBullets = Mathf.RoundToInt(a.numberOfBullets * b.numberOfBullets),
                reloadSpeedUp = a.reloadSpeedUp * b.reloadSpeedUp,
                bulletExplosionRadius = a.bulletExplosionRadius * b.bulletExplosionRadius,
                numberOfGrenades = Mathf.RoundToInt(a.numberOfGrenades * b.numberOfGrenades),
                grenadeExplosionRadius = a.grenadeExplosionRadius * b.grenadeExplosionRadius,
                grenadeReloadSpeedUp = a.grenadeReloadSpeedUp * b.grenadeReloadSpeedUp,
                numberOfGrenadesPerLaunch = a.numberOfGrenadesPerLaunch * b.numberOfGrenadesPerLaunch,
                bulletEnemyDamage = a.bulletEnemyDamage * b.bulletEnemyDamage,
                grenadeDamage = a.grenadeDamage * b.grenadeDamage,
                lootCollectionDistance = a.lootCollectionDistance * b.lootCollectionDistance,
                diggingDamage = a.diggingDamage * b.diggingDamage,
                jetPackFuel = a.jetPackFuel * b.jetPackFuel,
                health = a.health * b.health,
                healthRegen = a.healthRegen * b.healthRegen,
                bulletRicochetCount = Mathf.RoundToInt(a.bulletRicochetCount * b.bulletRicochetCount),
                magSize = a.magSize * b.magSize,
                mags = a.mags * b.mags,
                reloadTime = a.reloadTime * b.reloadTime,
                dashDamage = a.dashDamage * b.dashDamage,
                dashRadius = a.dashRadius * b.dashRadius,
                dashReloadSpeed = a.dashReloadSpeed * b.dashReloadSpeed,
                spikeProtection = a.spikeProtection * b.spikeProtection,
                headProtection = a.headProtection * b.headProtection,
                bulletProtection = a.bulletProtection * b.bulletProtection,
            };
        }
}