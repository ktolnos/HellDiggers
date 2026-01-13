using System.Collections;
using UnityEngine;

public class EnemyAttackMeleeWave : EnemyAttack
{
    public EnemyAttack baseMeleeAttack;
    public float waveDistanceStart = 1f;
    public float waveDistanceEnd = 10f;
    public float waveDuration = 2f;
    public int numAttacksInWave = 5;
    public bool twoSides = false;

    public override IEnumerator PerformAttack(Vector3 target)
    {
        float attackInterval = waveDuration / numAttacksInWave;
        var startPosition = transform.position;
        for (int i = 0; i < numAttacksInWave; i++)
        {
            float t = (float)i / (numAttacksInWave - 1);
            float distance = Mathf.Lerp(waveDistanceStart, waveDistanceEnd, t);
            Vector3 direction = ((Vector2)target - (Vector2)startPosition).normalized;
            Vector3 attackPosition = startPosition + direction * distance;

            // Perform the base melee attack at the calculated position
            StartCoroutine(baseMeleeAttack.PerformAttack(attackPosition));

            if (twoSides)
            {
                Vector3 oppositeAttackPosition = startPosition - direction * distance;
                StartCoroutine(baseMeleeAttack.PerformAttack(oppositeAttackPosition));
            }

            yield return new WaitForSeconds(attackInterval);
        }
    }
}
