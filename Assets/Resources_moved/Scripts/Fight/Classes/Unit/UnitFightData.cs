using Retro.ThirdPersonCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unit;

public class UnitFightData
{
	readonly Unit parent;
	public Sprite sprite;
	public readonly BaseStats baseStats;
	public CurrentStats currentStats;
    public List<Trait> traitList = new();

    public UnitFightData(Unit parent)
    {
        this.parent = parent;

        Sprite unitSprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, parent.UnitData.PortraitName);
		sprite = unitSprite;
		baseStats = new(parent);
		currentStats = new();
		LoadTraits(parent.UnitData.Traits);
		LoadStats();
	}

	public void TakeDamage(int damage, AttackType typeOfDamage)
	{
		damage -= ApplyArmor(damage, typeOfDamage);

		if (ContainsTrait(TraitsEnum.Overload, out int levelOverload))
			damage = Trait.GetBonus(TraitsEnum.Overload, levelOverload, StatsType.HP, damage);

		if (damage > 0)
			currentStats.CURRENT_HP -= damage;

		if (ContainsTrait(TraitsEnum.Second_Wind, out int levelSecondWind))
			currentStats.CURRENT_HP = Trait.GetBonus(TraitsEnum.Second_Wind, levelSecondWind, StatsType.HP, baseStats.HP);

	}

	public void Heal(int healAmount)
	{
		currentStats.CURRENT_HP += healAmount;
		if(currentStats.CURRENT_HP > currentStats.MAXIMUM_HP)
			currentStats.CURRENT_HP = currentStats.MAXIMUM_HP;
	}

	public void ResetMovement()
	{
		currentStats.MOVEMENT = LoadMovement();
	}

	public void RemoveMovement(int movementToRemove)
	{
		currentStats.MOVEMENT -= movementToRemove;
		if (currentStats.MOVEMENT < 0) currentStats.MOVEMENT = 0;
	}

	public void StartOfTurnEffects()
    {
		ResetMovement();
		if (ContainsTrait(TraitsEnum.Regeneration, out int levelRegen))
			Heal(Trait.GetBonus(TraitsEnum.Regeneration, levelRegen, StatsType.HP, baseStats.HP));
	}

    public void LoadTraits(List<Traits> traitFromJSON)
    {
        foreach (var trait in traitFromJSON)
        {
            TraitsEnum traitEnum = (TraitsEnum)Enum.Parse(typeof(TraitsEnum), trait.Name);
			traitList.Add(new Trait(traitEnum, trait.Name, trait.Level));
		}
	}

	public bool ContainsTrait(TraitsEnum traitEnum, out int level)
	{
		try
		{
			Trait traitToSearch = traitList.Find(t => t.traitEnum == traitEnum);
			level = traitToSearch.level;
			return level != 0;
		}
		catch
		{
			level = -1;
			return false;
		}
	}
	void LoadStats()
	{
		currentStats.MAXIMUM_HP = LoadHp();
		currentStats.CURRENT_HP = currentStats.MAXIMUM_HP;
		currentStats.MOVEMENT = LoadMovement();
		currentStats.ATTACK = LoadAttack();
		currentStats.RANGE = parent.UnitData.Stats.Range;
		currentStats.ARMOR = LoadArmor();
		currentStats.WARD = LoadWard();
		currentStats.ATTACK_TYPE = (AttackType)Enum.Parse(typeof(AttackType), parent.UnitData.AttackType);
	}
	int LoadHp()
	{
		int hp = baseStats.HP;
		if (ContainsTrait(TraitsEnum.Tanky, out int level))
			hp += Trait.GetBonus(TraitsEnum.Tanky, level, StatsType.HP);
		if (ContainsTrait(TraitsEnum.Healthy, out int levelHealthy))
			hp += Trait.GetBonus(TraitsEnum.Healthy, levelHealthy, StatsType.HP, baseStats.HP);

		return hp;
	}
	int LoadMovement()
	{
		int movement = parent.UnitData.Stats.Movement;

		if (ContainsTrait(TraitsEnum.Speedy, out int level))
			movement += Trait.GetBonus(TraitsEnum.Speedy, level);

		return movement;
	}
	int LoadAttack()
	{
		int attack = baseStats.ATTACK;
		if (ContainsTrait(TraitsEnum.Strong, out int levelStrong))
			attack += Trait.GetBonus(TraitsEnum.Strong, levelStrong, StatsType.Attack, baseStats.ATTACK);
		if (ContainsTrait(TraitsEnum.Overload, out int levelOverload))
			attack += Trait.GetBonus(TraitsEnum.Overload, levelOverload, StatsType.Attack, baseStats.ATTACK);
		if (ContainsTrait(TraitsEnum.Tanky, out int levelTanky))
			attack += Trait.GetBonus(TraitsEnum.Tanky, levelTanky, StatsType.Attack);

		return attack;
	}
	int LoadArmor()
	{
		int armor = baseStats.ARMOR;
		return armor;
	}
	int LoadWard()
	{
		int ward = baseStats.WARD;
		if (ContainsTrait(TraitsEnum.Magic_Defence, out int levelDefence))
			ward += Trait.GetBonus(TraitsEnum.Magic_Defence, levelDefence, StatsType.Ward, baseStats.WARD);

		return ward;
	}
	int ApplyArmor(int damage, AttackType attackType)
	{
		switch (attackType)
		{
			case AttackType.Physical:
				damage -= currentStats.ARMOR;
				break;
			case AttackType.Elemental:
				damage -= currentStats.WARD;
				break;
			case AttackType.Arcane:
			default:
				break;
		}
		return damage;
	}

	public string GetStatText(int baseStat, int currentStat)
	{
		int difference = baseStat - currentStat;
		if (difference > 0)
			return $"{baseStat} - {difference}";
		if(difference < 0)
			return $"{baseStat} + {Math.Abs(difference)}";

		return baseStat.ToString();
	}
}

public class BaseStats
{
	public readonly int HP;
	public readonly int ATTACK;
	public readonly int ARMOR;
	public readonly int WARD;
	public readonly int MOVEMENT;
	public readonly int RANGE;
	public readonly AttackType ATTACK_TYPE;
	public BaseStats(Unit parent)
    {
		HP = parent.UnitData.Stats.Hp;
		ATTACK = parent.UnitData.Stats.Attack;
		ARMOR = parent.UnitData.Stats.Armor;
		WARD = parent.UnitData.Stats.Ward;
		RANGE = parent.UnitData.Stats.Range;
		MOVEMENT = parent.UnitData.Stats.Movement;
		ATTACK_TYPE = (AttackType)Enum.Parse(typeof(AttackType), parent.UnitData.AttackType);
	}
}

public class CurrentStats
{
	public int MAXIMUM_HP;
	public int CURRENT_HP;
	public int ATTACK;
	public int ARMOR;
	public int WARD;
	public int MOVEMENT;
	public int RANGE;
	public AttackType ATTACK_TYPE;
}