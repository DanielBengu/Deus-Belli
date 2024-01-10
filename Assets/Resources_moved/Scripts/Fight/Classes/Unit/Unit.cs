using System;
using UnityEngine;
public class Unit : MonoBehaviour
{
    public UnitMovement Movement { get; set; }

    public FightManager FightManager { get; set; }

    public UnitData UnitData { get; set; } = new();
	public UnitFightData FightData { get; set; }

	public void Load(UnitData UnitData, FightManager manager, Tile currentTile)
	{
        this.UnitData.Name = UnitData.Name;
        this.UnitData.PortraitName = UnitData.PortraitName;
        Movement = new(this, currentTile);
		LoadStats(UnitData);
		FightData = new(this);
		FightManager = manager;
	}

	void LoadStats(UnitData unit)
	{
        UnitData.Traits = unit.Traits;
		UnitData.Stats.Hp = unit.Stats.Hp;
		UnitData.Stats.Armor = unit.Stats.Armor;
		UnitData.Stats.Ward = unit.Stats.Ward;
		UnitData.Stats.Movement = unit.Stats.Movement;
		UnitData.Stats.Attack = unit.Stats.Attack;
		UnitData.Stats.Range = unit.Stats.Range;
		UnitData.Faction = unit.Faction;
		UnitData.AttackType = unit.AttackType;
	}

    public void OnMouseDown()
    {
		Movement.CurrentTile.OnMouseDown();
    }

	public enum AttackType
	{
		Physical,
		Elemental,
		Arcane, //Special attack type, value isn't affected by bonus or malus
	}

	public enum StatsType
	{
		HP,
		Armor,
		Ward,
		Attack,
		Movement,
		Range,
	}
}