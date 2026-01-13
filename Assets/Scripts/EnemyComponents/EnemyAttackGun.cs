using System.Collections;
using UnityEngine;

public class EnemyAttackGun : EnemyAttack
{
    public Gun gun;
    public bool aimGun = true;
    
    private SpriteRenderer gunSpriteRenderer;
    
    void Start()
    {
        gunSpriteRenderer = gun.gameObject.GetComponentInChildren<SpriteRenderer>();
        gun.Reset();
    }

    public override IEnumerator PerformAttack(Vector3 target)
    {
        if (aimGun)
        {
            Vector3 gunVector = target - gun.transform.position;
            if (gunSpriteRenderer != null)
                gunSpriteRenderer.flipY = gunVector.x < 0f;
            gun.transform.localEulerAngles = Vector3.back * (Mathf.Atan2(gunVector.x, gunVector.y) * Mathf.Rad2Deg - 90f);
        }
        gun.Shoot();
        yield break;
    }
}
