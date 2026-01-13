using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyAttacChooser : EnemyAttack
{
    [Serializable]
    public class AttackOption
    {
        public EnemyAttack attack;
        public float minDistance;
        public float maxDistance;
    }

    public AttackOption[] attackOptions;
    private AttackOption _lastAttackOption;
    public bool avoidRepeatingLastAttack = false;

    public override IEnumerator PerformAttack(Vector3 target)
    {
        float distanceToTarget = Vector3.Distance(transform.position, target);
        var availableAttacks =
            attackOptions.Where(x => distanceToTarget >= x.minDistance
                                     && distanceToTarget <= x.maxDistance
                                     && (!avoidRepeatingLastAttack || x != _lastAttackOption));
        var selectedAttack = availableAttacks.OrderBy(x => UnityEngine.Random.value).FirstOrDefault();
        if (selectedAttack != null)
        {
            _lastAttackOption = selectedAttack;
            yield return selectedAttack.attack.Attack(target);
        }
    }
}