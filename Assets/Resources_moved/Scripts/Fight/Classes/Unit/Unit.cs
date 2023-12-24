using System;
using UnityEngine;
public class Unit : MonoBehaviour
{
    public UnitMovement Movement { get; set; }

    public FightManager FightManager { get; set; }

    public UnitData unitData;
    public UnitFightData fightData;

	public void Load(UnitData unit)
	{
        unitData.Name = unit.Name;
        unitData.Traits = unit.Traits;
        Movement = new(this);
        LoadStats(unit);
	}

	void LoadStats(UnitData unit)
	{
        unitData.Traits = unit.Traits;
		unitData.Stats.Hp = unit.Stats.Hp;
		unitData.Stats.Movement = unit.Stats.Movement;
		unitData.Stats.Attack = unit.Stats.Attack;
		unitData.Faction = unit.Faction;
		unitData.Stats.Range = unit.Stats.Range;
	}

    public void OnMouseDown()
    {
		Movement.CurrentTile.OnMouseDown();
    }
    //Called at the end of an attack animation
    public void StartDamageForOpponent()
	{
        FightManager.MakeUnitTakeDamage();
	}

    public enum TraitsEnum
	{
        Floaty,
        Healthy,
        Magic_Defence,
        Overload,
        Regeneration,
        Second_Wind,
        Speedy,
        Strong,
        Tanky,
        Wealthy
	}
}