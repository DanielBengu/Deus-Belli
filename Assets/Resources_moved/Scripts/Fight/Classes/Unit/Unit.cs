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
		public TraitsEnum traitEnum;
		public string name;
		public string description;
		public int level;

		public Trait(TraitsEnum traitEnum, string name, string description, int level)
		{
			this.traitEnum = traitEnum;
			this.name = name;
			this.description = description;
			this.level = level;
		}

		#region Movement Bonuses
		public static int GetSpeedyBonus(int baseValue, int level)
		{
			int SPEED_BONUS_VALUE = level + 1;
			return baseValue + SPEED_BONUS_VALUE;
		}

		#endregion

		#region Attack Bonuses

		public static int GetStrongBonus(int baseValue, int level)
		{
			int STRONG_BONUS_VALUE = level;
			return baseValue * STRONG_BONUS_VALUE;
		}

		public static int GetOverloadBonus(int baseValue, int level)
		{
			int OVERLOAD_BONUS_VALUE = level + 1;
			return baseValue * OVERLOAD_BONUS_VALUE;
		}

		public static int GetTankyBonusAttack(int attack, int level)
		{
			int TANKY_BONUS_ATTACK_VALUE = level;
			return attack + TANKY_BONUS_ATTACK_VALUE;
		}

		#endregion

		#region Hp Bonuses

		public static int GetTankyBonusHp(int hp, int level)
		{
			int TANKY_BONUS_HP_VALUE = level * 2;
			return hp + TANKY_BONUS_HP_VALUE;
		}

		public static int GetHealthyBonusHp(int baseHp, int level)
		{
			int HEALTHY_BONUS_HP_VALUE = level;
			return baseHp * HEALTHY_BONUS_HP_VALUE;
		}
		#endregion

		#region Ward Traits

		public static int GetMagicDefenceBonus(int baseWard, int level)
		{
			int DEFENCE_BONUS_VALUE = 20 + (10 * level); //percentage of hp restored
			return baseWard * DEFENCE_BONUS_VALUE / 100;
		}

		#endregion

		public static int GetOverloadBonusDamageTaken(int damage, int level)
		{
			int OVERLOAD_BONUS_VALUE = level + 1;
			return damage * OVERLOAD_BONUS_VALUE;
		}

		public static int GetRegenerationAmount(int baseHp, int level)
		{
			int REGENERATION_BONUS_VALUE = 5 * level; //percentage of hp restored
			return baseHp * REGENERATION_BONUS_VALUE / 100;
		}
	}

	public enum AttackType
	{
		Physical,
		Elemental,
		Arcane, //Special attack type, value isn't affected by bonus or malus
	}
}