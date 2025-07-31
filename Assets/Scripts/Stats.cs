using System;

[Serializable]
public class Stats
{
    public int numJumps;
    public int numDashes;
    public float jumpHeight;
    public float dashInvincibilityTime;
    public int numberOfBullets;
    public float reloadSpeedUp;
    public float explosionRadius;
    public int numberOfGrenades;
    public int grenadeExplosionRadius;


    public static Stats operator +(Stats a, Stats b)
    {
        return new Stats
        {
            numJumps = a.numJumps + b.numJumps,
            numDashes = a.numDashes + b.numDashes,
            jumpHeight = a.jumpHeight + b.jumpHeight,
            dashInvincibilityTime = a.dashInvincibilityTime + b.dashInvincibilityTime,
            numberOfBullets = a.numberOfBullets + b.numberOfBullets,
            reloadSpeedUp = a.reloadSpeedUp + b.reloadSpeedUp,
            explosionRadius = a.explosionRadius + b.explosionRadius,
            numberOfGrenades = a.numberOfGrenades + b.numberOfGrenades,
            grenadeExplosionRadius = a.grenadeExplosionRadius + b.grenadeExplosionRadius
        };
    }
    
    public static Stats operator -(Stats a, Stats b)
    {
        return new Stats
        {
            numJumps = a.numJumps - b.numJumps,
            numDashes = a.numDashes - b.numDashes,
            jumpHeight = a.jumpHeight - b.jumpHeight,
            dashInvincibilityTime = a.dashInvincibilityTime - b.dashInvincibilityTime,
            numberOfBullets = a.numberOfBullets - b.numberOfBullets,
            reloadSpeedUp = a.reloadSpeedUp - b.reloadSpeedUp,
            explosionRadius = a.explosionRadius - b.explosionRadius,
            numberOfGrenades = a.numberOfGrenades - b.numberOfGrenades,
            grenadeExplosionRadius = a.grenadeExplosionRadius - b.grenadeExplosionRadius
        };
    }
}