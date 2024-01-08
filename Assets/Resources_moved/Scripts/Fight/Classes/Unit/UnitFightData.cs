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
			damage = Trait.GetOverloadBonusDamageTaken(damage, levelOverload);

		if (damage > 0)
			currentStats.CURRENT_HP -= damage;
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
			Heal(Trait.GetRegenerationAmount(currentStats.MAXIMUM_HP, levelRegen));
	}

    public void LoadTraits(List<Traits> traitFromJSON)
    {
        foreach (var trait in traitFromJSON)
        {
            TraitsEnum traitEnum = (TraitsEnum)Enum.Parse(typeof(TraitsEnum), trait.Name);
			string description = string.Empty;
			switch (traitEnum)
			{
				case TraitsEnum.Floaty:
					description = "User ignores terrain condition"; //Implemented
					break;
				case TraitsEnum.Healthy:
					description = "x2 HP"; //Implemented
					break;
				case TraitsEnum.Magic_Defence:
					description = "Increase Ward by 50%"; //Implemented
					break;
				case TraitsEnum.Overload:
					description = "x3 base Attack but x2 damage received"; //Implemented
					break;
				case TraitsEnum.Regeneration:
					description = "Heals 10% of HP at the start of turn"; //Implemented
					break;
				case TraitsEnum.Second_Wind:
					description = "On death revives with 50% HP";
					break;
				case TraitsEnum.Speedy:
					description = "+2 movement"; //Implemented
					break;
				case TraitsEnum.Strong:
					description = "x2 base attack"; //Implemented
					break;
				case TraitsEnum.Tanky:
					description = "+1 attack, +2 hp"; //Implemented
					break;
				case TraitsEnum.Wealthy:
					description = "Drops double gold on death";
					break;
			}
			traitList.Add(new Trait(traitEnum, trait.Name, description, trait.Level));
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
			hp = Trait.GetTankyBonusHp(hp, level);
		if (ContainsTrait(TraitsEnum.Healthy, out int levelHealthy))
			hp += Trait.GetHealthyBonusHp(baseStats.HP, levelHealthy);

		return hp;
	}
	int LoadMovement()
	{
		int movement = parent.UnitData.Stats.Movement;
		if (ContainsTrait(TraitsEnum.Speedy, out int level))
			movement = Trait.GetSpeedyBonus(movement, level);

		return movement;
	}
	int LoadAttack()
	{
		int attack = baseStats.ATTACK;
		if (ContainsTrait(TraitsEnum.Strong, out int levelStrong))
			attack += Trait.GetStrongBonus(baseStats.ATTACK, levelStrong);
		if (ContainsTrait(TraitsEnum.Overload, out int levelOverload))
			attack += Trait.GetOverloadBonus(baseStats.ATTACK, levelOverload);
		if (ContainsTrait(TraitsEnum.Tanky, out int levelTanky))
			attack = Trait.GetTankyBonusAttack(attack, levelTanky);

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
			ward += Trait.GetMagicDefenceBonus(baseStats.WARD, levelDefence);

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