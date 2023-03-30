using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPerformer
{

    public void Attack(Unit attacker, Unit defender)
    {
        Animator anim = attacker.GetComponent<Animator>();
        string gender = attacker.name.Split(' ')[0];
        anim.Play($"{gender} Attack 1");
        defender.hpCurrent -= attacker.attack;
        if (defender.hpCurrent <= 0)
            Object.Destroy(defender.gameObject);
    }
}