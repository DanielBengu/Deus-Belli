using UnityEngine;

public class ActionPerformer
{
    public FightManager manager;

    public void PerformAttack(Unit attacker, Unit defender)
    {
        attacker.HasPerformedMainAction = true;
        Attack(attacker, defender);
        manager.ActionInQueue = ActionPerformed.Default;
        manager.ActionTarget = null;
    }

    void Attack(Unit attacker, Unit defender)
    {
        AnimationPerformer.PerformAnimation(Animation.Attack, attacker);
        Quaternion rotation = defender.transform.rotation;
        rotation.y = Movement.FindCharacterDirection(defender.transform, attacker.transform);
        defender.transform.rotation = rotation;

        AnimationPerformer.PerformAnimation(Animation.TakeDamage, defender);

        defender.hpCurrent -= attacker.attack;
        if (defender.hpCurrent <= 0)
            KillUnit(defender);
    }

    void KillUnit(Unit unitToKill)
    {
        manager.unitsOnField.Remove(unitToKill);
        Object.Destroy(unitToKill.gameObject);
    }
}