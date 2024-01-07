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
	public int baseHp;
	public int currentHp;
	public int currentMovement;
	public int currentAttack;
	public int currentRange;
    public List<Trait> traitList = new();

    public UnitFightData(Unit parent)
    {
        this.parent = parent;

        Sprite unitSprite = AddressablesManager.LoadResource<Sprite>(AddressablesManager.TypeOfResource.Sprite, parent.UnitData.PortraitName);
		sprite = unitSprite;

		LoadTraits(parent.UnitData.Traits);
		LoadStats();
	}

	public void TakeDamage(int damage)
	{
		if (ContainsTrait(TraitsEnum.Overload, out int levelOverload))
			damage = Trait.GetOverloadBonusDamageTaken(damage, levelOverload);

		if (damage > 0)
			currentHp -= damage;
	}

	public void Heal(int healAmount)
	{
		currentHp += healAmount;
		if(currentHp > baseHp)
			currentHp = baseHp;
	}

	public void ResetMovement()
	{
		currentMovement = LoadMovement();
	}

    public void StartOfTurnEffects()
    {
		ResetMovement();
		if (ContainsTrait(TraitsEnum.Regeneration, out int levelRegen))
			Heal(Trait.GetRegenerationAmount(baseHp, levelRegen));
	}

    public void LoadTraits(List<string> traitFromJSON)
    {
        foreach (var trait in traitFromJSON)
        {
            TraitsEnum traitEnum = (TraitsEnum)Enum.Parse(typeof(TraitsEnum), trait);
			switch (traitEnum)
			{
				case TraitsEnum.Floaty:
					traitList.Add(new Trait(traitEnum, "Floaty", "User ignores terrain condition", 1));
					break;
				case TraitsEnum.Healthy:
					traitList.Add(new Trait(traitEnum, "Healthy", "x2 HP", 1)); //Implemented
					break;
				case TraitsEnum.Magic_Defence:
					traitList.Add(new Trait(traitEnum, "Magic Defence", "Magical damage is halved", 2));
					break;
				case TraitsEnum.Overload:
					traitList.Add(new Trait(traitEnum, "Overload", "x3 base Attack but x2 damage received", 1)); //Implemented, need testing
					break;
				case TraitsEnum.Regeneration:
					traitList.Add(new Trait(traitEnum, "Regeneration", "Heals 10% of HP at the start of turn", 3));
					break;
				case TraitsEnum.Second_Wind:
					traitList.Add(new Trait(traitEnum, "Second Wind", "On death revives with 50% HP", 1));
					break;
				case TraitsEnum.Speedy:
					traitList.Add(new Trait(traitEnum, "Speedy", "+2 movement", 1)); //Implemented
					break;
				case TraitsEnum.Strong:
					traitList.Add(new Trait(traitEnum, "Strong", "x2 base attack", 1)); //Implemented
					break;
				case TraitsEnum.Tanky:
					traitList.Add(new Trait(traitEnum, "Tanky", "+1 attack, +2 hp", 1)); //Implemented
					break;
				case TraitsEnum.Wealthy:
					traitList.Add(new Trait(traitEnum, "Wealthy", "Drops double gold on death", 1));
					break;
			}
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
		baseHp = LoadHp();
		currentHp = baseHp;
		currentMovement = LoadMovement();
		currentAttack = LoadAttack();
		currentRange = parent.UnitData.Stats.Range;
	}
	int LoadHp()
	{
		int hp = parent.UnitData.Stats.Hp;
		if (ContainsTrait(TraitsEnum.Tanky, out int level))
			hp = Trait.GetTankyBonusHp(hp, level);
		if (ContainsTrait(TraitsEnum.Healthy, out int levelHealthy))
			hp += Trait.GetHealthyBonusHp(parent.UnitData.Stats.Hp, levelHealthy);

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
		int attack = parent.UnitData.Stats.Attack;
		if (ContainsTrait(TraitsEnum.Strong, out int levelStrong))
			attack += Trait.GetStrongBonus(parent.UnitData.Stats.Attack, levelStrong);
		if (ContainsTrait(TraitsEnum.Overload, out int levelOverload))
			attack += Trait.GetOverloadBonus(parent.UnitData.Stats.Attack, levelOverload);
		if (ContainsTrait(TraitsEnum.Tanky, out int levelTanky))
			attack = Trait.GetTankyBonusAttack(attack, levelTanky);

		return attack;
	}
}