using Retro.ThirdPersonCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unit;

public class UnitFightData
{
	const int INFINITE_EFFECT = 999;
	readonly Unit parent;
	public Sprite sprite;
	public readonly BaseStats baseStats;
	public CurrentStats currentStats;
    public List<TraitStruct> traitList = new();

	public List<Tuple<TraitStruct, int>> temporaryEffects = new();

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


	public void TakeDamage(int baseDamage, AttackType typeOfDamage, Unit attacker)
	{
		int updatedDamage = CalculateDamage(baseDamage, typeOfDamage);

        currentStats.CURRENT_HP -= updatedDamage;

		if(currentStats.CURRENT_HP > 0)
		{
			if (attacker && attacker.FightData.ContainsEnabledTrait(TraitsEnum.Poisoned_Tip, out int levelPoison))
				temporaryEffects.Add(new(
					new(TraitsEnum.Poisoned_Tip, TraitText.GetConvertedText(TraitsEnum.Poisoned_Tip.ToString()), levelPoison, true),
					TraitStruct.GetBonus(TraitsEnum.Poisoned_Tip, levelPoison, StatsType.Timer)
                    ));

			return;
		}

		//Dead, we check for resurrection effects

		if (ContainsEnabledTrait(TraitsEnum.Second_Wind, out int levelSecondWind))
		{
			currentStats.CURRENT_HP = TraitStruct.GetBonus(TraitsEnum.Second_Wind, levelSecondWind, StatsType.HP, baseStats.HP);
			DisableTrait(TraitsEnum.Second_Wind);
		}
	}

	public bool IsDead()
	{
		return currentStats.CURRENT_HP <= 0;
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

	public async void StartOfTurnEffects()
    {
		ResetMovement();

		if (ContainsEnabledTrait(TraitsEnum.Regeneration, out int levelRegen))
			Heal(TraitStruct.GetBonus(TraitsEnum.Regeneration, levelRegen, StatsType.HP, baseStats.HP));

        // Create a new list to store updated values
        var updatedEffects = new List<Tuple<TraitStruct, int>>();

        foreach (var effect in temporaryEffects)
        {
            switch (effect.Item1.traitEnum)
            {
                case TraitsEnum.Poisoned_Tip:
					var animationCompleted = parent.FightManager.MakeUnitTakeDamage(parent, TraitStruct.GetBonus(effect.Item1.traitEnum, effect.Item1.level, StatsType.Attack, currentStats.MAXIMUM_HP), AttackType.Arcane);
					await animationCompleted;
                    break;
            }
			//We dont update the timer for infinite effects
			if (effect.Item2 == INFINITE_EFFECT)
				continue;

			//Update timer, if timer runs out then we remove from the list of temporary effects
			int timer = effect.Item2 - 1;
			if(timer > 0)
                updatedEffects.Add(new Tuple<TraitStruct, int>(effect.Item1, timer)); 
        }

        temporaryEffects = updatedEffects;
    }

    public void LoadTraits(List<Traits> traitFromJSON)
    {
        foreach (var trait in traitFromJSON)
        {
            TraitsEnum traitEnum = (TraitsEnum)Enum.Parse(typeof(TraitsEnum), trait.Name);
			traitList.Add(new TraitStruct(traitEnum, trait.Name, trait.Level, true));
		}
	}

	public bool ContainsEnabledTrait(TraitsEnum traitEnum, out int level)
	{
		try
		{
			TraitStruct traitToSearch = traitList.Find(t => t.traitEnum == traitEnum && t.enabled == true);
			level = traitToSearch.level;
			return level != 0;
		}
		catch
		{
			level = -1;
			return false;
		}
	}

	public void DisableTrait(TraitsEnum traitEnum)
	{
		int traitToDisable = traitList.FindIndex(t => t.traitEnum == traitEnum && t.enabled == true);
		if(traitToDisable != -1)
		{
			TraitStruct trait = traitList[traitToDisable];
			trait.enabled = false;
			traitList[traitToDisable] = trait;
		}
	}
	public int CalculateDamage(int incomingDamage, AttackType attackType)
	{
		if (ContainsEnabledTrait(TraitsEnum.Overload, out int levelOverload))
			incomingDamage = TraitStruct.GetBonus(TraitsEnum.Overload, levelOverload, StatsType.HP, incomingDamage);

		incomingDamage = ApplyArmor(incomingDamage, attackType);

		if(incomingDamage < 0)
			incomingDamage = 0;

		return incomingDamage;
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
		currentStats.FACTION = parent.UnitData.Faction;
		currentStats.ATTACK_TYPE = (AttackType)Enum.Parse(typeof(AttackType), parent.UnitData.AttackType);
	}
	int LoadHp()
	{
		int hp = baseStats.HP;
		if (ContainsEnabledTrait(TraitsEnum.Tanky, out int level))
			hp += TraitStruct.GetBonus(TraitsEnum.Tanky, level, StatsType.HP);
		if (ContainsEnabledTrait(TraitsEnum.Healthy, out int levelHealthy))
			hp += TraitStruct.GetBonus(TraitsEnum.Healthy, levelHealthy, StatsType.HP, baseStats.HP);

		return hp;
	}
	int LoadMovement()
	{
		int movement = parent.UnitData.Stats.Movement;

		if (ContainsEnabledTrait(TraitsEnum.Speedy, out int level))
			movement += TraitStruct.GetBonus(TraitsEnum.Speedy, level);

		return movement;
	}
	int LoadAttack()
	{
		int attack = baseStats.ATTACK;
		if (ContainsEnabledTrait(TraitsEnum.Strong, out int levelStrong))
			attack += TraitStruct.GetBonus(TraitsEnum.Strong, levelStrong, StatsType.Attack, baseStats.ATTACK);
		if (ContainsEnabledTrait(TraitsEnum.Overload, out int levelOverload))
			attack += TraitStruct.GetBonus(TraitsEnum.Overload, levelOverload, StatsType.Attack, baseStats.ATTACK);
		if (ContainsEnabledTrait(TraitsEnum.Plunderer__s_Fortune, out int levelPlunderer))
			attack += TraitStruct.GetBonus(TraitsEnum.Plunderer__s_Fortune, levelPlunderer, StatsType.Attack, parent.FightManager.generalManager.Gold);

		return attack;
	}
	int LoadArmor()
	{
		int armor = baseStats.ARMOR;

		if (ContainsEnabledTrait(TraitsEnum.Tanky, out int levelTanky))
			armor += TraitStruct.GetBonus(TraitsEnum.Tanky, levelTanky, StatsType.Armor);

		return armor;
	}
	int LoadWard()
	{
		int ward = baseStats.WARD;
		if (ContainsEnabledTrait(TraitsEnum.Magic_Defence, out int levelDefence))
			ward += TraitStruct.GetBonus(TraitsEnum.Magic_Defence, levelDefence, StatsType.Ward, baseStats.WARD);

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
		string baseStatText = $"Base {baseStat}";
		int difference = baseStat - currentStat;
		if (difference > 0)
			return $"{baseStatText} - {difference}";
		if(difference < 0)
			return $"{baseStatText} + {Math.Abs(difference)}";

		return baseStatText;
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
	public readonly int FACTION;
	public readonly AttackType ATTACK_TYPE;

	public BaseStats(Unit parent)
    {
		HP = parent.UnitData.Stats.Hp;
		ATTACK = parent.UnitData.Stats.Attack;
		ARMOR = parent.UnitData.Stats.Armor;
		WARD = parent.UnitData.Stats.Ward;
		RANGE = parent.UnitData.Stats.Range;
		MOVEMENT = parent.UnitData.Stats.Movement;
		FACTION = parent.UnitData.Faction;
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
	public int FACTION;
	public AttackType ATTACK_TYPE;
}