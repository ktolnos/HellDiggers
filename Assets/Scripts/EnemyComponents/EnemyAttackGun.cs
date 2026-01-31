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

            // rotate towards target smoothly for up to 1 second
            float elapsed = 0f;
            Quaternion targetRotation =
                Quaternion.LookRotation(Vector3.forward, gunVector) * Quaternion.Euler(0, 0, 90f);
            while (elapsed < 1f && Quaternion.Angle(gun.transform.rotation, targetRotation) > 1f)
            {
                gun.transform.rotation =
                    Quaternion.RotateTowards(gun.transform.rotation, targetRotation, 360f * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        yield return gun.Shoot();
    }
}