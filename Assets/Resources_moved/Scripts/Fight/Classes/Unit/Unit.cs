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
		UnitData.Stats.Movement = unit.Stats.Movement;
		UnitData.Stats.Attack = unit.Stats.Attack;
		UnitData.Faction = unit.Faction;
		UnitData.Stats.Range = unit.Stats.Range;
	}

    public void OnMouseDown()
    {
		Movement.CurrentTile.OnMouseDown();
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

	public struct Trait
	{
		public string name;
		public string description;
		public int level;

		public Trait(string name, string description, int level)
		{
			this.name = name;
			this.description = description;
			this.level = level;
		}
	}
}